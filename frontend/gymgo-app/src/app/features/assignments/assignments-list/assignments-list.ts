import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import {
  MembershipAssignmentSummaryDto,
  AssignmentStatus,
  PaymentStatus,
} from '../models/membership-assignment.models';
import { MembershipAssignmentService } from '../services/membership-assignment.service';
import { DialogService } from '../../../core/services/dialog.service';

@Component({
  selector: 'app-assignments-list',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './assignments-list.html',
})
export class AssignmentsListComponent implements OnInit {
  private readonly assignmentService = inject(MembershipAssignmentService);
  private readonly dialog            = inject(DialogService);

  readonly loading  = signal(false);
  readonly error    = signal<string | null>(null);
  readonly overdue  = signal<MembershipAssignmentSummaryDto[]>([]);

  readonly AssignmentStatus = AssignmentStatus;
  readonly PaymentStatus    = PaymentStatus;

  ngOnInit(): void {
    this.loadOverdue();
  }

  loadOverdue(): void {
    this.loading.set(true);
    this.error.set(null);

    this.assignmentService.getOverdueAssignments().subscribe({
      next: (result) => {
        this.overdue.set(result);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('No se pudo cargar el listado de membresías morosas. Intenta nuevamente.');
        this.loading.set(false);
      },
    });
  }

  formatAmount(amount: number): string {
    return new Intl.NumberFormat('es-CL', { style: 'currency', currency: 'CLP' }).format(amount);
  }

  /** Muestra los últimos 8 caracteres del UUID para identificación rápida */
  shortId(id: string): string {
    return `…${id.slice(-8)}`;
  }

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
      },
      error: () => this.error.set('No se pudo registrar el pago. Intenta nuevamente.'),
    });
  }

  async cancelAssignment(assignment: MembershipAssignmentSummaryDto): Promise<void> {
    const result = await this.dialog.confirmDanger(
      '¿Cancelar membresía?',
      `Esta acción cancelará la membresía vigente hasta el ${assignment.endDate}.`,
      'Sí, cancelar',
    );
    if (!result.isConfirmed) return;

    this.assignmentService.cancelAssignment(assignment.id).subscribe({
      next: () => {
        this.dialog.toast('Membresía cancelada', 'success');
        this.loadOverdue();
      },
      error: () => this.error.set('No se pudo cancelar la membresía. Intenta nuevamente.'),
    });
  }
}
