import {
  Component,
  OnInit,
  inject,
  signal,
  computed,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { MemberService } from '../../members/services/member.service';
import { MemberSummaryDto, MemberStatus } from '../../members/models/member.models';
import { MembershipAssignmentService } from '../../assignments/services/membership-assignment.service';
import {
  MembershipAssignmentDto,
  AssignmentStatus,
  PaymentStatus,
} from '../../assignments/models/membership-assignment.models';
import { GymEntryService } from '../services/gym-entry.service';
import { DialogService } from '../../../core/services/dialog.service';
import {
  GymEntryMethod,
  GymEntryDto,
  GYM_ENTRY_METHOD_OPTIONS,
} from '../models/gym-entry.models';

@Component({
  selector: 'app-gym-entry',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './gym-entry.html',
})
export class GymEntryComponent implements OnInit {
  private readonly memberService     = inject(MemberService);
  private readonly assignmentService = inject(MembershipAssignmentService);
  private readonly entryService      = inject(GymEntryService);
  private readonly dialog            = inject(DialogService);

  // ── Búsqueda ──────────────────────────────────────────────────────────
  readonly searchQuery    = signal('');
  readonly searchResults  = signal<MemberSummaryDto[]>([]);
  readonly searchLoading  = signal(false);
  readonly showDropdown   = signal(false);

  // ── Socio seleccionado ────────────────────────────────────────────────
  readonly selectedMember    = signal<MemberSummaryDto | null>(null);
  readonly activeAssignment  = signal<MembershipAssignmentDto | null>(null);
  readonly assignmentLoading = signal(false);

  // ── Acción de registro ────────────────────────────────────────────────
  readonly selectedMethod = signal<GymEntryMethod>(GymEntryMethod.Manual);
  readonly notes          = signal('');
  readonly registering    = signal(false);
  readonly lastError      = signal<string | null>(null);
  readonly lastSuccess    = signal<string | null>(null);

  // ── Log del día ────────────────────────────────────────────────────────
  readonly todayEntries   = signal<GymEntryDto[]>([]);
  readonly entriesLoading = signal(false);
  readonly todayCount     = computed(() => this.todayEntries().length);

  // ── Registro de salida ────────────────────────────────────────────────
  /** Set de IDs de entradas con salida en proceso (para deshabilitar botón) */
  readonly registeringExitIds = signal<Set<string>>(new Set());

  // ── Constantes ────────────────────────────────────────────────────────
  readonly MemberStatus    = MemberStatus;
  readonly AssignmentStatus = AssignmentStatus;
  readonly PaymentStatus   = PaymentStatus;
  readonly GymEntryMethod  = GymEntryMethod;
  readonly methodOptions   = GYM_ENTRY_METHOD_OPTIONS;

  /**
   * Verdadero si el socio seleccionado ya tiene un ingreso registrado hoy.
   * Usa los datos del log local (sin llamada extra al API).
   */
  readonly alreadyEnteredToday = computed(() => {
    const member = this.selectedMember();
    if (!member) return false;
    return this.todayEntries().some(e => e.memberId === member.id);
  });

  readonly canRegister = computed(() =>
    this.selectedMember() !== null &&
    this.selectedMember()!.status === MemberStatus.Active &&
    !this.registering() &&
    !this.assignmentLoading() &&
    !this.alreadyEnteredToday()
  );

  private readonly searchSubject = new Subject<string>();
  private clearTimer: ReturnType<typeof setTimeout> | null = null;

  constructor() {
    this.searchSubject
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntilDestroyed())
      .subscribe((q) => this.doSearch(q));
  }

  ngOnInit(): void {
    this.loadTodayEntries();
  }

  // ── Log del día ────────────────────────────────────────────────────────

  loadTodayEntries(): void {
    this.entriesLoading.set(true);
    this.entryService.getEntriesByDate().subscribe({
      next:  (entries) => { this.todayEntries.set(entries); this.entriesLoading.set(false); },
      error: ()        => this.entriesLoading.set(false),
    });
  }

  // ── Búsqueda ──────────────────────────────────────────────────────────

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
    this.showDropdown.set(false);
    this.searchQuery.set(member.fullName);
    this.lastError.set(null);
    this.lastSuccess.set(null);
    this.cancelClearTimer();
    this.loadActiveAssignment(member.id);
  }

  closeDropdown(): void {
    setTimeout(() => this.showDropdown.set(false), 150);
  }

  clearSelection(): void {
    this.cancelClearTimer();
    this.selectedMember.set(null);
    this.activeAssignment.set(null);
    this.searchQuery.set('');
    this.searchResults.set([]);
    this.lastError.set(null);
    this.lastSuccess.set(null);
    this.notes.set('');
    this.selectedMethod.set(GymEntryMethod.Manual);
  }

  private cancelClearTimer(): void {
    if (this.clearTimer !== null) {
      clearTimeout(this.clearTimer);
      this.clearTimer = null;
    }
  }

  // ── Membresía activa ──────────────────────────────────────────────────

  private loadActiveAssignment(memberId: string): void {
    this.assignmentLoading.set(true);
    this.activeAssignment.set(null);
    this.assignmentService.getActiveAssignment(memberId).subscribe({
      next:  (a) => { this.activeAssignment.set(a); this.assignmentLoading.set(false); },
      error: ()  => this.assignmentLoading.set(false),
    });
  }

  // ── Registro de ingreso ───────────────────────────────────────────────

  registerEntry(): void {
    const member = this.selectedMember();
    if (!member) return;

    this.registering.set(true);
    this.lastError.set(null);
    this.lastSuccess.set(null);

    this.entryService
      .registerEntry({
        memberId: member.id,
        method:   this.selectedMethod(),
        notes:    this.notes().trim() || null,
      })
      .subscribe({
        next: (res) => {
          this.registering.set(false);
          this.lastSuccess.set(`¡Bienvenido/a, ${member.fullName}!`);
          this.dialog.toast(`✓ Ingreso registrado — ${member.fullName}`, 'success');

          const nowIso = new Date().toISOString();
          const newEntry: GymEntryDto = {
            id:                     res.id,
            memberId:               member.id,
            memberFullName:         member.fullName,
            membershipAssignmentId: '',
            entryDate:              nowIso.substring(0, 10),
            enteredAtUtc:           nowIso,
            exitedAtUtc:            null,
            method:                 GYM_ENTRY_METHOD_OPTIONS.find(
                                      (o) => o.value === this.selectedMethod()
                                    )?.label ?? 'Manual',
            notes:                  this.notes().trim() || null,
            createdAtUtc:           nowIso,
          };
          this.todayEntries.update((list) => [newEntry, ...list]);

          // ── Mejora 3: auto-limpiar el formulario después de 2.5s ─────────
          this.clearTimer = setTimeout(() => this.clearSelection(), 2500);
        },
        error: (err) => {
          this.registering.set(false);
          const detail: string =
            err?.error?.detail ?? 'No se pudo registrar el ingreso. Intenta nuevamente.';
          this.lastError.set(detail);
        },
      });
  }

  // ── Registro de salida ────────────────────────────────────────────────

  registerExit(entry: GymEntryDto): void {
    // Evitar doble click
    const current = this.registeringExitIds();
    if (current.has(entry.id)) return;

    this.registeringExitIds.update(ids => new Set([...ids, entry.id]));

    this.entryService.registerExit(entry.id).subscribe({
      next: () => {
        const exitedAt = new Date().toISOString();
        this.todayEntries.update(list =>
          list.map(e => e.id === entry.id ? { ...e, exitedAtUtc: exitedAt } : e)
        );
        this.registeringExitIds.update(ids => {
          const next = new Set(ids);
          next.delete(entry.id);
          return next;
        });
        this.dialog.toast(`✓ Salida registrada — ${entry.memberFullName}`, 'success');
      },
      error: (err) => {
        this.registeringExitIds.update(ids => {
          const next = new Set(ids);
          next.delete(entry.id);
          return next;
        });
        const detail: string =
          err?.error?.detail ?? 'No se pudo registrar la salida. Intenta nuevamente.';
        this.dialog.toast(`✗ ${detail}`, 'error');
      },
    });
  }

  isRegisteringExit(entryId: string): boolean {
    return this.registeringExitIds().has(entryId);
  }

  // ── Helpers ───────────────────────────────────────────────────────────

  formatTime(isoString: string): string {
    return new Date(isoString).toLocaleTimeString('es-CL', {
      hour: '2-digit', minute: '2-digit',
    });
  }

  today(): string {
    return new Date().toLocaleDateString('es-CL', {
      weekday: 'long', year: 'numeric', month: 'long', day: 'numeric',
    });
  }
}
