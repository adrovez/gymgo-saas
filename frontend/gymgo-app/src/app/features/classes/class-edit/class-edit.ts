import { Component, OnInit, inject, input, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { ClassService } from '../services/class.service';
import { DialogService } from '../../../core/services/dialog.service';
import {
  GymClassDto,
  ClassScheduleDto,
  CLASS_CATEGORY_OPTIONS,
  ClassCategory,
  PRESET_COLORS,
  DAY_OF_WEEK_LABELS,
  CreateClassScheduleRequest,
} from '../models/class.models';

@Component({
  selector: 'app-class-edit',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './class-edit.html',
})
export class ClassEditComponent implements OnInit {
  readonly id = input.required<string>();

  private readonly fb           = inject(FormBuilder);
  private readonly router       = inject(Router);
  private readonly classService = inject(ClassService);
  private readonly dialog       = inject(DialogService);

  readonly loading        = signal(true);
  readonly saving         = signal(false);
  readonly errorLoad      = signal<string | null>(null);
  readonly error          = signal<string | null>(null);
  readonly gymClass       = signal<GymClassDto | null>(null);

  // Schedule form visibility
  readonly showScheduleForm = signal(false);
  readonly editingSchedule  = signal<ClassScheduleDto | null>(null);
  readonly savingSchedule   = signal(false);
  readonly scheduleError    = signal<string | null>(null);

  readonly CATEGORY_OPTIONS = CLASS_CATEGORY_OPTIONS;
  readonly PRESET_COLORS    = PRESET_COLORS;
  readonly ClassCategory    = ClassCategory;
  readonly dayLabels        = DAY_OF_WEEK_LABELS;

  readonly DAY_OPTIONS = [1,2,3,4,5,6,0].map(d => ({ value: d, label: DAY_OF_WEEK_LABELS[d] }));

  readonly form = this.fb.nonNullable.group({
    name:            ['', [Validators.required, Validators.maxLength(100)]],
    description:     ['', Validators.maxLength(500)],
    category:        [ClassCategory.Other],
    color:           ['#3B82F6'],
    durationMinutes: [60, [Validators.required, Validators.min(1), Validators.max(300)]],
    maxCapacity:     [20, [Validators.required, Validators.min(1), Validators.max(999)]],
  });

  readonly scheduleForm = this.fb.nonNullable.group({
    dayOfWeek:      [1, Validators.required],
    startTime:      ['07:00', Validators.required],
    endTime:        ['08:00', Validators.required],
    instructorName: [''],
    room:           [''],
    maxCapacity:    [null as number | null],
  });

  ngOnInit(): void {
    this.loadClass();
  }

  loadClass(): void {
    this.loading.set(true);
    this.errorLoad.set(null);

    this.classService.getClassById(this.id()).subscribe({
      next: (c) => {
        this.gymClass.set(c);
        this.form.patchValue({
          name:            c.name,
          description:     c.description ?? '',
          category:        c.category,
          color:           c.color ?? '#3B82F6',
          durationMinutes: c.durationMinutes,
          maxCapacity:     c.maxCapacity,
        });
        this.loading.set(false);
      },
      error: () => {
        this.errorLoad.set('No se pudo cargar la clase.');
        this.loading.set(false);
      },
    });
  }

  isInvalid(field: keyof typeof this.form.controls): boolean {
    const control = this.form.get(field as string);
    return !!(control?.invalid && control?.touched);
  }

  selectColor(hex: string): void {
    this.form.controls.color.setValue(hex);
  }

  async onSubmit(): Promise<void> {
    if (this.form.invalid || this.saving()) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.error.set(null);

    const raw = this.form.getRawValue();

    this.classService.updateClass(this.id(), {
      name:            raw.name.trim(),
      description:     raw.description?.trim() || null,
      category:        raw.category,
      color:           raw.color || null,
      durationMinutes: raw.durationMinutes,
      maxCapacity:     raw.maxCapacity,
    }).subscribe({
      next: async () => {
        this.saving.set(false);
        await this.dialog.success('¡Clase actualizada!', 'Los cambios fueron guardados exitosamente.');
        this.router.navigate(['/app/classes']);
      },
      error: (err: HttpErrorResponse) => {
        this.error.set(this.parseError(err));
        this.saving.set(false);
      },
    });
  }

  // ── Horarios ───────────────────────────────────────────────────────────────

  openScheduleForm(schedule?: ClassScheduleDto): void {
    this.editingSchedule.set(schedule ?? null);
    this.scheduleError.set(null);

    if (schedule) {
      this.scheduleForm.patchValue({
        dayOfWeek:      schedule.dayOfWeek,
        startTime:      schedule.startTime,
        endTime:        schedule.endTime,
        instructorName: schedule.instructorName ?? '',
        room:           schedule.room ?? '',
        maxCapacity:    schedule.maxCapacity ?? null,
      });
    } else {
      this.scheduleForm.reset({
        dayOfWeek: 1, startTime: '07:00', endTime: '08:00',
        instructorName: '', room: '', maxCapacity: null,
      });
    }
    this.showScheduleForm.set(true);
  }

  closeScheduleForm(): void {
    this.showScheduleForm.set(false);
    this.editingSchedule.set(null);
  }

  saveSchedule(): void {
    if (this.scheduleForm.invalid || this.savingSchedule()) {
      this.scheduleForm.markAllAsTouched();
      return;
    }

    this.savingSchedule.set(true);
    this.scheduleError.set(null);

    const raw = this.scheduleForm.getRawValue();
    const request: CreateClassScheduleRequest = {
      dayOfWeek:      raw.dayOfWeek,
      startTime:      raw.startTime,
      endTime:        raw.endTime,
      instructorName: raw.instructorName?.trim() || null,
      room:           raw.room?.trim() || null,
      maxCapacity:    raw.maxCapacity ?? null,
    };

    const editing = this.editingSchedule();

    const onNext = () => {
      this.savingSchedule.set(false);
      this.closeScheduleForm();
      this.loadClass();
    };
    const onError = (err: HttpErrorResponse) => {
      this.scheduleError.set(this.parseError(err));
      this.savingSchedule.set(false);
    };

    if (editing) {
      this.classService.updateSchedule(editing.id, request).subscribe({
        next: onNext, error: onError,
      });
    } else {
      this.classService.createSchedule(this.id(), request).subscribe({
        next: onNext, error: onError,
      });
    }
  }

  async deleteSchedule(schedule: ClassScheduleDto): Promise<void> {
    const confirm = await this.dialog.confirmDanger(
      '¿Eliminar horario?',
      `Se eliminará el horario del ${schedule.dayLabel} ${schedule.startTime}–${schedule.endTime}.`,
      'Eliminar',
    );
    if (!confirm.isConfirmed) return;

    this.classService.deleteSchedule(schedule.id).subscribe({
      next: () => this.loadClass(),
      error: () => this.error.set('No se pudo eliminar el horario.'),
    });
  }

  cancel(): void {
    this.router.navigate(['/app/classes']);
  }

  private parseError(err: HttpErrorResponse): string {
    if (err.status === 0)   return 'No se pudo conectar con el servidor.';
    if (err.status === 401) return 'Sesión expirada.';
    if (err.status === 404) return 'Clase no encontrada.';
    if (err.status === 422) {
      const body = typeof err.error === 'string'
        ? (() => { try { return JSON.parse(err.error); } catch { return null; } })()
        : err.error;
      if (body?.detail) return body.detail;
      return 'Operación no permitida.';
    }
    if (err.status === 400) {
      const body = typeof err.error === 'string'
        ? (() => { try { return JSON.parse(err.error); } catch { return null; } })()
        : err.error;
      if (body?.detail) return body.detail;
      if (body?.errors) {
        const msgs = (Object.values(body.errors) as string[][]).flat();
        if (msgs.length) return msgs[0];
      }
      return 'Datos inválidos.';
    }
    return `Error ${err.status}.`;
  }
}
