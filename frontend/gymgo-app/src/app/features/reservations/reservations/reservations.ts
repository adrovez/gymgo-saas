import {
  Component,
  OnInit,
  inject,
  signal,
  computed,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NgClass } from '@angular/common';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { ClassService } from '../../classes/services/class.service';
import {
  ClassScheduleDto,
  DAY_OF_WEEK_LABELS,
  CALENDAR_DAYS,
} from '../../classes/models/class.models';
import { MemberService } from '../../members/services/member.service';
import { MemberSummaryDto, MemberStatus } from '../../members/models/member.models';
import { ReservationService } from '../services/reservation.service';
import { DialogService } from '../../../core/services/dialog.service';
import {
  ClassReservationDto,
  ReservationStatus,
  RESERVATION_STATUS_LABELS,
  RESERVATION_STATUS_CSS,
} from '../models/reservation.models';

/** Datos de un día en la vista de semana. */
export interface WeekDay {
  dayOfWeek: number;   // 0=Dom, 1=Lun … 6=Sáb
  date: Date;
  dateStr: string;     // "YYYY-MM-DD"
  shortLabel: string;  // "Lun", "Mar" …
  dayNum: number;      // día del mes
  isToday: boolean;
  isPast: boolean;
}

@Component({
  selector: 'app-reservations',
  standalone: true,
  imports: [FormsModule, NgClass],
  templateUrl: './reservations.html',
})
export class ReservationsComponent implements OnInit {
  private readonly classService       = inject(ClassService);
  private readonly memberService      = inject(MemberService);
  private readonly reservationService = inject(ReservationService);
  private readonly dialog             = inject(DialogService);

  // ── Horarios disponibles ──────────────────────────────────────────────────
  readonly schedules        = signal<ClassScheduleDto[]>([]);
  readonly schedulesLoading = signal(false);

  // ── Semana actual ─────────────────────────────────────────────────────────
  readonly currentWeekStart = signal<Date>(this.mondayOf(new Date()));

  readonly weekDays = computed<WeekDay[]>(() => {
    const monday = this.currentWeekStart();
    const todayMs = this.startOfDay(new Date()).getTime();
    const SHORT = ['Dom', 'Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb'];

    return CALENDAR_DAYS.map((dayOfWeek, idx) => {
      const d = new Date(monday);
      d.setDate(monday.getDate() + idx);
      const dateStr = this.toDateStr(d);
      const dMs = d.getTime();
      return {
        dayOfWeek,
        date: d,
        dateStr,
        shortLabel: SHORT[dayOfWeek],
        dayNum: d.getDate(),
        isToday: dMs === todayMs,
        isPast: dMs < todayMs,
      };
    });
  });

  readonly schedulesByDay = computed(() => {
    const map = new Map<number, ClassScheduleDto[]>();
    for (const s of this.schedules()) {
      const list = map.get(s.dayOfWeek) ?? [];
      list.push(s);
      map.set(s.dayOfWeek, [...list].sort((a, b) => a.startTime.localeCompare(b.startTime)));
    }
    return map;
  });

  readonly weekLabel = computed(() => {
    const days = this.weekDays();
    if (!days.length) return '';
    const first = days[0].date;
    const last  = days[6].date;
    const f = first.toLocaleDateString('es-CL', { day: 'numeric', month: 'short' });
    const l = last.toLocaleDateString('es-CL', { day: 'numeric', month: 'short', year: 'numeric' });
    return `${f} — ${l}`;
  });

  readonly isCurrentWeek = computed(() =>
    this.currentWeekStart().getTime() === this.mondayOf(new Date()).getTime()
  );

  // ── Sesión seleccionada ───────────────────────────────────────────────────
  readonly selectedScheduleId = signal<string>('');
  readonly selectedDate        = signal<string>('');

  readonly selectedSchedule = computed(() =>
    this.schedules().find(s => s.id === this.selectedScheduleId()) ?? null
  );

  // ── Reservas de la sesión ─────────────────────────────────────────────────
  readonly reservations        = signal<ClassReservationDto[]>([]);
  readonly reservationsLoading = signal(false);

  readonly activeReservations = computed(() =>
    this.reservations().filter(r => r.status === ReservationStatus.Active)
  );

  readonly effectiveCapacity = computed(() =>
    this.selectedSchedule()?.maxCapacity ?? null
  );

