import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { KeyValuePipe } from '@angular/common';

import {
  CashTransactionDto,
  CashSummaryDto,
  CashTransactionType,
  CashPaymentMethod,
  TransactionConcept,
  CONCEPT_LABELS,
  PAYMENT_METHOD_LABELS,
} from '../models/cash.models';
import { CashService } from '../services/cash.service';
import { DialogService } from '../../../core/services/dialog.service';

@Component({
  selector: 'app-cash-list',
  standalone: true,
  imports: [RouterLink, FormsModule, KeyValuePipe],
  templateUrl: './cash-list.html',
})
export class CashListComponent implements OnInit {
  private readonly cashService = inject(CashService);
  private readonly dialog      = inject(DialogService);

  // ── Filtros ───────────────────────────────────────────────────────────────
  readonly filterFrom = signal(this.todayMinus(30));
  readonly filterTo   = signal(this.today());
  readonly filterType = signal<string>('');

  // ── Transacciones ─────────────────────────────────────────────────────────
  readonly loading      = signal(false);
  readonly error        = signal<string | null>(null);
  readonly transactions = signal<CashTransactionDto[]>([]);

  // ── Resumen ───────────────────────────────────────────────────────────────
  readonly summaryLoading = signal(false);
  readonly summary        = signal<CashSummaryDto | null>(null);

  // ── Anulación ─────────────────────────────────────────────────────────────
  readonly voidingId = signal<string | null>(null);

  // ── Constantes para template ──────────────────────────────────────────────
  readonly CashTransactionType = CashTransactionType;
  readonly CONCEPT_LABELS      = CONCEPT_LABELS;
  readonly PAYMENT_METHOD_LABELS = PAYMENT_METHOD_LABELS;

  readonly activeCount = computed(() =>
    this.transactions().filter((t) => !t.isVoided).length,
  );

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.summaryLoading.set(true);

    const from = this.filterFrom();
    const to   = this.filterTo();
    const type = this.filterType()
      ? (Number(this.filterType()) as CashTransactionType)
      : undefined;

    this.cashService.getTransactions({ from, to, type }).subscribe({
      next: (items) => {
        this.transactions.set(items);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('No se pudo cargar el historial de movimientos.');
        this.loading.set(false);
      },
    });

    this.cashService.getSummary(from, to).subscribe({
      next: (s) => {
        this.summary.set(s);
        this.summaryLoading.set(false);
      },
      error: () => {
        this.summaryLoading.set(false);
      },
    });
  }

  applyFilters(): void {
    this.load();
  }

  async voidTransaction(tx: CashTransactionDto): Promise<void> {
    const result = await this.dialog.confirmDanger(
      'Anular movimiento',
      `¿Confirmas la anulación de ${this.formatAmount(tx.amount)} (${CONCEPT_LABELS[tx.concept]})?`,
      'Sí, anular',
    );
    if (!result.isConfirmed) return;

    const reasonResult = await (this.dialog as any).input?.(
      'Motivo de anulación',
      'Ingresa el motivo (obligatorio)',
    );

    const reason: string =
      typeof reasonResult === 'object' && reasonResult?.value
        ? reasonResult.value
        : 'Anulado por el operador';

    this.voidingId.set(tx.id);
    this.cashService.voidTransaction(tx.id, { reason }).subscribe({
      next: () => {
        this.dialog.toast('Movimiento anulado', 'success');
        this.voidingId.set(null);
        this.load();
      },
      error: () => {
        this.dialog.toast('No se pudo anular el movimiento.', 'error');
        this.voidingId.set(null);
      },
    });
  }

  // ── Helpers ───────────────────────────────────────────────────────────────

  formatAmount(amount: number): string {
    return new Intl.NumberFormat('es-CL', {
      style:    'currency',
      currency: 'CLP',
    }).format(amount);
  }

  formatDate(dateStr: string): string {
    const [y, m, d] = dateStr.split('-');
    return `${d}/${m}/${y}`;
  }

  typeLabel(type: CashTransactionType): string {
    return type === CashTransactionType.Ingreso ? 'Ingreso' : 'Egreso';
  }

  typeBadgeClass(tx: CashTransactionDto): string {
    if (tx.isVoided) return 'badge';
    return tx.type === CashTransactionType.Ingreso
      ? 'badge badge-active'
      : 'badge badge-delinquent';
  }

  amountClass(tx: CashTransactionDto): string {
    if (tx.isVoided) return 'cell-right font-medium';
    return tx.type === CashTransactionType.Ingreso
      ? 'cell-right font-medium'
      : 'cell-right font-medium';
  }

  amountSign(tx: CashTransactionDto): string {
    if (tx.isVoided) return '';
    return tx.type === CashTransactionType.Ingreso ? '+' : '−';
  }

  amountColor(tx: CashTransactionDto): string {
    if (tx.isVoided) return 'var(--color-muted-fg)';
    return tx.type === CashTransactionType.Ingreso
      ? 'var(--color-success, #16a34a)'
      : 'var(--color-error)';
  }

  private today(): string {
    return new Date().toISOString().slice(0, 10);
  }

  private todayMinus(days: number): string {
    const d = new Date();
    d.setDate(d.getDate() - days);
    return d.toISOString().slice(0, 10);
  }
}
