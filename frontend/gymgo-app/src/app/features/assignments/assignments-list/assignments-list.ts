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

  // ── Membresías morosas ────────────────────────────────────────────────────
  readonly overdueLoading = signal(false);
  readonly overdueError   = signal<string | null>(null);
  readonly overdue        = signal<MembershipAssignmentSummaryDto[]>([]);

  // ── Búsqueda ──────────────────────────────────────────────────────────────
  readonly searchQuery    = signal('');
  readonly searchLoading  = signal(false);
  readonly searchResults  = signal<MembershipAssignmentSummaryDto[]>([]);
  readonly searchTouched  = signal(false);   // true tras la primera búsqueda

  readonly hasSearchQuery = computed(() => this.searchQuery().trim().length >= 2);

  // ── Constantes ────────────────────────────────────────────────────────────
  readonly AssignmentStatus       = AssignmentStatus;
  readonly PaymentStatus          = PaymentStatus;
  readonly ASSIGNMENT_STATUS_LABELS = ASSIGNMENT_STATUS_LABELS;

  private readonly searchSubject = new Subject<string>();

  constructor() {
    this.searchSubject
      .pipe(debounceTime(350), distinctUntilChanged(), takeUntilDestroyed())
      .subscribe((q) => this.doSearch(q));
  }

  ngOnInit(): void {
    this.loadOverdue();
  }

  // ── Morosas ───────────────────────────────────────────────────────────────

  loadOverdue(): void {
    this.overdueLoading.set(true);
    this.overdueError.set(null);

    this.assignmentService.getOverdueAssignments().subscribe({
      next: (result) => {
        this.overdue.set(result);
        this.overdueLoading.set(false);
      },
      error: () => {
        this.overdueError.set('No se pudo cargar el listado de membresías morosas.');
        this.overdueLoading.set(false);
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
        this.loadOverdue();
        // Refrescar resultados de búsqueda si hay una búsqueda activa
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
        this.loadOverdue();
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
