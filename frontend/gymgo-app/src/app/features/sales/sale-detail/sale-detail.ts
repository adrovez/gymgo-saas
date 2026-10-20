import { Component, OnInit, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import Swal from 'sweetalert2';

import {
  SaleDto,
  SaleStatus,
  SALE_ITEM_TYPE_LABELS,
  SALE_DISCOUNT_TYPE_LABELS,
  SaleDiscountType,
} from '../models/sale.models';
import { SaleService } from '../services/sale.service';
import { DialogService } from '../../../core/services/dialog.service';

@Component({
  selector: 'app-sale-detail',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './sale-detail.html',
})
export class SaleDetailComponent implements OnInit {
  private readonly saleService = inject(SaleService);
  private readonly dialog      = inject(DialogService);

  /** Enlazado desde la ruta /sales/:id via withComponentInputBinding() */
  readonly id = input.required<string>();

  readonly loading  = signal(true);
  readonly error    = signal<string | null>(null);
  readonly sale     = signal<SaleDto | null>(null);
  readonly voiding  = signal(false);

  readonly SaleStatus              = SaleStatus;
  readonly SALE_ITEM_TYPE_LABELS   = SALE_ITEM_TYPE_LABELS;
  readonly SALE_DISCOUNT_TYPE_LABELS = SALE_DISCOUNT_TYPE_LABELS;
  readonly SaleDiscountType        = SaleDiscountType;

  ngOnInit(): void {
    this.loadSale();
  }

  loadSale(): void {
    this.loading.set(true);
    this.error.set(null);
    this.saleService.getSaleById(this.id()).subscribe({
      next: (sale) => {
        this.sale.set(sale);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('No se pudo cargar el detalle de la venta.');
        this.loading.set(false);
      },
    });
  }

  async voidSale(): Promise<void> {
    const result = await Swal.fire({
      title: 'Anular venta',
      text: 'Indica el motivo de anulación. Esta acción revertirá la membresía y el ingreso en Caja.',
      input: 'textarea',
      inputPlaceholder: 'Motivo de anulación...',
      showCancelButton: true,
      confirmButtonText: 'Anular venta',
      cancelButtonText: 'Cancelar',
      confirmButtonColor: '#dc2626',
      cancelButtonColor: '#475569',
      reverseButtons: true,
      inputValidator: (v) => (!v.trim() ? 'El motivo es obligatorio.' : undefined),
    });
    if (!result.isConfirmed) return;
    const reason = result.value as string;

    this.voiding.set(true);
    this.saleService.voidSale(this.id(), { reason }).subscribe({
      next: () => {
        this.voiding.set(false);
        this.dialog.toast('Venta anulada correctamente', 'success');
        this.loadSale();
      },
      error: (err) => {
        const msg = err?.error?.detail ?? 'No se pudo anular la venta.';
        this.dialog.toast(msg, 'error');
        this.voiding.set(false);
      },
    });
  }

  statusBadgeStyle(status: SaleStatus): string {
    switch (status) {
      case SaleStatus.Draft:     return 'badge';
      case SaleStatus.Confirmed: return 'badge badge-active';
      case SaleStatus.Voided:    return 'badge badge-delinquent';
      default: return 'badge';
    }
  }

  formatAmount(amount: number): string {
    return new Intl.NumberFormat('es-CL', { style: 'currency', currency: 'CLP' }).format(amount);
  }
}
