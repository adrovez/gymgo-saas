import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  GymClassDto,
  GymClassSummaryDto,
  ClassScheduleDto,
  CreateGymClassRequest,
  CreateClassScheduleRequest,
} from '../models/class.models';

@Injectable({ providedIn: 'root' })
export class ClassService {
  private readonly http   = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/classes`;

  // ── GymClasses ────────────────────────────────────────────────────────────

  /** GET /api/v1/classes */
  getClasses(isActive?: boolean | null): Observable<GymClassSummaryDto[]> {
    let params = new HttpParams();
    if (isActive != null) params = params.set('isActive', isActive.toString());
    return this.http.get<GymClassSummaryDto[]>(this.apiUrl, { params });
  }

  /** GET /api/v1/classes/{id} */
  getClassById(id: string): Observable<GymClassDto> {
    return this.http.get<GymClassDto>(`${this.apiUrl}/${id}`);
  }

  /** POST /api/v1/classes */
  createClass(request: CreateGymClassRequest): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(this.apiUrl, request);
  }

  /** PUT /api/v1/classes/{id} */
  updateClass(id: string, request: CreateGymClassRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, request);
  }

  /** PATCH /api/v1/classes/{id}/deactivate */
  deactivateClass(id: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/deactivate`, {});
  }

  /** PATCH /api/v1/classes/{id}/reactivate */
  reactivateClass(id: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/reactivate`, {});
  }

  // ── ClassSchedules ────────────────────────────────────────────────────────

  /** GET /api/v1/classes/schedule/weekly */
  getWeeklySchedule(): Observable<ClassScheduleDto[]> {
    return this.http.get<ClassScheduleDto[]>(`${this.apiUrl}/schedule/weekly`);
  }

  /** POST /api/v1/classes/{classId}/schedules */
  createSchedule(classId: string, request: CreateClassScheduleRequest): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(`${this.apiUrl}/${classId}/schedules`, request);
  }

  /** PUT /api/v1/schedules/{id} */
  updateSchedule(id: string, request: CreateClassScheduleRequest): Observable<void> {
    return this.http.put<void>(`${environment.apiUrl}/schedules/${id}`, request);
  }

  /** DELETE /api/v1/schedules/{id} */
  deleteSchedule(id: string): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}/schedules/${id}`);
  }
}
