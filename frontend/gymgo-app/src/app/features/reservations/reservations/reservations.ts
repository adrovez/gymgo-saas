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
import { ClassScheduleDto, DAY_OF_WEEK_LABELS } from '../../classes/models/class.models';
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

  readonly effectiveCapacity = computed(() => {
    const s = this.selectedSchedule();
    return s?.maxCapacity ?? null;
  });

  readonly availableSlots = computed(() => {
    const cap = this.effectiveCapacity();
    if (cap === null) return null;
    return cap - this.activeReservations().length;
  });

  // ── Nueva reserva ─────────────────────────────────────────────────────────
  readonly showCreateForm   = signal(false);
  readonly searchQuery      = signal('');
  readonly searchResults    = signal<MemberSummaryDto[]>([]);
  readonly searchLoading    = signal(false);
  readonly showDropdown     = signal(false);
  readonly selectedMember   = signal<MemberSummaryDto | null>(null);
  readonly newNotes         = signal('');
  readonly creating         = signal(false);
  readonly createError      = signal<string | null>(null);

  // ── Constantes ────────────────────────────────────────────────────────────
  readonly ReservationStatus      = ReservationStatus;
  readonly RESERVATION_STATUS_LABELS = RESERVATION_STATUS_LABELS;
  readonly RESERVATION_STATUS_CSS    = RESERVATION_STATUS_CSS;
  readonly DAY_OF_WEEK_LABELS        = DAY_OF_WEEK_LABELS;

  readonly canCreate = computed(() =>
    !!this.selectedScheduleId() &&
    !!this.selectedDate() &&
    !!this.selectedMember() &&
    !this.creating()
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

  // ── Horarios ──────────────────────────────────────────────────────────────

  loadSchedules(): void {
    this.schedulesLoading.set(true);
    this.classService.getWeeklySchedule().subscribe({
      next:  (s) => { this.schedules.set(s.filter(sc => sc.isActive)); this.schedulesLoading.set(false); },
      error: ()  => this.schedulesLoading.set(false),
    });
  }

  onScheduleChange(id: string): void {
    this.selectedScheduleId.set(id);
    this.selectedDate.set('');
    this.reservations.set([]);
    this.hideCreateForm();
  }

  onDateChange(date: string): void {
    this.selectedDate.set(date);
    if (date && this.selectedScheduleId()) {
      this.loadReservations();
    } else {
      this.reservations.set([]);
    }
    this.hideCreateForm();
  }

  scheduleDateMin(): string {
    // Fechas a partir de hoy
    return new Date().toISOString().substring(0, 10);
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
          const detail: string =
            err?.error?.detail ?? 'No se pudo cancelar la reserva.';
          this.dialog.toast(detail, 'error');
        },
      });
  }

  // ── Nueva reserva — formulario ────────────────────────────────────────────

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
          const detail: string =
            err?.error?.detail ?? 'No se pudo crear la reserva.';
          this.createError.set(detail);
        },
      });
  }

  // ── Helpers ───────────────────────────────────────────────────────────────

  formatTime(iso: string): string {
    return new Date(iso).toLocaleTimeString('es-CL', {
      hour: '2-digit', minute: '2-digit',
    });
  }

  formatDate(isoDate: string): string {
    const [y, m, d] = isoDate.split('-').map(Number);
    return new Date(y, m - 1, d).toLocaleDateString('es-CL', {
      weekday: 'long', year: 'numeric', month: 'long', day: 'numeric',
    });
  }

  scheduleLabel(s: ClassScheduleDto): string {
    return `${s.gymClassName} — ${DAY_OF_WEEK_LABELS[s.dayOfWeek]} ${s.startTime}–${s.endTime}`;
  }

  readonly MemberStatus = MemberStatus;
}
