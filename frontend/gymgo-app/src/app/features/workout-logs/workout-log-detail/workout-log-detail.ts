import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { DecimalPipe, SlicePipe } from '@angular/common';
import { ReplacePipe } from '../../../shared/pipes/replace.pipe';
import { WorkoutLogService } from '../services/workout-log.service';
import { DialogService } from '../../../core/services/dialog.service';
import {
  WorkoutLogDto,
  WorkoutLogExerciseDto,
  WorkoutLogStatus,
  MuscleGroup,
  MUSCLE_GROUP_OPTIONS,
  muscleGroupBadgeClass,
} from '../models/workout-log.models';

@Component({
  selector: 'app-workout-log-detail',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, DecimalPipe, SlicePipe, ReplacePipe],
  templateUrl: './workout-log-detail.html',
})
export class WorkoutLogDetailComponent implements OnInit {
  private readonly fb                = inject(FormBuilder);
  private readonly route             = inject(ActivatedRoute);
  private readonly router            = inject(Router);
  private readonly workoutLogService = inject(WorkoutLogService);
  private readonly dialog            = inject(DialogService);

  readonly loading         = signal(false);
  readonly saving          = signal(false);
  readonly error           = signal<string | null>(null);
  readonly log             = signal<WorkoutLogDto | null>(null);
  readonly showAddForm     = signal(false);
  readonly editingId       = signal<string | null>(null);   // ejercicio en edición
  readonly savingExercise  = signal(false);
  readonly exerciseError   = signal<string | null>(null);

  readonly WorkoutLogStatus = WorkoutLogStatus;
  readonly muscleGroupOptions = MUSCLE_GROUP_OPTIONS;
  readonly muscleGroupBadgeClass = muscleGroupBadgeClass;

  // ── Formulario para agregar ejercicio ────────────────────────────────────
  readonly addForm = this.fb.nonNullable.group({
    exerciseName:    ['', [Validators.required, Validators.maxLength(200)]],
    muscleGroup:     [MuscleGroup.NotSpecified],
    sets:            [null as number | null],
    reps:            [null as number | null],
    weightKg:        [null as number | null],
    durationSeconds: [null as number | null],
    distanceMeters:  [null as number | null],
    notes:           [''],
  });

  // ── Formulario para editar ejercicio ─────────────────────────────────────
  readonly editForm = this.fb.nonNullable.group({
    exerciseName:    ['', [Validators.required, Validators.maxLength(200)]],
    muscleGroup:     [MuscleGroup.NotSpecified],
    sortOrder:       [0],
    sets:            [null as number | null],
    reps:            [null as number | null],
    weightKg:        [null as number | null],
    durationSeconds: [null as number | null],
    distanceMeters:  [null as number | null],
    notes:           [''],
  });

  private logId!: string;

  ngOnInit(): void {
    this.logId = this.route.snapshot.paramMap.get('id')!;
    this.loadLog();
  }

  loadLog(): void {
    this.loading.set(true);
    this.error.set(null);
    this.workoutLogService.getById(this.logId).subscribe({
      next: (data) => {
        this.log.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('No se pudo cargar la sesión.');
        this.loading.set(false);
      },
    });
  }

  get isDraft(): boolean {
    return this.log()?.status === WorkoutLogStatus.Draft;
  }

  formatDate(dateStr: string): string {
    if (!dateStr) return '-';
    const [y, m, d] = dateStr.split('-');
    return `${d}/${m}/${y}`;
  }

  // ── Completar sesión ──────────────────────────────────────────────────────
  async onComplete(): Promise<void> {
    const confirm = await this.dialog.confirmDanger(
      'Completar sesión',
      '¿Confirmas que la sesión de entrenamiento ha finalizado? Esta acción es irreversible.',
      'Sí, completar',
    );
    if (!confirm.isConfirmed) return;

    this.saving.set(true);
    this.workoutLogService.complete(this.logId).subscribe({
      next: () => {
        this.saving.set(false);
        this.dialog.toast('Sesión completada exitosamente.', 'success');
        this.loadLog();
      },
      error: (err) => {
        const detail = err?.error?.detail ?? err?.error?.title;
        this.dialog.toast(detail ?? 'No se pudo completar la sesión.', 'error');
        this.saving.set(false);
      },
    });
  }

  // ── Eliminar log ──────────────────────────────────────────────────────────
  async onDelete(): Promise<void> {
    const confirm = await this.dialog.confirmDanger(
      'Eliminar sesión',
      'Se eliminará la sesión y todos sus ejercicios. ¿Deseas continuar?',
      'Eliminar',
    );
    if (!confirm.isConfirmed) return;

    this.saving.set(true);
    this.workoutLogService.delete(this.logId).subscribe({
      next: async () => {
        this.saving.set(false);
        await this.dialog.success('Sesión eliminada', '');
        this.router.navigate(['/app/workout-logs']);
      },
      error: () => {
        this.dialog.toast('No se pudo eliminar la sesión.', 'error');
        this.saving.set(false);
      },
    });
  }

  // ── Agregar ejercicio ─────────────────────────────────────────────────────
  toggleAddForm(): void {
    this.showAddForm.update(v => !v);
    if (!this.showAddForm()) this.addForm.reset({ muscleGroup: MuscleGroup.NotSpecified });
    this.exerciseError.set(null);
  }