  readonly availableSlots = computed(() => {
    const cap = this.effectiveCapacity();
    if (cap === null) return null;
    return Math.max(0, cap - this.activeReservations().length);
  });

  // ── Nueva reserva ─────────────────────────────────────────────────────────
  readonly showCreateForm = signal(false);
  readonly searchQuery    = signal('');
  readonly searchResults  = signal<MemberSummaryDto[]>([]);
  readonly searchLoading  = signal(false);
  readonly showDropdown   = signal(false);
  readonly selectedMember = signal<MemberSummaryDto | null>(null);
  readonly newNotes       = signal('');
  readonly creating       = signal(false);
  readonly createError    = signal<string | null>(null);

  // ── Constantes ────────────────────────────────────────────────────────────
  readonly MemberStatus            = MemberStatus;
  readonly ReservationStatus       = ReservationStatus;
  readonly RESERVATION_STATUS_LABELS = RESERVATION_STATUS_LABELS;
  readonly RESERVATION_STATUS_CSS    = RESERVATION_STATUS_CSS;

  readonly canCreate = computed(() =>
    !!this.selectedScheduleId() &&
    !!this.selectedDate() &&
    !!this.selectedMember() &&
    !this.creating() &&
    this.canReserveForDate(this.selectedDate())
  );

  private readonly searchSubject = new Subject<string>();

