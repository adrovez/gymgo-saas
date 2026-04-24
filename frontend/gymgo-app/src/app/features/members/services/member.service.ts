import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  MemberDto,
  GetMembersResult,
  CreateMemberRequest,
  UpdateMemberRequest,
  ChangeMemberStatusRequest,
  MemberStatus,
} from '../models/member.models';

export interface GetMembersParams {
  search?:   string;
  status?:   MemberStatus | null;
  page:      number;
  pageSize:  number;
}

@Injectable({ providedIn: 'root' })
export class MemberService {
  private readonly http   = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/members`;

  /** GET /api/v1/members — listado paginado */
  getMembers(params: GetMembersParams): Observable<GetMembersResult> {
    let httpParams = new HttpParams()
      .set('page',     params.page.toString())
      .set('pageSize', params.pageSize.toString());

    if (params.search?.trim()) {
      httpParams = httpParams.set('search', params.search.trim());
    }
    if (params.status != null) {
      httpParams = httpParams.set('status', params.status.toString());
    }

    return this.http.get<GetMembersResult>(this.apiUrl, { params: httpParams });
  }

  /** GET /api/v1/members/{id} */
  getMemberById(id: string): Observable<MemberDto> {
    return this.http.get<MemberDto>(`${this.apiUrl}/${id}`);
  }

  /** POST /api/v1/members */
  createMember(request: CreateMemberRequest): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(this.apiUrl, request);
  }

  /** PUT /api/v1/members/{id} */
  updateMember(id: string, request: UpdateMemberRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, request);
  }

  /** PATCH /api/v1/members/{id}/status */
  changeMemberStatus(id: string, request: ChangeMemberStatusRequest): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/status`, request);
  }

  /** DELETE /api/v1/members/{id} */
  deleteMember(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
