import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  MembershipPlanDto,
  MembershipPlanSummaryDto,
  CreateMembershipPlanRequest,
  UpdateMembershipPlanRequest,
  Periodicity,
} from '../models/membership-plan.models';

export interface GetMembershipPlansParams {
  search?:      string;
  periodicity?: Periodicity | null;
  isActive?:    boolean | null;
}

@Injectable({ providedIn: 'root' })
export class MembershipPlanService {
  private readonly http   = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/membership-plans`;

  /** GET /api/v1/membership-plans — listado con filtros opcionales */
  getMembershipPlans(params?: GetMembershipPlansParams): Observable<MembershipPlanSummaryDto[]> {
    let httpParams = new HttpParams();

    if (params?.search?.trim()) {
      httpParams = httpParams.set('search', params.search.trim());
    }
    if (params?.periodicity != null) {
      httpParams = httpParams.set('periodicity', params.periodicity.toString());
    }
    if (params?.isActive != null) {
      httpParams = httpParams.set('isActive', params.isActive.toString());
    }

    return this.http.get<MembershipPlanSummaryDto[]>(this.apiUrl, { params: httpParams });
  }

  /** GET /api/v1/membership-plans/{id} */
  getMembershipPlanById(id: string): Observable<MembershipPlanDto> {
    return this.http.get<MembershipPlanDto>(`${this.apiUrl}/${id}`);
  }

  /** POST /api/v1/membership-plans */
  createMembershipPlan(request: CreateMembershipPlanRequest): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(this.apiUrl, request);
  }

  /** PUT /api/v1/membership-plans/{id} */
  updateMembershipPlan(id: string, request: UpdateMembershipPlanRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, request);
  }

  /** PATCH /api/v1/membership-plans/{id}/deactivate */
  deactivateMembershipPlan(id: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/deactivate`, {});
  }

  /** PATCH /api/v1/membership-plans/{id}/reactivate */
  reactivateMembershipPlan(id: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/reactivate`, {});
  }
}
