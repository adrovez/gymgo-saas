import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  WorkoutLogDto,
  WorkoutLogSummaryDto,
  CreateWorkoutLogRequest,
  UpdateWorkoutLogRequest,
  AddExerciseRequest,
  UpdateExerciseRequest,
} from '../models/workout-log.models';

export interface GetWorkoutLogsParams {
  memberId: string;
  from?:    string | null;
  to?:      string | null;
}

@Injectable({ providedIn: 'root' })
export class WorkoutLogService {
  private readonly http   = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/workout-logs`;

  /** GET /api/v1/workout-logs?memberId=...&from=...&to=... */
  getLogs(params: GetWorkoutLogsParams): Observable<WorkoutLogSummaryDto[]> {
    let httpParams = new HttpParams().set('memberId', params.memberId);
    if (params.from) httpParams = httpParams.set('from', params.from);
    if (params.to)   httpParams = httpParams.set('to',   params.to);
    return this.http.get<WorkoutLogSummaryDto[]>(this.apiUrl, { params: httpParams });
  }

  /** GET /api/v1/workout-logs/{id} */
  getById(id: string): Observable<WorkoutLogDto> {
    return this.http.get<WorkoutLogDto>(`${this.apiUrl}/${id}`);
  }

  /** POST /api/v1/workout-logs */
  create(request: CreateWorkoutLogRequest): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(this.apiUrl, request);
  }

  /** PUT /api/v1/workout-logs/{id} */
  update(id: string, request: UpdateWorkoutLogRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, request);
  }

  /** PATCH /api/v1/workout-logs/{id}/complete */
  complete(id: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/complete`, {});
  }

  /** DELETE /api/v1/workout-logs/{id} */
  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  /** POST /api/v1/workout-logs/{id}/exercises */
  addExercise(logId: string, request: AddExerciseRequest): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(`${this.apiUrl}/${logId}/exercises`, request);
  }

  /** PUT /api/v1/workout-logs/{logId}/exercises/{exerciseId} */
  updateExercise(logId: string, exerciseId: string, request: UpdateExerciseRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${logId}/exercises/${exerciseId}`, request);
  }

  /** DELETE /api/v1/workout-logs/{logId}/exercises/{exerciseId} */
  removeExercise(logId: string, exerciseId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${logId}/exercises/${exerciseId}`);
  }
}
