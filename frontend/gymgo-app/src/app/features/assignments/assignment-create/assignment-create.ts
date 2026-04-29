import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { MembershipAssignmentService } from '../services/membership-assignment.service';
import { MemberService } from '../../members/services/member.service';
import { MembershipPlanService } from '../../membership-plans/services/membership-plan.service';
import { MemberSummaryDto } from '../../members/models/member.models';
import { MembershipPlanSummaryDto } from '../../membership-plans/models/membership-plan.models';
import { DialogService } from '../../../core/services/dialog.service';

@Component({
  selector: 'app-assignment-create',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './assignment-create.html',
})
export class AssignmentCreateComponent implements OnInit {
  private readonly fb                = inject(FormBuilder);
  private readonly router            = inject(Router);
  private readonly assignmentService = inject(MembershipAssignmentService);
  private readonly memberService     = inject(MemberService);
  private readonly planService       = inject(MembershipPlanService);
  private readonly dialog            = inject(DialogService);

  readonly loadingData = signal(true);
  readonly loading     = signal(false);
  readonly errorLoad   = signal<string | null>(null);
  readonly error       = signal<string | null>(null);

  readonly members = signal<MemberSummaryDto[]>([]);
  readonly plans   = signal<MembershipPlanSummaryDto[]>([]);

  readonly PAYMENT_METHODS = [
    { value: 'Efectivo',  label: 'Efectivo' },
    { value: 'Depósito',  label: 'Depósito' },
    { value: 'Débito',    label: 'Débito' },
    { value: 'Crédito',   label: 'Crédito' },
    { value: 'Otros',     label: 'Otros' },
  ];

  readonly form = this.fb.nonNullable.group({
    memberId:         ['', Validators.required],
    membershipPlanId: ['', Validators.required],
    paymentMethod:    ['Efectivo', Validators.required],
    startDate:        [''],
    notes:            ['', Validators.maxLength(500)],
  });

  /** Plan seleccionado actualmente (para mostrar info de referencia) */
  get selectedPlan(): MembershipPlanSummaryDto | null {
    const id = this.form.controls.membershipPlanId.value;
    return this.plans().find((p) => p.id === id) ?? null;
  }

  ngOnInit(): void {
    this.loadFormData();
  }

  loadFormData(): void {
    this.loadingData.set(true);
    this.errorLoad.set(null);

    let membersLoaded = false;
    let plansLoaded   = false;

    const checkDone = () => {
      if (membersLoaded && plansLoaded) this.loadingData.set(false);
    };

    // Cargar socios activos (sin filtro de estado para permitir asignación a cualquiera)
    this.memberService.getMembers({ page: 1, pageSize: 200 }).subscribe({
      next: (result) => {
        this.members.set(result.items);
        membersLoaded = true;
        checkDone();
      },
      error: () => {
        this.errorLoad.set('No se pudo cargar la lista de socios.');
        this.loadingData.set(false);
      },
    });

    // Cargar planes activos
    this.planService.getMembershipPlans({ isActive: true }).subscribe({
      next: (result) => {
        this.plans.set(result);
        plansLoaded = true;
        checkDone();
      },
      error: () => {
        this.errorLoad.set('No se pudo cargar la lista de planes.');
        this.loadingData.set(false);
      },
    });
  }

  formatAmount(amount: number): string {
    return new Intl.NumberFormat('es-CL', { style: 'currency', currency: 'CLP' }).format(amount);
  }

  async onSubmit(): Promise<void> {
    if (this.form.invalid || this.loading()) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.error.set(null);

    const raw = this.form.getRawValue();

    // Construir notas incluyendo método de pago
    const paymentNote = `Pago: ${raw.paymentMethod}`;
    const notes       = raw.notes?.trim()
      ? `${paymentNote}. ${raw.notes.trim()}`
      : paymentNote;

    // Paso 1: crear asignación
    this.assignmentService
      .assignMembershipPlan(raw.memberId, {
        membershipPlanId: raw.membershipPlanId,
        startDate:        raw.startDate || null,
        notes,
      })
      .subscribe({
        next: ({ id }) => {
          // Paso 2: registrar pago
          this.assignmentService.registerPayment(id).subscribe({
            next: async () => {
              this.loading.set(false);
              await this.dialog.success(
                '¡Membresía asignada!',
                `El plan fue asignado y el pago registrado (${raw.paymentMethod}) exitosamente.`,
              );
              this.router.navigate(['/app/assignments']);
            },
            error: () => {
              // La asignación se creó pero el pago falló — informar al usuario
              this.error.set(
                'La membresía fue asignada, pero no se pudo registrar el pago. Regístralo manualmente desde el perfil del socio.',
              );
              this.loading.set(false);
            },
          });
        },
        error: (err: HttpErrorResponse) => {
          this.error.set(this.parseError(err));
          this.loading.set(false);
        },
      });
  }

  isInvalid(field: keyof typeof this.form.controls): boolean {
    const control = this.form.get(field as string);
    return !!(control?.invalid && control?.touched);
  }

  cancel(): void {
    this.router.navigate(['/app/assignments']);
  }

  private parseError(err: HttpErrorResponse): string {
    console.error('[AssignmentCreate] error HTTP', err.status, err.error);

    if (err.status === 0)   return 'No se pudo conectar con el servidor.';
    if (err.status === 401) return 'Sesión expirada. Por favor, inicia sesión nuevamente.';
    if (err.status === 403) return 'No tienes permisos para realizar esta acción.';
    if (err.status === 404) return 'El socio o el plan no fueron encontrados.';

    if (err.status === 422) {
      const body = typeof err.error === 'string'
        ? (() => { try { return JSON.parse(err.error); } catch { return null; } })()
        : err.error;

      if (body?.detail) return body.detail;
      // Errores de dominio comunes
      return 'El socio ya tiene una membresía activa o congelada. Cancélala primero para asignar un nuevo plan.';
    }

    if (err.status === 400) {
      const body = typeof err.error === 'string'
        ? (() => { try { return JSON.parse(err.error); } catch { return null; } })()
        : err.error;

      if (body) {
        const detail = body.detail ?? body.title;
        if (detail) return detail;
        const errors = body.errors;
        if (errors) {
          const messages = (Object.values(errors) as string[][]).flat();
          if (messages.length) return messages[0];
        }
      }
      return 'Los datos enviados no son válidos.';
    }

    return `Error ${err.status}: no se pudo asignar el plan. Intenta nuevamente.`;
  }
}
