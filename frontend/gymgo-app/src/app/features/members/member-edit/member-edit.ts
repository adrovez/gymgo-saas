import { Component, OnInit, inject, input, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import {
  Gender,
  MemberStatus,
  MemberDto,
  GENDER_OPTIONS,
  MEMBER_STATUS_OPTIONS,
} from '../models/member.models';
import {
  MembershipAssignmentDto,
  AssignmentStatus,
  PaymentStatus,
} from '../../assignments/models/membership-assignment.models';
import { MemberService } from '../services/member.service';
import { MembershipAssignmentService } from '../../assignments/services/membership-assignment.service';
import { DialogService } from '../../../core/services/dialog.service';

@Component({
  selector: 'app-member-edit',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './member-edit.html',
})
export class MemberEditComponent implements OnInit {
  private readonly fb                = inject(FormBuilder);
  private readonly router            = inject(Router);
  private readonly memberService     = inject(MemberService);
  private readonly assignmentService = inject(MembershipAssignmentService);
  private readonly dialog            = inject(DialogService);

  /** Enlazado desde la ruta /members/:id/edit via withComponentInputBinding() */
  readonly id = input.required<string>();

  readonly loadingMember     = signal(true);
  readonly loading           = signal(false);
  readonly loadingStatus     = signal(false);
  readonly loadingAssignment = signal(true);
  readonly loadingAction     = signal(false);

  readonly errorLoad       = signal<string | null>(null);
  readonly error           = signal<string | null>(null);
  readonly errorStatus     = signal<string | null>(null);
  readonly errorAssignment = signal<string | null>(null);

  readonly successStatus    = signal(false);
  readonly member           = signal<MemberDto | null>(null);
  readonly activeAssignment = signal<MembershipAssignmentDto | null>(null);

  readonly AssignmentStatus = AssignmentStatus;
  readonly PaymentStatus    = PaymentStatus;

  readonly genderOptions = GENDER_OPTIONS;
  readonly statusOptions = MEMBER_STATUS_OPTIONS;

  // ── Formulario de datos personales y contacto (PUT /members/{id}) ──────────
  readonly form = this.fb.nonNullable.group({
    firstName: ['', [Validators.required, Validators.maxLength(100)]],
    lastName:  ['', [Validators.required, Validators.maxLength(100)]],
    birthDate: ['', Validators.required],
    gender:    [Gender.NotSpecified, Validators.required],

    email:   ['', [Validators.required, Validators.email, Validators.maxLength(200)]],
    phone:   ['', [Validators.required, Validators.maxLength(40)]],
    address: ['', [Validators.required, Validators.maxLength(300)]],

    emergencyContactName:  ['', Validators.maxLength(200)],
    emergencyContactPhone: ['', Validators.maxLength(40)],

    notes: ['', Validators.maxLength(1000)],
  });

  // ── Formulario de cambio de estado (PATCH /members/{id}/status) ─────────────
  readonly statusForm = this.fb.nonNullable.group({
    newStatus: [MemberStatus.Active, Validators.required],
  });

  ngOnInit(): void {
    this.memberService.getMemberById(this.id()).subscribe({
      next: (m) => {
        this.member.set(m);
        this.populateForms(m);
        this.loadingMember.set(false);
      },
      error: () => {
        this.errorLoad.set('No se pudo cargar el socio. Verifica el enlace e intenta nuevamente.');
        this.loadingMember.set(false);
      },
    });

    this.loadActiveAssignment();
  }

  loadActiveAssignment(): void {
    this.loadingAssignment.set(true);
    this.errorAssignment.set(null);

    this.assignmentService.getActiveAssignment(this.id()).subscribe({
      next: (dto) => {
        this.activeAssignment.set(dto);
        this.loadingAssignment.set(false);
      },
      error: () => {
        // 204 No Content → Angular puede lanzar error; tratar como sin membresía activa
        this.activeAssignment.set(null);
        this.loadingAssignment.set(false);
      },
    });
  }

  private populateForms(m: MemberDto): void {
    this.form.patchValue({
      firstName: m.firstName,
      lastName:  m.lastName,
      birthDate: m.birthDate,
      gender:    m.gender,
      email:     m.email   ?? '',
      phone:     m.phone   ?? '',
      address:   m.address ?? '',
      emergencyContactName:  m.emergencyContactName  ?? '',
      emergencyContactPhone: m.emergencyContactPhone ?? '',
      notes:     m.notes   ?? '',
    });

    this.statusForm.patchValue({ newStatus: m.status });
  }

  // ── Acciones sobre la membresía activa ────────────────────────────────────

  formatAmount(amount: number): string {
    return new Intl.NumberFormat('es-CL', { style: 'currency', currency: 'CLP' }).format(amount);
  }

  shortId(id: string): string {
    return `…${id.slice(-8)}`;
  }

  async registerPayment(): Promise<void> {
    const a = this.activeAssignment();
    if (!a || this.loadingAction()) return;

    const result = await this.dialog.confirmAction(
      'Registrar pago',
      `¿Confirmas el pago de ${this.formatAmount(a.amountSnapshot)}? El socio se reactivará automáticamente.`,
      'Sí, registrar pago',
    );
    if (!result.isConfirmed) return;

    this.loadingAction.set(true);
    this.errorAssignment.set(null);

    this.assignmentService.registerPayment(a.id).subscribe({
      next: () => {
        this.dialog.toast('Pago registrado exitosamente', 'success');
        this.loadingAction.set(false);
        this.loadActiveAssignment();
      },
      error: () => {
        this.errorAssignment.set('No se pudo registrar el pago. Intenta nuevamente.');
        this.loadingAction.set(false);
      },
    });
  }

  async freezeMembership(): Promise<void> {
    const a = this.activeAssignment();
    if (!a || this.loadingAction()) return;

    const result = await this.dialog.confirmAction(
      'Congelar membresía',
      'La membresía se pausará. Los días restantes quedarán en espera hasta que se descongele.',
      'Sí, congelar',
    );
    if (!result.isConfirmed) return;

    this.loadingAction.set(true);
    this.errorAssignment.set(null);

    this.assignmentService.freezeMembership(a.id).subscribe({
      next: () => {
        this.dialog.toast('Membresía congelada', 'success');
        this.loadingAction.set(false);
        this.loadActiveAssignment();
      },
      error: (err: HttpErrorResponse) => {
        this.errorAssignment.set(
          err.status === 422
            ? 'El plan no permite congelamiento.'
            : 'No se pudo congelar la membresía. Intenta nuevamente.',
        );
        this.loadingAction.set(false);
      },
    });
  }

  async unfreezeMembership(): Promise<void> {
    const a = this.activeAssignment();
    if (!a || this.loadingAction()) return;

    const result = await this.dialog.confirmAction(
      'Descongelar membresía',
      'La membresía se reanudará. La fecha de vencimiento se extenderá por los días congelados.',
      'Sí, descongelar',
    );
    if (!result.isConfirmed) return;

    this.loadingAction.set(true);
    this.errorAssignment.set(null);

    this.assignmentService.unfreezeMembership(a.id).subscribe({
      next: () => {
        this.dialog.toast('Membresía descongelada', 'success');
        this.loadingAction.set(false);
        this.loadActiveAssignment();
      },
      error: () => {
        this.errorAssignment.set('No se pudo descongelar la membresía. Intenta nuevamente.');
        this.loadingAction.set(false);
      },
    });
  }

  async cancelAssignment(): Promise<void> {
    const a = this.activeAssignment();
    if (!a || this.loadingAction()) return;

    const result = await this.dialog.confirmDanger(
      '¿Cancelar membresía?',
      `Esta acción cancelará la membresía vigente (${a.startDate} → ${a.endDate}). Esta operación no se puede deshacer.`,
      'Sí, cancelar membresía',
    );
    if (!result.isConfirmed) return;

    this.loadingAction.set(true);
    this.errorAssignment.set(null);

    this.assignmentService.cancelAssignment(a.id).subscribe({
      next: () => {
        this.dialog.toast('Membresía cancelada', 'success');
        this.loadingAction.set(false);
        this.loadActiveAssignment();
      },
      error: () => {
        this.errorAssignment.set('No se pudo cancelar la membresía. Intenta nuevamente.');
        this.loadingAction.set(false);
      },
    });
  }

  // ── Formulario de datos ───────────────────────────────────────────────────

  async onSubmit(): Promise<void> {
    if (this.form.invalid || this.loading()) {
      this.form.markAllAsTouched();
      return;
    }

    const confirm = await this.dialog.confirmAction(
      '¿Guardar cambios?',
      'Se actualizarán los datos del socio.',
      'Guardar',
    );
    if (!confirm.isConfirmed) return;

    this.loading.set(true);
    this.error.set(null);

    const raw = this.form.getRawValue();

    this.memberService
      .updateMember(this.id(), {
        firstName:             raw.firstName,
        lastName:              raw.lastName,
        birthDate:             raw.birthDate,
        gender:                Number(raw.gender) as Gender,
        email:                 raw.email   || null,
        phone:                 raw.phone   || null,
        address:               raw.address || null,
        emergencyContactName:  raw.emergencyContactName  || null,
        emergencyContactPhone: raw.emergencyContactPhone || null,
        notes:                 raw.notes || null,
      })
      .subscribe({
        next: async () => {
          this.loading.set(false);
          await this.dialog.toast('Datos guardados correctamente', 'success');
          this.router.navigate(['/app/members']);
        },
        error: (err: HttpErrorResponse) => {
          this.error.set(this.parseError(err));
          this.loading.set(false);
        },
      });
  }

  async onChangeStatus(): Promise<void> {
    if (this.statusForm.invalid || this.loadingStatus()) return;

    const raw         = this.statusForm.getRawValue();
    const newStatus   = Number(raw.newStatus) as MemberStatus;
    const statusLabel = this.statusOptions.find((s) => s.value === newStatus)?.label ?? '';

    const confirm = await this.dialog.confirmAction(
      'Cambiar estado',
      `El socio pasará a estado "${statusLabel}".`,
      'Confirmar',
    );
    if (!confirm.isConfirmed) return;

    this.loadingStatus.set(true);
    this.errorStatus.set(null);
    this.successStatus.set(false);

    this.memberService
      .changeMemberStatus(this.id(), { newStatus })
      .subscribe({
        next: () => {
          const current = this.member();
          if (current) this.member.set({ ...current, status: newStatus, statusLabel });
          this.successStatus.set(true);
          this.loadingStatus.set(false);
        },
        error: (err: HttpErrorResponse) => {
          this.errorStatus.set(this.parseError(err));
          this.loadingStatus.set(false);
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
    console.error('[MemberEdit] error HTTP', err.status, err.error);

    if (err.status === 0)   return 'No se pudo conectar con el servidor.';
    if (err.status === 401) return 'Sesión expirada. Por favor, inicia sesión nuevamente.';
    if (err.status === 403) return 'No tienes permisos para realizar esta acción.';
    if (err.status === 404) return 'Socio no encontrado.';

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

    return `Error ${err.status}: no se pudo guardar. Intenta nuevamente.`;
  }
}
