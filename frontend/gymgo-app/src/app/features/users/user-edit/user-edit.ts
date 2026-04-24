import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { UserRole } from '../../../core/models/auth.models';
import { ASSIGNABLE_ROLES } from '../models/user.models';

@Component({
  selector: 'app-user-edit',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './user-edit.html',
})
export class UserEditComponent {
  private readonly fb     = inject(FormBuilder);
  private readonly router = inject(Router);

  readonly loading              = signal(false);
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

  onSubmit(): void {
    if (this.form.invalid || this.loading()) return;
    this.loading.set(true);
    this.error.set(null);
    // TODO: llamar al servicio de actualización
  }

  onChangePassword(): void {
    if (this.passwordForm.invalid || this.loadingPassword()) return;
    const { newPassword, confirmPassword } = this.passwordForm.getRawValue();
    if (newPassword !== confirmPassword) {
      this.errorPassword.set('Las contraseñas no coinciden.');
      return;
    }
    this.loadingPassword.set(true);
    this.errorPassword.set(null);
    this.successPassword.set(false);
    // TODO: llamar al servicio de cambio de contraseña
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
