import { Component, OnInit, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MemberService } from '../services/member.service';
import { MembershipAssignmentService } from '../../assignments/services/membership-assignment.service';
import { MembershipAssignmentSummaryDto, AssignmentStatus, PaymentStatus } from '../../assignments/models/membership-assignment.models';
import { MemberDto } from '../models/member.models';

@Component({
  selector: 'app-member-assignments',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './member-assignments.html',
})
export class MemberAssignmentsComponent implements OnInit {
  private readonly memberService     = inject(MemberService);
  private readonly assignmentService = inject(MembershipAssignmentService);

  /** Enlazado desde la ruta /members/:id/assignments via withComponentInputBinding() */
  readonly id = input.required<string>();

  readonly loadingMember      = signal(true);
  readonly loadingAssignments = signal(true);
  readonly error              = signal<string | null>(null);

  readonly member      = signal<MemberDto | null>(null);
  readonly assignments = signal<MembershipAssignmentSummaryDto[]>([]);

  readonly AssignmentStatus = AssignmentStatus;
  readonly PaymentStatus    = PaymentStatus;

  ngOnInit(): void {
    // Cargar datos del socio y su historial en paralelo
    this.memberService.getMemberById(this.id()).subscribe({
      next: (m) => {
        this.member.set(m);
        this.loadingMember.set(false);
      },
      error: () => {
        this.error.set('No se pudo cargar el socio.');
        this.loadingMember.set(false);
      },
    });

    this.assignmentService.getMemberAssignments(this.id()).subscribe({
      next: (result) => {
        // Ordenar por fecha de inicio descendente (más reciente primero)
        const sorted = [...result].sort(
          (a, b) => b.startDate.localeCompare(a.startDate),
        );
        this.assignments.set(sorted);
        this.loadingAssignments.set(false);
      },
      error: () => {
        this.error.set('No se pudo cargar el historial de membresías.');
        this.loadingAssignments.set(false);
      },
    });
  }

  isLoading(): boolean {
    return this.loadingMember() || this.loadingAssignments();
  }

  formatAmount(amount: number): string {
    return new Intl.NumberFormat('es-CL', { style: 'currency', currency: 'CLP' }).format(amount);
  }

  shortId(id: string): string {
    return `…${id.slice(-8)}`;
  }
}
