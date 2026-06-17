import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  CashTransactionDto,
  CashSummaryDto,
  RegisterTransactionRequest,
  VoidTransactionRequest,
  CashTransactionType,
} from '../models/cash.models';

@Injectable({ providedIn: 'root' })
export class CashService {
  private readonly http   = inject(HttpClient);
  private readonly apiUrl = environment.apiUrl;

  /** GET /api/v1/cash/transactions?from=&to=&type=&memberId= */
  getTransactions(filters: {
    from?:     string;
    to?:       string;
    type?:     CashTransactionType;
    memberId?: string;
  } = {}): Observable<CashTransactionDto[]> {
    let params = new HttpParams();
    if (filters.from     != null) params = params.set('from',     filters.from);
    if (filters.to       != null) params = params.set('to',       filters.to);
    if (filters.type     != null) params = params.set('type',     filters.type.toString());
    if (filters.memberId != null) params = params.set('memberId', filters.memberId);
    return this.http.get<CashTransactionDto[]>(`${this.apiUrl}/cash/transactions`, { params });
  }

  /** GET /api/v1/cash/summary?from=&to= */
  getSummary(from: string, to: string): Observable<CashSummaryDto> {
    const params = new HttpParams().set('from', from).set('to', to);
    return this.http.get<CashSummaryDto>(`${this.apiUrl}/cash/summary`, { params });
  }

  /** POST /api/v1/cash/transactions */
  registerTransaction(request: RegisterTransactionRequest): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(`${this.apiUrl}/cash/transactions`, request);
  }

  /** PATCH /api/v1/cash/transactions/{id}/void */
  voidTransaction(id: string, request: VoidTransactionRequest): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/cash/transactions/${id}/void`, request);
  }
}
