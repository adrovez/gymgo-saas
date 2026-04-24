import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { Gender, GENDER_OPTIONS } from '../models/member.models';
import { MemberService } from '../services/member.service';
import { DialogService } from '../../../core/services/dialog.service';
import { rutValidator } from '../../../core/validators/rut.validator';

@Component({
  selector: 'app-member-create',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './member-create.html',
})
export class MemberCreateComponent {
  private readonly fb            = inject(FormBuilder);
  private readonly router        = inject(Router);
  private readonly memberService = inject(MemberService);
  private readonly dialog        = inject(DialogService);

  readonly loading       = signal(false);
  readonly error         = signal<string | null>(null);
  readonly genderOptions = GENDER_OPTIONS;

  readonly form = this.fb.nonNullable.group({
    // ── Identificación ──────────────────────────────────
    rut:       ['', [Validators.required, Validators.maxLength(20), rutValidator]],
    firstName: ['', [Validators.required, Validators.maxLength(100)]],
    lastName:  ['', [Validators.required, Validators.maxLength(100)]],
    birthDate: ['', Validators.required],
    gender:    [Gender.NotSpecified, Validators.required],

    // ── Contacto (obligatorios según requerimiento) ──────
    email:   ['', [Validators.required, Validators.email, Validators.maxLength(200)]],
    phone:   ['', [Validators.required, Validators.maxLength(40)]],
    address: ['', [Validators.required, Validators.maxLength(300)]],

    // ── Contacto de emergencia ───────────────────────────
    emergencyContactName:  ['', Validators.maxLength(200)],
    emergencyContactPhone: ['', Validators.maxLength(40)],

    // ── Extras ──────────────────────────────────────────
    registrationDate: [''],
    notes:            ['', Validators.maxLength(1000)],
  });

  async onSubmit(): Promise<void> {
    if (this.form.invalid || this.loading()) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.error.set(null);

    const raw = this.form.getRawValue();

    this.memberService
      .createMember({
        rut:                   raw.rut,
        firstName:             raw.firstName,
        lastName:              raw.lastName,
        birthDate:             raw.birthDate,
        gender:                Number(raw.gender) as Gender,
        email:                 raw.email   || null,
        phone:                 raw.phone   || null,
        address:               raw.address || null,
        emergencyContactName:  raw.emergencyContactName  || null,
        emergencyContactPhone: raw.emergencyContactPhone || null,
        registrationDate:      raw.registrationDate || null,
        notes:                 raw.notes || null,
      })
      .subscribe({
        next: async () => {
          this.loading.set(false);
          await this.dialog.success(
            '¡Socio creado!',
            `${raw.firstName} ${raw.lastName} fue registrado exitosamente.`,
          );
          this.router.navigate(['/app/members']);
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
    this.router.navigate(['/app/members']);
  }

  private parseError(err: HttpErrorResponse): string {
    console.error('[MemberCreate] error HTTP', err.status, err.error);

    if (err.status === 0)   return 'No se pudo conectar con el servidor. Verifica tu conexión o que la API esté en ejecución.';
    if (err.status === 401) return 'Sesión expirada. Por favor, inicia sesión nuevamente.';
    if (err.status === 403) return 'No tienes permisos para realizar esta acción.';
    if (err.status === 409) return 'Ya existe un socio con ese RUT en este gimnasio.';

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

    return `Error ${err.status}: no se pudo crear el socio. Intenta nuevamente.`;
  }
}