  constructor() {
    this.searchSubject
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntilDestroyed())
      .subscribe((q) => this.doSearch(q));
  }

  ngOnInit(): void {
    this.loadSchedules();
  }

  // ── Semana ────────────────────────────────────────────────────────────────

  prevWeek(): void {
    const d = new Date(this.currentWeekStart());
    d.setDate(d.getDate() - 7);
    this.currentWeekStart.set(d);
    this.resetSession();
  }

  nextWeek(): void {
    const d = new Date(this.currentWeekStart());
    d.setDate(d.getDate() + 7);
    this.currentWeekStart.set(d);
    this.resetSession();
  }

  goToToday(): void {
    this.currentWeekStart.set(this.mondayOf(new Date()));
    this.resetSession();
  }

  resetSession(): void {
    this.selectedScheduleId.set('');
    this.selectedDate.set('');
    this.reservations.set([]);
    this.hideCreateForm();
  }

  // ── Selección de clase ────────────────────────────────────────────────────

  selectClass(schedule: ClassScheduleDto, dateStr: string): void {
    // Toggle off si se hace click en el mismo bloque
    if (this.selectedScheduleId() === schedule.id && this.selectedDate() === dateStr) {
      this.resetSession();
      return;
    }
    this.selectedScheduleId.set(schedule.id);
    this.selectedDate.set(dateStr);
    this.loadReservations();
    this.hideCreateForm();
  }

  isSelected(scheduleId: string, dateStr: string): boolean {
    return this.selectedScheduleId() === scheduleId && this.selectedDate() === dateStr;
  }

  canReserveForDate(dateStr: string): boolean {
    if (!dateStr) return false;
    const [y, m, d] = dateStr.split('-').map(Number);
    return new Date(y, m - 1, d) >= this.startOfDay(new Date());
  }

  // ── Horarios ──────────────────────────────────────────────────────────────

  loadSchedules(): void {
    this.schedulesLoading.set(true);
    this.classService.getWeeklySchedule().subscribe({
      next:  (s) => { this.schedules.set(s.filter(sc => sc.isActive)); this.schedulesLoading.set(false); },
      error: ()  => this.schedulesLoading.set(false),
    });
  }

  // ── Reservas ──────────────────────────────────────────────────────────────

  loadReservations(): void {
    const sid  = this.selectedScheduleId();
    const date = this.selectedDate();
    if (!sid || !date) return;

    this.reservationsLoading.set(true);
    this.reservationService.getReservationsBySession(sid, date).subscribe({
      next:  (r) => { this.reservations.set(r); this.reservationsLoading.set(false); },
      error: ()  => this.reservationsLoading.set(false),
    });
  }

  // ── Cancelar reserva ──────────────────────────────────────────────────────

  async cancelReservation(reservation: ClassReservationDto): Promise<void> {
    const result = await this.dialog.confirmAction(
      `¿Cancelar la reserva de ${reservation.memberFullName}?`,
      'Esta acción no se puede deshacer.',
      'Sí, cancelar',
    );
    if (!result.isConfirmed) return;

    this.reservationService
      .cancelReservation(reservation.id, {
        cancelStatus: ReservationStatus.CancelledByStaff,
        reason:       null,
      })
      .subscribe({
        next: () => {
          this.dialog.toast('Reserva cancelada', 'success');
          this.loadReservations();
        },
        error: (err) => {
          const detail: string = err?.error?.detail ?? 'No se pudo cancelar la reserva.';
          this.dialog.toast(detail, 'error');
        },
      });
  }

  // ── Nueva reserva ─────────────────────────────────────────────────────────

  showNewReservationForm(): void {
    this.showCreateForm.set(true);
    this.createError.set(null);
  }

  hideCreateForm(): void {
    this.showCreateForm.set(false);
    this.selectedMember.set(null);
    this.searchQuery.set('');
    this.searchResults.set([]);
    this.newNotes.set('');
    this.createError.set(null);
  }

  onSearchInput(value: string): void {
    this.searchQuery.set(value);
    if (value.trim().length < 2) {
      this.searchResults.set([]);
      this.showDropdown.set(false);
      return;
    }
    this.searchSubject.next(value.trim());
  }

  private doSearch(query: string): void {
    this.searchLoading.set(true);
    this.memberService
      .getMembers({ search: query, page: 1, pageSize: 8 })
      .subscribe({
        next: (res) => {
          this.searchResults.set(res.items);
          this.showDropdown.set(res.items.length > 0);
          this.searchLoading.set(false);
        },
        error: () => this.searchLoading.set(false),
      });
  }

  selectMember(member: MemberSummaryDto): void {
    this.selectedMember.set(member);
    this.searchQuery.set(member.fullName);
    this.showDropdown.set(false);
    this.createError.set(null);
  }

  closeDropdown(): void {
    setTimeout(() => this.showDropdown.set(false), 150);
  }

  createReservation(): void {
    const member = this.selectedMember();
    const sid    = this.selectedScheduleId();
    const date   = this.selectedDate();
    if (!member || !sid || !date) return;

    this.creating.set(true);
    this.createError.set(null);

    this.reservationService
      .createReservation({
        memberId:        member.id,
        classScheduleId: sid,
        sessionDate:     date,
        notes:           this.newNotes().trim() || null,
      })
      .subscribe({
        next: () => {
          this.creating.set(false);
          this.dialog.toast(`✓ Reserva creada para ${member.fullName}`, 'success');
          this.hideCreateForm();
          this.loadReservations();
        },
        error: (err) => {
          this.creating.set(false);
          const detail: string = err?.error?.detail ?? 'No se pudo crear la reserva.';
          this.createError.set(detail);
        },
      });
  }

  // ── Helpers ───────────────────────────────────────────────────────────────

  formatTime(iso: string): string {
    return new Date(iso).toLocaleTimeString('es-CL', { hour: '2-digit', minute: '2-digit' });
  }

  formatSessionDate(isoDate: string): string {
    const [y, m, d] = isoDate.split('-').map(Number);
    return new Date(y, m - 1, d).toLocaleDateString('es-CL', {
      weekday: 'long', day: 'numeric', month: 'long', year: 'numeric',
    });
  }

  classColor(schedule: ClassScheduleDto): string {
    return schedule.gymClassColor ?? '#3B82F6';
  }

  /** Devuelve el mes abreviado del primer día de la semana visible */
  weekMonthLabel(): string {
    const days = this.weekDays();
    if (!days.length) return '';
    return days[0].date.toLocaleDateString('es-CL', { month: 'long', year: 'numeric' });
  }

  // ── Utilidades de fecha ───────────────────────────────────────────────────

  private mondayOf(d: Date): Date {
    const result = new Date(d);
    result.setHours(0, 0, 0, 0);
    const dow = result.getDay();
    result.setDate(result.getDate() - (dow === 0 ? 6 : dow - 1));
    return result;
  }

  private startOfDay(d: Date): Date {
    const r = new Date(d);
    r.setHours(0, 0, 0, 0);
    return r;
  }

  private toDateStr(d: Date): string {
    const y = d.getFullYear();
    const m = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${y}-${m}-${day}`;
  }
}
