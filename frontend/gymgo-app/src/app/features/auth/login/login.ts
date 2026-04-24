import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './login.html',
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly showPassword = signal(false);

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    tenantId: [''],
  });

  onSubmit(): void {
    if (this.form.invalid || this.loading()) return;

    this.loading.set(true);
    this.error.set(null);

    const { email, password, tenantId } = this.form.getRawValue();

    this.auth.login({ email, password }, tenantId || undefined).subscribe({
      next: () => {
        this.loading.set(false);
        this.router.navigate(['/app']);
      },
      error: (err) => {
        this.loading.set(false);
        if (err.status === 400 || err.status === 422) {
          this.error.set('Credenciales incorrectas. Verifica tu email y contraseña.');
        } else {
          this.error.set('Error de conexión. Intenta nuevamente.');
        }
      },
    });
  }

  togglePassword(): void {
    this.showPassword.update((v) => !v);
  }

  isInvalid(field: 'email' | 'password'): boolean {
    const control = this.form.get(field);
    return !!(control?.invalid && control?.touched);
  }

  readonly currentYear = new Date().getFullYear();
}
