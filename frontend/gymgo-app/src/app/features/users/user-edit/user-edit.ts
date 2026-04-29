import { Component, inject, signal, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { UserRole } from '../../../core/models/auth.models';
import { DialogService } from '../../../core/services/dialog.service';
import { ASSIGNABLE_ROLES } from '../models/user.models';
import { UsersService } from '../services/users.service';

@Component({
  selector: 'app-user-edit',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './user-edit.html',
})
export class UserEditComponent implements OnInit {
  private readonly fb           = inject(FormBuilder);
  private readonly router       = inject(Router);
  private readonly route        = inject(ActivatedRoute);
  private readonly usersService = inject(UsersService);
  private readonly dialog       = inject(DialogService);

  private userId = '';

  readonly loading              = signal(false);
  readonly loadingData          = signal(true);
  readonly loadingPassword      = signal(false);
  readonly error                = signal<string | null>(null);
  readonly errorPassword        = signal<string | null>(null);
  readonly successPassword      = signal(false);
  readonly showNewPassword      = signal(false);
  readonly showConfirmPassword  = signal(false);
  readonly assignableRoles      = ASSIGNABLE_ROLES;

  // Formulario de datos generales
  readonly form = this.fb.nonNullable.group({
    fullName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    role:     [UserRole.GymStaff, Validators.required],
    isActive: [true],
  });

  // Formulario de cambio de contraseña
  readonly passwordForm = this.fb.nonNullable.group({
    newPassword:     ['', [Validators.required, Validators.minLength(6)]],
    confirmPassword: ['', Validators.required],
  });

  ngOnInit(): void {
    this.userId = this.route.snapshot.paramMap.get('id') ?? '';
    this.loadUser();
  }

  private loadUser(): void {
    this.loadingData.set(true);
    this.usersService.getUserById(this.userId).subscribe({
      next: (user) => {
        this.form.patchValue({
          fullName: user.fullName,
          role:     user.role,
          isActive: user.isActive,
        });
        this.loadingData.set(false);
      },
      error: () => {
        this.loadingData.set(false);
        this.error.set('No se pudo cargar el usuario.');
      },
    });
  }

  onSubmit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid || this.loading()) return;

    this.loading.set(true);
    this.error.set(null);

    const { fullName, role, isActive } = this.form.getRawValue();

    this.usersService.updateUser(this.userId, { fullName, role, isActive }).subscribe({
      next: () => {
        this.loading.set(false);
        this.dialog.toast('Usuario actualizado correctamente.', 'success');
      },
      error: (err) => {
        this.loading.set(false);
        const msg = err?.error?.detail ?? err?.error?.message ?? 'Error al actualizar el usuario.';
        this.error.set(msg);
      },
    });
  }

  onChangePassword(): void {
    this.passwordForm.markAllAsTouched();
    if (this.passwordForm.invalid || this.loadingPassword()) return;

    const { newPassword, confirmPassword } = this.passwordForm.getRawValue();
    if (newPassword !== confirmPassword) {
      this.errorPassword.set('Las contraseñas no coinciden.');
      return;
    }

    this.loadingPassword.set(true);
    this.errorPassword.set(null);
    this.successPassword.set(false);

    this.usersService.changePassword(this.userId, { newPassword }).subscribe({
      next: () => {
        this.loadingPassword.set(false);
        this.successPassword.set(true);
        this.passwordForm.reset();
      },
      error: (err) => {
        this.loadingPassword.set(false);
        const msg = err?.error?.detail ?? err?.error?.message ?? 'Error al cambiar la contraseña.';
        this.errorPassword.set(msg);
      },
    });
  }

  isInvalid(field: 'fullName' | 'role' | 'isActive'): boolean {
    const control = this.form.get(field);
    return !!(control?.invalid && control?.touched);
  }

  isPasswordInvalid(field: 'newPassword' | 'confirmPassword'): boolean {
    const control = this.passwordForm.get(field);
    return !!(control?.invalid && control?.touched);
  }

  cancel(): void {
    this.router.navigate(['/app/users']);
  }
}
