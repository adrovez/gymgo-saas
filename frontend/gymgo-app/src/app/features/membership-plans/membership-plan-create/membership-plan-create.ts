import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import {
  Periodicity,
  PERIODICITY_OPTIONS,
  DAYS_OF_WEEK,
  toTimeApi,
} from '../models/membership-plan.models';
import { MembershipPlanService } from '../services/membership-plan.service';
import { DialogService } from '../../../core/services/dialog.service';

@Component({
  selector: 'app-membership-plan-create',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './membership-plan-create.html',
})
export class MembershipPlanCreateComponent {
  private readonly fb          = inject(FormBuilder);
  private readonly router      = inject(Router);
  private readonly planService = inject(MembershipPlanService);
  private readonly dialog      = inject(DialogService);

  readonly loading            = signal(false);
  readonly error              = signal<string | null>(null);
  readonly periodicityOptions = PERIODICITY_OPTIONS;
  readonly daysOfWeek         = DAYS_OF_WEEK;

  readonly form = this.fb.nonNullable.group({
    // ── Identificación ───────────────────────────────────
    name:        ['', [Validators.required, Validators.maxLength(100)]],
    description: ['', Validators.maxLength(500)],
    periodicity: [Periodicity.Monthly, Validators.required],
    amount:      [0, [Validators.required, Validators.min(0)]],

    // ── Días ─────────────────────────────────────────────
    daysPerWeek: [1, [Validators.required, Validators.min(1), Validators.max(7)]],
    fixedDays:   [false],
    monday:      [false],
    tuesday:     [false],
    wednesday:   [false],
    thursday:    [false],
    friday:      [false],
    saturday:    [false],
    sunday:      [false],

    // ── Horario ───────────────────────────────────────────
    freeSchedule: [true],
    timeFrom:     [''],
    timeTo:       [''],

    // ── Extras ───────────────────────────────────────────
    allowsFreezing: [false],
  });

  get fixedDays(): boolean {
    return this.form.controls.fixedDays.value;
  }

  get freeSchedule(): boolean {
    return this.form.controls.freeSchedule.value;
  }

  get checkedDaysCount(): number {
    const c = this.form.controls;
    return [c.monday, c.tuesday, c.wednesday, c.thursday, c.friday, c.saturday, c.sunday]
      .filter((ctrl) => ctrl.value).length;
  }

  onFixedDaysChange(checked: boolean): void {
    if (!checked) {
      // Limpiar selección de días al desactivar
      this.form.patchValue({
        monday: false, tuesday: false, wednesday: false, thursday: false,
        friday: false, saturday: false, sunday: false,
      });
    }
  }

  onFreeScheduleChange(checked: boolean): void {
    if (checked) {
      this.form.patchValue({ timeFrom: '', timeTo: '' });
    }
  }

  async onSubmit(): Promise<void> {
    if (this.form.invalid || this.loading()) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();

    // Validar que si fixedDays está activado, los días marcados = daysPerWeek
    if (raw.fixedDays && this.checkedDaysCount !== raw.daysPerWeek) {
      this.error.set(
        `Debes seleccionar exactamente ${raw.daysPerWeek} día(s) cuando usas días fijos. Actualmente tienes ${this.checkedDaysCount} seleccionado(s).`,
      );
      return;
    }

    // Validar horario si no es horario libre
    if (!raw.freeSchedule && (!raw.timeFrom || !raw.timeTo)) {
      this.error.set('Debes ingresar la hora de inicio y fin cuando el horario no es libre.');
      return;
    }

    this.loading.set(true);
    this.error.set(null);

    this.planService
      .createMembershipPlan({
        name:           raw.name,
        description:    raw.description || null,
        periodicity:    Number(raw.periodicity) as Periodicity,
        daysPerWeek:    raw.daysPerWeek,
        fixedDays:      raw.fixedDays,
        monday:         raw.monday,
        tuesday:        raw.tuesday,
        wednesday:      raw.wednesday,
        thursday:       raw.thursday,
        friday:         raw.friday,
        saturday:       raw.saturday,
        sunday:         raw.sunday,
        freeSchedule:   raw.freeSchedule,
        timeFrom:       raw.freeSchedule ? null : toTimeApi(raw.timeFrom),
        timeTo:         raw.freeSchedule ? null : toTimeApi(raw.timeTo),
        amount:         raw.amount,
        allowsFreezing: raw.allowsFreezing,
      })
      .subscribe({
        next: async () => {
          this.loading.set(false);
          await this.dialog.success(
            '¡Plan creado!',
            `El plan "${raw.name}" fue registrado exitosamente.`,
          );
          this.router.navigate(['/app/membership-plans']);
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

  hasError(field: keyof typeof this.form.controls, errorKey: string): boolean {
    const control = this.form.get(field as string);
    return !!(control?.touched && control?.hasError(errorKey));
  }

  cancel(): void {
    this.router.navigate(['/app/membership-plans']);
  }

  private parseError(err: HttpErrorResponse): string {
    console.error('[MembershipPlanCreate] error HTTP', err.status, err.error);

    if (err.status === 0)   return 'No se pudo conectar con el servidor.';
    if (err.status === 401) return 'Sesión expirada. Por favor, inicia sesión nuevamente.';
    if (err.status === 403) return 'No tienes permisos para realizar esta acción.';
    if (err.status === 409) return 'Ya existe un plan con ese nombre en este gimnasio.';

    if (err.status === 400 || err.status === 422) {
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
      return 'Los datos enviados no son válidos. Revisa el formulario.';
    }

    return `Error ${err.status}: no se pudo crear el plan. Intenta nuevamente.`;
  }
}
