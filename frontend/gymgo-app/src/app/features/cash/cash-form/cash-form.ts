import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';

import {
  CashTransactionType,
  CashPaymentMethod,
  TransactionConcept,
  INCOME_CONCEPTS,
  EXPENSE_CONCEPTS,
  PAYMENT_METHOD_OPTIONS,
} from '../models/cash.models';
import { CashService } from '../services/cash.service';
import { MemberService } from '../../members/services/member.service';
import { MemberSummaryDto } from '../../members/models/member.models';
import { DialogService } from '../../../core/services/dialog.service';

@Component({
  selector: 'app-cash-form',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './cash-form.html',
})
export class CashFormComponent implements OnInit {
  private readonly fb          = inject(FormBuilder);
  private readonly router      = inject(Router);
  private readonly cashService = inject(CashService);
  private readonly memberService = inject(MemberService);
  private readonly dialog      = inject(DialogService);

  readonly loading     = signal(false);
  readonly loadingData = signal(false);
  readonly error       = signal<string | null>(null);
  readonly members     = signal<MemberSummaryDto[]>([]);

  readonly CashTransactionType  = CashTransactionType;
  readonly CashPaymentMethod    = CashPaymentMethod;
  readonly PAYMENT_METHOD_OPTIONS = PAYMENT_METHOD_OPTIONS;

  readonly form = this.fb.nonNullable.group({
    type:          [CashTransactionType.Ingreso, Validators.required],
    date:          [this.today(), Validators.required],
    amount:        [null as unknown as number, [Validators.required, Validators.min(1)]],
    paymentMethod: [CashPaymentMethod.Efectivo, Validators.required],
    concept:       [TransactionConcept.CuotaMembresia as TransactionConcept, Validators.required],
    description:   ['', Validators.maxLength(500)],
    memberId:      [''],
  });

  readonly isExpense = computed(() =>
    this.form.controls.type.value === CashTransactionType.Egreso,
  );

  readonly availableConcepts = computed(() =>
    this.isExpense() ? EXPENSE_CONCEPTS : INCOME_CONCEPTS,
  );

  ngOnInit(): void {
    this.loadMembers();
    this.form.controls.type.valueChanges.subscribe(() => this.onTypeChange());
  }

  private loadMembers(): void {
    this.loadingData.set(true);
    this.memberService.getMembers({ page: 1, pageSize: 500 }).subscribe({
      next: (result) => {
        this.members.set(result.items);
        this.loadingData.set(false);
      },
      error: () => {
        this.loadingData.set(false);
      },
    });
  }

  private onTypeChange(): void {
    const isExpense = this.form.controls.type.value === CashTransactionType.Egreso;

    if (isExpense) {
      this.form.controls.concept.setValue(TransactionConcept.Servicios);
      this.form.controls.memberId.setValue('');
      this.form.controls.description.addValidators(Validators.required);
    } else {
      this.form.controls.concept.setValue(TransactionConcept.CuotaMembresia);
      this.form.controls.description.removeValidators(Validators.required);
    }
    this.form.controls.description.updateValueAndValidity();
  }

  async onSubmit(): Promise<void> {
    if (this.form.invalid || this.loading()) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.error.set(null);

    const raw = this.form.getRawValue();

    this.cashService
      .registerTransaction({
        date:                   raw.date,
        type:                   raw.type,
        amount:                 raw.amount,
        paymentMethod:          raw.paymentMethod,
        concept:                raw.concept,
        description:            raw.description?.trim() || null,
        memberId:               raw.memberId || null,
        membershipAssignmentId: null,
      })
      .subscribe({
        next: async () => {
          this.loading.set(false);
          await this.dialog.success(
            '¡Movimiento registrado!',
            `El ${raw.type === CashTransactionType.Ingreso ? 'ingreso' : 'egreso'} fue registrado exitosamente.`,
          );
          this.router.navigate(['/app/cash']);
        },
        error: (err: HttpErrorResponse) => {
          this.error.set(this.parseError(err));
          this.loading.set(false);
        },
      });
  }

  isInvalid(field: keyof typeof this.form.controls): boolean {
    const ctrl = this.form.get(field as string);
    return !!(ctrl?.invalid && ctrl?.touched);
  }

  cancel(): void {
    this.router.navigate(['/app/cash']);
  }

  private today(): string {
    return new Date().toISOString().slice(0, 10);
  }

  private parseError(err: HttpErrorResponse): string {
    if (err.status === 0)   return 'No se pudo conectar con el servidor.';
    if (err.status === 401) return 'Sesión expirada. Por favor, inicia sesión nuevamente.';
    if (err.status === 403) return 'No tienes permisos para realizar esta acción.';

    if (err.status === 422 || err.status === 400) {
      const body = typeof err.error === 'string'
        ? (() => { try { return JSON.parse(err.error); } catch { return null; } })()
        : err.error;
      if (body?.detail) return body.detail;
      if (body?.errors) {
        const msgs = (Object.values(body.errors) as string[][]).flat();
        if (msgs.length) return msgs[0];
      }
    }

    return `Error ${err.status}: no se pudo registrar el movimiento. Intenta nuevamente.`;
  }
}
