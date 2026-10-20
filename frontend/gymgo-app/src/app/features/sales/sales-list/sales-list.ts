import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';

import {
  SaleSummaryDto,
  SaleStatus,
  SalePaymentMethod,
  SALE_STATUS_LABELS,
  SALE_PAYMENT_METHOD_LABELS,
} from '../models/sale.models';
import { SaleService } from '../services/sale.service';
import { DialogService } from '../../../core/services/dialog.service';

@Component({
  selector: 'app-sales-list',
  standalone: true,
  imports: [RouterLink, FormsModule],
  templateUrl: './sales-list.html',
})
export class SalesListComponent implements OnInit {
  private readonly saleService = inject(SaleService);
  private readonly dialog      = inject(DialogService);

  readonly loading = signal(false);
  readonly error   = signal<string | null>(null);
  readonly sales   = signal<SaleSummaryDto[]>([]);

  // ── Filtros ───────────────────────────────────────────────────────────────
  readonly filterStatus = signal<SaleStatus | ''>('');
  readonly filterFrom   = signal('');
  readonly filterTo     = signal('');

  // ── Constantes ────────────────────────────────────────────────────────────
  readonly SaleStatus               = SaleStatus;
  readonly SALE_STATUS_LABELS       = SALE_STATUS_LABELS;
  readonly SALE_PAYMENT_METHOD_LABELS = SALE_PAYMENT_METHOD_LABELS;

  readonly statusOptions = [
    { value: SaleStatus.Draft,     label: SALE_STATUS_LABELS[SaleStatus.Draft] },
    { value: SaleStatus.Confirmed, label: SALE_STATUS_LABELS[SaleStatus.Confirmed] },
    { value: SaleStatus.Voided,    label: SALE_STATUS_LABELS[SaleStatus.Voided] },
  ];

  readonly totalIncome = computed(() =>
    this.sales()
      .filter(s => s.status === SaleStatus.Confirmed)
      .reduce((sum, s) => sum + s.totalAmount, 0),
  );

  ngOnInit(): void {
    this.loadSales();
  }

  loadSales(): void {
    this.loading.set(true);
    this.error.set(null);

    const filters: { status?: SaleStatus; from?: string; to?: string } = {};
    const st = this.filterStatus();
    if (st !== '') filters.status = st;
    if (this.filterFrom()) filters.from = this.filterFrom();
    if (this.filterTo())   filters.to   = this.filterTo();

    this.saleService.getSales(filters).subscribe({
      next: (data) => {
        this.sales.set([...data].sort((a, b) => b.date.localeCompare(a.date)));
        this.loading.set(false);
      },
      error: () => {
        this.error.set('No se pudo cargar el listado de ventas.');
        this.loading.set(false);
      },
    });
  }

  applyFilters(): void {
    this.loadSales();
  }

  clearFilters(): void {
    this.filterStatus.set('');
    this.filterFrom.set('');
    this.filterTo.set('');
    this.loadSales();
  }

  async deleteVenta(sale: SaleSummaryDto): Promise<void> {
    const result = await this.dialog.confirmDanger(
      '¿Eliminar venta?',
      'Esta venta en borrador se eliminará permanentemente. Esta acción no se puede deshacer.',
      'Sí, eliminar',
    );
    if (!result.isConfirmed) return;

    this.saleService.deleteDraftSale(sale.id).subscribe({
      next: () => {
        this.dialog.toast('Venta eliminada', 'success');
        this.loadSales();
      },
      error: () => this.dialog.toast('No se pudo eliminar la venta.', 'error'),
    });
  }

  // ── Helpers ───────────────────────────────────────────────────────────────

  formatAmount(amount: number): string {
    return new Intl.NumberFormat('es-CL', { style: 'currency', currency: 'CLP' }).format(amount);
  }

  statusBadgeStyle(status: SaleStatus): string {
    switch (status) {
      case SaleStatus.Draft:     return 'badge';
      case SaleStatus.Confirmed: return 'badge badge-active';
      case SaleStatus.Voided:    return 'badge badge-delinquent';
      default: return 'badge';
    }
  }
}
