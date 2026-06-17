import {
  Component,
  OnInit,
  inject,
  signal,
  computed,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import {
  MembershipAssignmentSummaryDto,
  AssignmentStatus,
  PaymentStatus,
  ASSIGNMENT_STATUS_LABELS,
} from '../models/membership-assignment.models';
import { MembershipAssignmentService } from '../services/membership-assignment.service';
import { DialogService } from '../../../core/services/dialog.service';

@Component({
  selector: 'app-assignments-list',
  standalone: true,
  imports: [RouterLink, FormsModule],
  templateUrl: './assignments-list.html',
})
export class AssignmentsListComponent implements OnInit {
  private readonly assignmentService = inject(MembershipAssignmentService);
  private readonly dialog            = inject(DialogService);

  // ── Membresías por vencer / vencidas ─────────────────────────────────────
  readonly expiringLoading    = signal(false);
  readonly expiringError      = signal<string | null>(null);
  readonly expiringSoon       = signal<MembershipAssignmentSummaryDto[]>([]);
  readonly recentlyExpired    = signal<MembershipAssignmentSummaryDto[]>([]);

  // ── Búsqueda ──────────────────────────────────────────────────────────────
  readonly searchQuery    = signal('');
  readonly searchLoading  = signal(false);
  readonly searchResults  = signal<MembershipAssignmentSummaryDto[]>([]);
  readonly searchTouched  = signal(false);

  readonly hasSearchQuery = computed(() => this.searchQuery().trim().length >= 2);

  // ── Constantes ────────────────────────────────────────────────────────────
  readonly AssignmentStatus         = AssignmentStatus;
  readonly PaymentStatus            = PaymentStatus;
  readonly ASSIGNMENT_STATUS_LABELS = ASSIGNMENT_STATUS_LABELS;

  private readonly searchSubject = new Subject<string>();

  constructor() {
    this.searchSubject
      .pipe(debounceTime(350), distinctUntilChanged(), takeUntilDestroyed())
      .subscribe((q) => this.doSearch(q));
  }

  ngOnInit(): void {
    this.loadExpiring();
  }

  // ── Por vencer / vencidas ─────────────────────────────────────────────────

  loadExpiring(): void {
    this.expiringLoading.set(true);
    this.expiringError.set(null);

    this.assignmentService.getExpiringAssignments().subscribe({
      next: (result) => {
        this.expiringSoon.set(result.expiringSoon);
        this.recentlyExpired.set(result.recentlyExpired);
        this.expiringLoading.set(false);
      },
      error: () => {
        this.expiringError.set('No se pudo cargar el listado de membresías.');
        this.expiringLoading.set(false);
      },
    });
  }

  // ── Búsqueda ──────────────────────────────────────────────────────────────

  onSearchInput(value: string): void {
    this.searchQuery.set(value);
    if (value.trim().length < 2) {
      this.searchResults.set([]);
      this.searchTouched.set(false);
      return;
    }
    this.searchSubject.next(value.trim());
  }

  clearSearch(): void {
    this.searchQuery.set('');
    this.searchResults.set([]);
    this.searchTouched.set(false);
  }

  private doSearch(query: string): void {
    this.searchLoading.set(true);
    this.searchTouched.set(true);
    this.assignmentService.searchAssignments(query).subscribe({
      next: (results) => {
        this.searchResults.set(results);
        this.searchLoading.set(false);
      },
      error: () => {
        this.searchResults.set([]);
        this.searchLoading.set(false);
      },
    });
  }

  // ── Acciones comunes ──────────────────────────────────────────────────────

  async registerPayment(assignment: MembershipAssignmentSummaryDto): Promise<void> {
    const result = await this.dialog.confirmAction(
      'Registrar pago',
      `¿Confirmas el pago de ${this.formatAmount(assignment.amountSnapshot)}? El socio se reactivará automáticamente.`,
      'Sí, registrar pago',
    );
    if (!result.isConfirmed) return;

    this.assignmentService.registerPayment(assignment.id).subscribe({
      next: () => {
        this.dialog.toast('Pago registrado exitosamente', 'success');
        this.loadExpiring();
        if (this.hasSearchQuery()) {
          this.doSearch(this.searchQuery().trim());
        }
      },
      error: () => this.dialog.toast('No se pudo registrar el pago. Intenta nuevamente.', 'error'),
    });
  }

  async cancelAssignment(assignment: MembershipAssignmentSummaryDto): Promise<void> {
    const result = await this.dialog.confirmDanger(
      '¿Cancelar membresía?',
      `Esta acción cancelará la membresía vigente de ${assignment.memberFullName || 'este socio'} hasta el ${assignment.endDate}.`,
      'Sí, cancelar',
    );
    if (!result.isConfirmed) return;

    this.assignmentService.cancelAssignment(assignment.id).subscribe({
      next: () => {
        this.dialog.toast('Membresía cancelada', 'success');
        this.loadExpiring();
        if (this.hasSearchQuery()) {
          this.doSearch(this.searchQuery().trim());
        }
      },
      error: () => this.dialog.toast('No se pudo cancelar la membresía. Intenta nuevamente.', 'error'),
    });
  }

  // ── Helpers de formato ────────────────────────────────────────────────────

  formatAmount(amount: number): string {
    return new Intl.NumberFormat('es-CL', { style: 'currency', currency: 'CLP' }).format(amount);
  }

  /** Días hasta el vencimiento (desde hoy). Retorna 0 si ya venció. */
  daysUntilExpiry(endDate: string): number {
    const today  = new Date();
    today.setHours(0, 0, 0, 0);
    const end    = new Date(endDate + 'T00:00:00');
    const diff   = Math.ceil((end.getTime() - today.getTime()) / 86_400_000);
    return Math.max(diff, 0);
  }

  /** Días desde que venció (desde hoy). Retorna 0 si aún no venció. */
  daysSinceExpiry(endDate: string): number {
    const today  = new Date();
    today.setHours(0, 0, 0, 0);
    const end    = new Date(endDate + 'T00:00:00');
    const diff   = Math.ceil((today.getTime() - end.getTime()) / 86_400_000);
    return Math.max(diff, 0);
  }

  statusBadgeStyle(status: AssignmentStatus): string {
    switch (status) {
      case AssignmentStatus.Active:    return 'badge badge-active';
      case AssignmentStatus.Frozen:    return 'badge badge-suspended';
      case AssignmentStatus.Expired:   return 'badge badge-delinquent';
      case AssignmentStatus.Cancelled: return 'badge';
      default: return 'badge';
    }
  }

  paymentBadgeStyle(status: PaymentStatus): string {
    switch (status) {
      case PaymentStatus.Paid:    return 'badge badge-active';
      case PaymentStatus.Pending: return 'badge badge-suspended';
      case PaymentStatus.Overdue: return 'badge badge-delinquent';
      default: return 'badge';
    }
  }
}
