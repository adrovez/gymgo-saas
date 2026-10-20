import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  SaleDto,
  SaleSummaryDto,
  SaleStatus,
  CreateSaleRequest,
  AddSaleItemRequest,
  UpdateSaleItemRequest,
  AddSaleDiscountRequest,
  VoidSaleRequest,
  CheckoutSaleRequest,
} from '../models/sale.models';

@Injectable({ providedIn: 'root' })
export class SaleService {
  private readonly http   = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/sales`;

  /** GET /api/v1/sales?memberId=&status=&from=&to= */
  getSales(filters: {
    memberId?: string;
    status?:   SaleStatus;
    from?:     string;
    to?:       string;
  } = {}): Observable<SaleSummaryDto[]> {
    let params = new HttpParams();
    if (filters.memberId != null) params = params.set('memberId', filters.memberId);
    if (filters.status   != null) params = params.set('status',   filters.status.toString());
    if (filters.from     != null) params = params.set('from',     filters.from);
    if (filters.to       != null) params = params.set('to',       filters.to);
    return this.http.get<SaleSummaryDto[]>(this.apiUrl, { params });
  }

  /** GET /api/v1/sales/{id} */
  getSaleById(id: string): Observable<SaleDto> {
    return this.http.get<SaleDto>(`${this.apiUrl}/${id}`);
  }

  /** POST /api/v1/sales */
  createSale(request: CreateSaleRequest): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(this.apiUrl, request);
  }

  /** DELETE /api/v1/sales/{id} — solo Draft */
  deleteDraftSale(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  /** POST /api/v1/sales/checkout — crea y confirma la venta en un solo paso */
  checkoutSale(request: CheckoutSaleRequest): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(`${this.apiUrl}/checkout`, request);
  }

  // ── Ítems ─────────────────────────────────────────────────────────────────

  /** POST /api/v1/sales/{id}/items */
  addItem(saleId: string, request: AddSaleItemRequest): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(`${this.apiUrl}/${saleId}/items`, request);
  }

  /** PUT /api/v1/sales/{id}/items/{itemId} */
  updateItem(saleId: string, itemId: string, request: UpdateSaleItemRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${saleId}/items/${itemId}`, request);
  }

  /** DELETE /api/v1/sales/{id}/items/{itemId} */
  removeItem(saleId: string, itemId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${saleId}/items/${itemId}`);
  }

  // ── Descuentos ────────────────────────────────────────────────────────────

  /** POST /api/v1/sales/{id}/discounts */
  addDiscount(saleId: string, request: AddSaleDiscountRequest): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(`${this.apiUrl}/${saleId}/discounts`, request);
  }

  /** DELETE /api/v1/sales/{id}/discounts/{discountId} */
  removeDiscount(saleId: string, discountId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${saleId}/discounts/${discountId}`);
  }

  // ── Flujo de estado ───────────────────────────────────────────────────────

  /** PATCH /api/v1/sales/{id}/confirm */
  confirmSale(id: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/confirm`, {});
  }

  /** PATCH /api/v1/sales/{id}/void */
  voidSale(id: string, request: VoidSaleRequest): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/void`, request);
  }
}
