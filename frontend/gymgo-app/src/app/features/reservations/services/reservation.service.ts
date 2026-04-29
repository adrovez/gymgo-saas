import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  ClassReservationDto,
  CancelReservationRequest,
  CreateReservationRequest,
  ReservationStatus,
} from '../models/reservation.models';

@Injectable({ providedIn: 'root' })
export class ReservationService {
  private readonly http   = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}`;

  /**
   * POST /api/v1/reservations
   * Crea una reserva para un socio en una sesión concreta.
   */
  createReservation(request: CreateReservationRequest): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(`${this.apiUrl}/reservations`, request);
  }

  /**
   * DELETE /api/v1/reservations/{id}
   * Cancela una reserva activa.
   */
  cancelReservation(id: string, request: CancelReservationRequest): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/reservations/${id}`, { body: request });
  }

  /**
   * GET /api/v1/schedules/{scheduleId}/reservations?sessionDate=YYYY-MM-DD
   * Devuelve todas las reservas de una sesión concreta.
   */
  getReservationsBySession(
    scheduleId: string,
    sessionDate: string,
  ): Observable<ClassReservationDto[]> {
    return this.http.get<ClassReservationDto[]>(
      `${this.apiUrl}/schedules/${scheduleId}/reservations`,
      { params: { sessionDate } },
    );
  }

  /**
   * GET /api/v1/members/{memberId}/reservations?status=&from=&to=
   * Devuelve las reservas de un socio.
   */
  getReservationsByMember(
    memberId: string,
    status?: ReservationStatus,
    from?: string,
    to?: string,
  ): Observable<ClassReservationDto[]> {
    let params: Record<string, string> = {};
    if (status !== undefined) params['status'] = String(status);
    if (from) params['from'] = from;
    if (to)   params['to']   = to;
    return this.http.get<ClassReservationDto[]>(
      `${this.apiUrl}/members/${memberId}/reservations`,
      { params },
    );
  }
}