  isAddInvalid(field: string): boolean {
    const ctrl = this.addForm.get(field);
    return !!(ctrl?.invalid && ctrl.touched);
  }

  onAddExercise(): void {
    if (this.addForm.invalid || this.savingExercise()) {
      this.addForm.markAllAsTouched();
      return;
    }
    const raw = this.addForm.getRawValue();
    this.savingExercise.set(true);
    this.exerciseError.set(null);

    this.workoutLogService.addExercise(this.logId, {
      exerciseName:    raw.exerciseName.trim(),
      muscleGroup:     Number(raw.muscleGroup) as MuscleGroup,
      sets:            raw.sets ? Number(raw.sets) : null,
      reps:            raw.reps ? Number(raw.reps) : null,
      weightKg:        raw.weightKg ? Number(raw.weightKg) : null,
      durationSeconds: raw.durationSeconds ? Number(raw.durationSeconds) : null,
      distanceMeters:  raw.distanceMeters ? Number(raw.distanceMeters) : null,
      notes:           raw.notes.trim() || null,
    }).subscribe({
      next: () => {
        this.savingExercise.set(false);
        this.addForm.reset({ muscleGroup: MuscleGroup.NotSpecified });
        this.showAddForm.set(false);
        this.loadLog();
      },
      error: (err) => {
        const detail = err?.error?.detail ?? err?.error?.title;
        this.exerciseError.set(detail ?? 'No se pudo agregar el ejercicio.');
        this.savingExercise.set(false);
      },
    });
  }

  // ── Editar ejercicio ──────────────────────────────────────────────────────
  startEdit(exercise: WorkoutLogExerciseDto): void {
    this.editingId.set(exercise.id);
    this.exerciseError.set(null);
    this.editForm.patchValue({
      exerciseName:    exercise.exerciseName,
      muscleGroup:     exercise.muscleGroup,
      sortOrder:       exercise.sortOrder,
      sets:            exercise.sets,
      reps:            exercise.reps,
      weightKg:        exercise.weightKg,
      durationSeconds: exercise.durationSeconds,
      distanceMeters:  exercise.distanceMeters,
      notes:           exercise.notes ?? '',
    });
  }

  cancelEdit(): void {
    this.editingId.set(null);
    this.exerciseError.set(null);
  }

  isEditInvalid(field: string): boolean {
    const ctrl = this.editForm.get(field);
    return !!(ctrl?.invalid && ctrl.touched);
  }

  onUpdateExercise(exerciseId: string): void {
    if (this.editForm.invalid || this.savingExercise()) {
      this.editForm.markAllAsTouched();
      return;
    }
    const raw = this.editForm.getRawValue();
    this.savingExercise.set(true);
    this.exerciseError.set(null);

    this.workoutLogService.updateExercise(this.logId, exerciseId, {
      exerciseName:    raw.exerciseName.trim(),
      muscleGroup:     Number(raw.muscleGroup) as MuscleGroup,
      sortOrder:       raw.sortOrder,
      sets:            raw.sets ? Number(raw.sets) : null,
      reps:            raw.reps ? Number(raw.reps) : null,
      weightKg:        raw.weightKg ? Number(raw.weightKg) : null,
      durationSeconds: raw.durationSeconds ? Number(raw.durationSeconds) : null,
      distanceMeters:  raw.distanceMeters ? Number(raw.distanceMeters) : null,
      notes:           raw.notes.trim() || null,
    }).subscribe({
      next: () => {
        this.savingExercise.set(false);
        this.editingId.set(null);
        this.loadLog();
      },
      error: (err) => {
        const detail = err?.error?.detail ?? err?.error?.title;
        this.exerciseError.set(detail ?? 'No se pudo actualizar el ejercicio.');
        this.savingExercise.set(false);
      },
    });
  }

  // ── Eliminar ejercicio ────────────────────────────────────────────────────
  async onRemoveExercise(exercise: WorkoutLogExerciseDto): Promise<void> {
    const confirm = await this.dialog.confirmDanger(
      'Eliminar ejercicio',
      `¿Eliminar "${exercise.exerciseName}" de esta sesión?`,
      'Eliminar',
    );
    if (!confirm.isConfirmed) return;

    this.workoutLogService.removeExercise(this.logId, exercise.id).subscribe({
      next: () => {
        this.dialog.toast('Ejercicio eliminado.', 'success');
        this.loadLog();
      },
      error: () => this.dialog.toast('No se pudo eliminar el ejercicio.', 'error'),
    });
  }

  /** Resumen de métricas para mostrar en la card */
  exerciseSummary(e: WorkoutLogExerciseDto): string {
    const parts: string[] = [];
    if (e.sets && e.reps)         parts.push(`${e.sets} × ${e.reps} rep`);
    else if (e.sets)              parts.push(`${e.sets} series`);
    if (e.weightKg != null)       parts.push(`${e.weightKg} kg`);
    if (e.durationSeconds != null) {
      const min = Math.floor(e.durationSeconds / 60);
      const sec = e.durationSeconds % 60;
      parts.push(min > 0 ? `${min}m ${sec}s` : `${sec}s`);
    }
    if (e.distanceMeters != null) parts.push(`${e.distanceMeters} m`);
    return parts.join('  ·  ') || 'Sin métricas';
  }
}
