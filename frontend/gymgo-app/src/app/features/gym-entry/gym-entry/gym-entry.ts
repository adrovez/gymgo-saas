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
  readonly todayEntries  = signal<GymEntryDto[]>([]);
  readonly entriesLoading = signal(false);
  readonly todayCount    = computed(() => this.todayEntries().length);

  // ── Constantes ────────────────────────────────────────────────────────
  readonly MemberStatus    = MemberStatus;
  readonly AssignmentStatus = AssignmentStatus;
  readonly PaymentStatus   = PaymentStatus;
  readonly GymEntryMethod  = GymEntryMethod;
  readonly methodOptions   = GYM_ENTRY_METHOD_OPTIONS;

  readonly canRegister = computed(() =>
    this.selectedMember() !== null &&
    this.selectedMember()!.status === MemberStatus.Active &&
    !this.registering() &&
    !this.assignmentLoading()
  );

  private readonly searchSubject = new Subject<string>();

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
    this.loadActiveAssignment(member.id);
  }

  closeDropdown(): void {
    setTimeout(() => this.showDropdown.set(false), 150);
  }

  clearSelection(): void {
    this.selectedMember.set(null);
    this.activeAssignment.set(null);
    this.searchQuery.set('');
    this.searchResults.set([]);
    this.lastError.set(null);
    this.lastSuccess.set(null);
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

  // ── Registro ──────────────────────────────────────────────────────────

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
            method:                 GYM_ENTRY_METHOD_OPTIONS.find(
                                      (o) => o.value === this.selectedMethod()
                                    )?.label ?? 'Manual',
            notes:                  this.notes().trim() || null,
            createdAtUtc:           nowIso,
          };
          this.todayEntries.update((list) => [newEntry, ...list]);
          this.notes.set('');
        },
        error: (err) => {
          this.registering.set(false);
          const detail: string =
            err?.error?.detail ?? 'No se pudo registrar el ingreso. Intenta nuevamente.';
          this.lastError.set(detail);
        },
      });
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
