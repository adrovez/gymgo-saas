import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  MembershipAssignmentDto,
  MembershipAssignmentSummaryDto,
  AssignMembershipPlanRequest,
} from '../models/membership-assignment.models';

@Injectable({ providedIn: 'root' })
export class MembershipAssignmentService {
  private readonly http   = inject(HttpClient);
  private readonly apiUrl = environment.apiUrl;

  // ── Queries por socio ────────────────────────────────────────────────────

  /** GET /api/v1/members/{memberId}/assignments — historial completo del socio */
  getMemberAssignments(memberId: string): Observable<MembershipAssignmentSummaryDto[]> {
    return this.http.get<MembershipAssignmentSummaryDto[]>(
      `${this.apiUrl}/members/${memberId}/assignments`,
    );
  }

  /** GET /api/v1/members/{memberId}/assignments/active — membresía activa o congelada */
  getActiveAssignment(memberId: string): Observable<MembershipAssignmentDto | null> {
    return this.http.get<MembershipAssignmentDto | null>(
      `${this.apiUrl}/members/${memberId}/assignments/active`,
    );
  }

  // ── Queries del tenant ───────────────────────────────────────────────────

  /** GET /api/v1/assignments/overdue — membresías morosas del tenant */
  getOverdueAssignments(): Observable<MembershipAssignmentSummaryDto[]> {
    return this.http.get<MembershipAssignmentSummaryDto[]>(
      `${this.apiUrl}/assignments/overdue`,
    );
  }

  /**
   * GET /api/v1/assignments/search?q=... — buscar membresías por nombre o RUT del socio.
   * Requiere mínimo 2 caracteres.
   */
  searchAssignments(query: string): Observable<MembershipAssignmentSummaryDto[]> {
    return this.http.get<MembershipAssignmentSummaryDto[]>(
      `${this.apiUrl}/assignments/search`,
      { params: { q: query } },
    );
  }

  // ── Comandos ─────────────────────────────────────────────────────────────

  /** POST /api/v1/members/{memberId}/assignments — asignar plan */
  assignMembershipPlan(
    memberId: string,
    request: AssignMembershipPlanRequest,
  ): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(
      `${this.apiUrl}/members/${memberId}/assignments`,
      request,
    );
  }

  /** PATCH /api/v1/assignments/{id}/pay — registrar pago */
  registerPayment(assignmentId: string): Observable<void> {
    return this.http.patch<void>(
      `${this.apiUrl}/assignments/${assignmentId}/pay`,
      {},
    );
  }

  /** PATCH /api/v1/assignments/{id}/overdue — marcar como morosa */
  markOverdue(assignmentId: string): Observable<void> {
    return this.http.patch<void>(
      `${this.apiUrl}/assignments/${assignmentId}/overdue`,
      {},
    );
  }

  /** PATCH /api/v1/assignments/{id}/cancel — cancelar membresía */
  cancelAssignment(assignmentId: string): Observable<void> {
    return this.http.patch<void>(
      `${this.apiUrl}/assignments/${assignmentId}/cancel`,
      {},
    );
  }

  /** PATCH /api/v1/assignments/{id}/freeze — congelar membresía */
  freezeMembership(assignmentId: string): Observable<void> {
    return this.http.patch<void>(
      `${this.apiUrl}/assignments/${assignmentId}/freeze`,
      {},
    );
  }

  /** PATCH /api/v1/assignments/{id}/unfreeze — descongelar membresía */
  unfreezeMembership(assignmentId: string): Observable<void> {
    return this.http.patch<void>(
      `${this.apiUrl}/assignments/${assignmentId}/unfreeze`,
      {},
    );
  }
}
