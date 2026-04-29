import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { UserRole } from '../../../core/models/auth.models';
import { DialogService } from '../../../core/services/dialog.service';
import { ASSIGNABLE_ROLES } from '../models/user.models';
import { UsersService } from '../services/users.service';

@Component({
  selector: 'app-user-create',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './user-create.html',
})
export class UserCreateComponent {
  private readonly fb           = inject(FormBuilder);
  private readonly router       = inject(Router);
  private readonly usersService = inject(UsersService);
  private readonly dialog       = inject(DialogService);

  readonly loading         = signal(false);
  readonly error           = signal<string | null>(null);
  readonly showPassword    = signal(false);
  readonly assignableRoles = ASSIGNABLE_ROLES;

  readonly form = this.fb.nonNullable.group({
    fullName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    email:    ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    role:     [UserRole.GymStaff, Validators.required],
  });

  onSubmit(): void {
    if (this.form.invalid || this.loading()) return;
    this.form.markAllAsTouched();
    if (this.form.invalid) return;

    this.loading.set(true);
    this.error.set(null);

    const { fullName, email, password, role } = this.form.getRawValue();

    this.usersService.createUser({ fullName, email, password, role }).subscribe({
      next: () => {
        this.dialog.toast('Usuario creado correctamente.', 'success');
        this.router.navigate(['/app/users']);
      },
      error: (err) => {
        this.loading.set(false);
        const msg = err?.error?.detail ?? err?.error?.message ?? 'Error al crear el usuario.';
        this.error.set(msg);
      },
    });
  }

  togglePassword(): void {
    this.showPassword.update((v) => !v);
  }

  isInvalid(field: 'fullName' | 'email' | 'password' | 'role'): boolean {
    const control = this.form.get(field);
    return !!(control?.invalid && control?.touched);
  }

  cancel(): void {
    this.router.navigate(['/app/users']);
  }
}
