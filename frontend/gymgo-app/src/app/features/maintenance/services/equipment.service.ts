import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  EquipmentDto,
  EquipmentSummaryDto,
  CreateEquipmentRequest,
} from '../models/maintenance.models';

@Injectable({ providedIn: 'root' })
export class EquipmentService {
  private readonly http   = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/equipment`;

  /** GET /api/v1/equipment?isActive=... */
  getEquipment(isActive?: boolean | null): Observable<EquipmentSummaryDto[]> {
    let params = new HttpParams();
    if (isActive != null) {
      params = params.set('isActive', isActive.toString());
    }
    return this.http.get<EquipmentSummaryDto[]>(this.apiUrl, { params });
  }

  /** GET /api/v1/equipment/{id} */
  getEquipmentById(id: string): Observable<EquipmentDto> {
    return this.http.get<EquipmentDto>(`${this.apiUrl}/${id}`);
  }

  /** POST /api/v1/equipment */
  createEquipment(request: CreateEquipmentRequest): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(this.apiUrl, request);
  }

  /** PUT /api/v1/equipment/{id} */
  updateEquipment(id: string, request: CreateEquipmentRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, request);
  }

  /** POST /api/v1/equipment/{id}/deactivate */
  deactivate(id: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/deactivate`, {});
  }

  /** POST /api/v1/equipment/{id}/reactivate */
  reactivate(id: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/reactivate`, {});
  }
}
