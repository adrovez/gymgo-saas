import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  MaintenanceRecordDto,
  MaintenanceRecordSummaryDto,
  CreateMaintenanceRecordRequest,
  CompleteMaintenanceRequest,
  MaintenanceType,
  MaintenanceStatus,
} from '../models/maintenance.models';

export interface GetMaintenanceParams {
  equipmentId?: string | null;
  type?:        MaintenanceType | null;
  status?:      MaintenanceStatus | null;
}

@Injectable({ providedIn: 'root' })
export class MaintenanceService {
  private readonly http   = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/maintenance`;

  /** GET /api/v1/maintenance?equipmentId=...&type=...&status=... */
  getRecords(filters?: GetMaintenanceParams): Observable<MaintenanceRecordSummaryDto[]> {
    let params = new HttpParams();
    if (filters?.equipmentId) params = params.set('equipmentId', filters.equipmentId);
    if (filters?.type != null) params = params.set('type', filters.type.toString());
    if (filters?.status != null) params = params.set('status', filters.status.toString());
    return this.http.get<MaintenanceRecordSummaryDto[]>(this.apiUrl, { params });
  }

  /** GET /api/v1/maintenance/{id} */
  getById(id: string): Observable<MaintenanceRecordDto> {
    return this.http.get<MaintenanceRecordDto>(`${this.apiUrl}/${id}`);
  }

  /** POST /api/v1/maintenance */
  create(request: CreateMaintenanceRecordRequest): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(this.apiUrl, request);
  }

  /** POST /api/v1/maintenance/{id}/start */
  start(id: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/start`, {});
  }

  /** POST /api/v1/maintenance/{id}/complete */
  complete(id: string, request: CompleteMaintenanceRequest): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/complete`, request);
  }

  /** POST /api/v1/maintenance/{id}/cancel?reason=... */
  cancel(id: string, reason?: string | null): Observable<void> {
    let params = new HttpParams();
    if (reason?.trim()) params = params.set('reason', reason.trim());
    return this.http.post<void>(`${this.apiUrl}/${id}/cancel`, {}, { params });
  }
}
