import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { GymEntryDto, RegisterGymEntryRequest } from '../models/gym-entry.models';

@Injectable({ providedIn: 'root' })
export class GymEntryService {
  private readonly http   = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/gym-entries`;

  /**
   * GET /api/v1/gym-entries?date=YYYY-MM-DD
   * Devuelve ingresos del día indicado (o hoy si no se indica).
   */
  getEntriesByDate(date?: string): Observable<GymEntryDto[]> {
    let params = new HttpParams();
    if (date) params = params.set('date', date);
    return this.http.get<GymEntryDto[]>(this.apiUrl, { params });
  }

  /** POST /api/v1/gym-entries — registrar ingreso */
  registerEntry(request: RegisterGymEntryRequest): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(this.apiUrl, request);
  }
}
