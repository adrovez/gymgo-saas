import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { ClassService } from '../services/class.service';
import { DialogService } from '../../../core/services/dialog.service';
import {
  CLASS_CATEGORY_OPTIONS,
  ClassCategory,
  PRESET_COLORS,
} from '../models/class.models';

@Component({
  selector: 'app-class-create',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './class-create.html',
})
export class ClassCreateComponent {
  private readonly fb           = inject(FormBuilder);
  private readonly router       = inject(Router);
  private readonly classService = inject(ClassService);
  private readonly dialog       = inject(DialogService);

  readonly loading          = false;
  readonly error            = { value: null as string | null };

  readonly CATEGORY_OPTIONS = CLASS_CATEGORY_OPTIONS;
  readonly PRESET_COLORS    = PRESET_COLORS;
  readonly ClassCategory    = ClassCategory;

  private _loading = false;
  private _error: string | null = null;

  get isLoading() { return this._loading; }
  get formError() { return this._error; }

  readonly form = this.fb.nonNullable.group({
    name:            ['', [Validators.required, Validators.maxLength(100)]],
    description:     ['', Validators.maxLength(500)],
    category:        [ClassCategory.Other],
    color:           ['#3B82F6'],
    durationMinutes: [60, [Validators.required, Validators.min(1), Validators.max(300)]],
    maxCapacity:     [20, [Validators.required, Validators.min(1), Validators.max(999)]],
  });

  isInvalid(field: keyof typeof this.form.controls): boolean {
    const control = this.form.get(field as string);
    return !!(control?.invalid && control?.touched);
  }

  selectColor(hex: string): void {
    this.form.controls.color.setValue(hex);
  }

  async onSubmit(): Promise<void> {
    if (this.form.invalid || this._loading) {
      this.form.markAllAsTouched();
      return;
    }

    this._loading = true;
    this._error   = null;

    const raw = this.form.getRawValue();

    this.classService.createClass({
      name:            raw.name.trim(),
      description:     raw.description?.trim() || null,
      category:        raw.category,
      color:           raw.color || null,
      durationMinutes: raw.durationMinutes,
      maxCapacity:     raw.maxCapacity,
    }).subscribe({
      next: async () => {
        this._loading = false;
        await this.dialog.success('¡Clase creada!', `La clase "${raw.name.trim()}" fue creada exitosamente.`);
        this.router.navigate(['/app/classes']);
      },
      error: (err: HttpErrorResponse) => {
        this._error   = this.parseError(err);
        this._loading = false;
      },
    });
  }

  cancel(): void {
    this.router.navigate(['/app/classes']);
  }

  private parseError(err: HttpErrorResponse): string {
    if (err.status === 0)   return 'No se pudo conectar con el servidor.';
    if (err.status === 401) return 'Sesión expirada. Por favor, inicia sesión nuevamente.';
    if (err.status === 400) {
      const body = typeof err.error === 'string'
        ? (() => { try { return JSON.parse(err.error); } catch { return null; } })()
        : err.error;
      if (body?.detail) return body.detail;
      if (body?.errors) {
        const msgs = (Object.values(body.errors) as string[][]).flat();
        if (msgs.length) return msgs[0];
      }
      return 'Los datos enviados no son válidos.';
    }
    return `Error ${err.status}: no se pudo crear la clase.`;
  }
}
