import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  WorkoutPlanDto,
  WorkoutPlanSummaryDto,
  WorkoutPlanStatus,
  CreateWorkoutPlanRequest,
  UpdateWorkoutPlanRequest,
  AddPlanDayRequest,
  AddPlanExerciseRequest,
} from '../models/workout-plan.models';

export interface GetWorkoutPlansParams {
  memberId: string;
  status?:  WorkoutPlanStatus | null;
}

@Injectable({ providedIn: 'root' })
export class WorkoutPlanService {
  private readonly http   = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/workout-plans`;

  /** GET /api/v1/workout-plans?memberId=...&status=... */
  getPlans(params: GetWorkoutPlansParams): Observable<WorkoutPlanSummaryDto[]> {
    let httpParams = new HttpParams().set('memberId', params.memberId);
    if (params.status != null) httpParams = httpParams.set('status', params.status.toString());
    return this.http.get<WorkoutPlanSummaryDto[]>(this.apiUrl, { params: httpParams });
  }

  /** GET /api/v1/workout-plans/{id} */
  getById(id: string): Observable<WorkoutPlanDto> {
    return this.http.get<WorkoutPlanDto>(`${this.apiUrl}/${id}`);
  }

  /** POST /api/v1/workout-plans */
  create(request: CreateWorkoutPlanRequest): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(this.apiUrl, request);
  }

  /** PUT /api/v1/workout-plans/{id} */
  update(id: string, request: UpdateWorkoutPlanRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, request);
  }

  /** DELETE /api/v1/workout-plans/{id} */
  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  /** POST /api/v1/workout-plans/{planId}/days */
  addDay(planId: string, request: AddPlanDayRequest): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(`${this.apiUrl}/${planId}/days`, request);
  }

  /** DELETE /api/v1/workout-plans/{planId}/days/{dayId} */
  removeDay(planId: string, dayId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${planId}/days/${dayId}`);
  }

  /** POST /api/v1/workout-plans/days/{dayId}/exercises */
  addExercise(dayId: string, request: AddPlanExerciseRequest): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(`${this.apiUrl}/days/${dayId}/exercises`, request);
  }

  /** PUT /api/v1/workout-plans/days/{dayId}/exercises/{exerciseId} */
  updateExercise(dayId: string, exerciseId: string, request: AddPlanExerciseRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/days/${dayId}/exercises/${exerciseId}`, request);
  }

  /** DELETE /api/v1/workout-plans/days/{dayId}/exercises/{exerciseId} */
  removeExercise(dayId: string, exerciseId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/days/${dayId}/exercises/${exerciseId}`);
  }
}
