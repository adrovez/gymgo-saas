import { Component, OnInit, inject, signal, computed } from '@angular/core';
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

export type ExerciseType = 'strength' | 'cardio' | 'timed';

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
  readonly editingId       = signal<string | null>(null);
  readonly savingExercise  = signal(false);
  readonly exerciseError   = signal<string | null>(null);
  readonly addFormType     = signal<ExerciseType>('strength');
  readonly editFormType    = signal<ExerciseType>('strength');

  readonly WorkoutLogStatus    = WorkoutLogStatus;
  readonly muscleGroupOptions  = MUSCLE_GROUP_OPTIONS;
  readonly muscleGroupBadgeClass = muscleGroupBadgeClass;

  // ── Métricas de sesión computadas ───────────────────────────────────────────
  readonly totalSeries = computed(() =>
    this.log()?.exercises.reduce((s, e) => s + (e.sets ?? 0), 0) ?? 0
  );

  readonly totalReps = computed(() =>
    this.log()?.exercises.reduce((s, e) => s + ((e.sets ?? 0) * (e.reps ?? 0)), 0) ?? 0
  );

  readonly maxWeight = computed(() => {
    const weights = this.log()?.exercises.map(e => e.weightKg ?? 0) ?? [];
    return weights.length > 0 ? Math.max(...weights) : 0;
  });

  // ── Formulario agregar ejercicio ─────────────────────────────────────────────
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

  // ── Formulario editar ejercicio ──────────────────────────────────────────────
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
      next:  (data) => { this.log.set(data); this.loading.set(false); },
      error: ()     => { this.error.set('No se pudo cargar la sesión.'); this.loading.set(false); },
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

  formatDuration(seconds: number): string {
    const min = Math.floor(seconds / 60);
    const sec = seconds % 60;
    if (min > 0 && sec > 0) return `${min}m ${sec}s`;
    if (min > 0) return `${min} min`;
    return `${sec}s`;
  }

  // ── Tipo de ejercicio ─────────────────────────────────────────────────────────
  private detectExerciseType(e: WorkoutLogExerciseDto): ExerciseType {
    const hasStrength = e.weightKg != null || (e.sets != null && e.reps != null);
    if (e.distanceMeters != null || (e.durationSeconds != null && !hasStrength)) return 'cardio';
    if (e.durationSeconds != null && e.sets != null && !e.reps) return 'timed';
    return 'strength';
  }

  setAddType(type: ExerciseType): void {
    this.addFormType.set(type);
    if (type === 'cardio')   this.addForm.patchValue({ sets: null, reps: null, weightKg: null });
    if (type === 'strength') this.addForm.patchValue({ durationSeconds: null, distanceMeters: null });
    if (type === 'timed')    this.addForm.patchValue({ reps: null, weightKg: null, distanceMeters: null });
  }

  setEditType(type: ExerciseType): void {
    this.editFormType.set(type);
    if (type === 'cardio')   this.editForm.patchValue({ sets: null, reps: null, weightKg: null });
    if (type === 'strength') this.editForm.patchValue({ durationSeconds: null, distanceMeters: null });
    if (type === 'timed')    this.editForm.patchValue({ reps: null, weightKg: null, distanceMeters: null });
  }

  // ── Toggle expand/collapse de tarjeta de ejercicio ───────────────────────────
  toggleExCard(exercise: WorkoutLogExerciseDto): void {
    if (this.editingId() === exercise.id) {
      this.cancelEdit();
    } else {
      this.startEdit(exercise);
    }
  }

  // ── Completar sesión ──────────────────────────────────────────────────────────
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

  // ── Eliminar log ──────────────────────────────────────────────────────────────
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

  // ── Agregar ejercicio ─────────────────────────────────────────────────────────
  toggleAddForm(): void {
    this.editingId.set(null);
    this.showAddForm.update(v => !v);
    if (!this.showAddForm()) {
      this.addForm.reset({ muscleGroup: MuscleGroup.NotSpecified });
      this.addFormType.set('strength');
    }
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
      sets:            raw.sets    ? Number(raw.sets)            : null,
      reps:            raw.reps    ? Number(raw.reps)            : null,
      weightKg:        raw.weightKg        ? Number(raw.weightKg)        : null,
      durationSeconds: raw.durationSeconds ? Number(raw.durationSeconds) : null,
      distanceMeters:  raw.distanceMeters  ? Number(raw.distanceMeters)  : null,
      notes:           raw.notes.trim() || null,
    }).subscribe({
      next: () => {
        this.savingExercise.set(false);
        this.addForm.reset({ muscleGroup: MuscleGroup.NotSpecified });
        this.addFormType.set('strength');
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

  // ── Editar ejercicio ──────────────────────────────────────────────────────────
  startEdit(exercise: WorkoutLogExerciseDto): void {
    this.showAddForm.set(false);
    this.editingId.set(exercise.id);
    this.editFormType.set(this.detectExerciseType(exercise));
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
      sets:            raw.sets            ? Number(raw.sets)            : null,
      reps:            raw.reps            ? Number(raw.reps)            : null,
      weightKg:        raw.weightKg        ? Number(raw.weightKg)        : null,
      durationSeconds: raw.durationSeconds ? Number(raw.durationSeconds) : null,
      distanceMeters:  raw.distanceMeters  ? Number(raw.distanceMeters)  : null,
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

  // ── Eliminar ejercicio ────────────────────────────────────────────────────────
  async onRemoveExercise(exercise: WorkoutLogExerciseDto): Promise<void> {
    const confirm = await this.dialog.confirmDanger(
      'Eliminar ejercicio',
      `¿Eliminar "${exercise.exerciseName}" de esta sesión?`,
      'Eliminar',
    );
    if (!confirm.isConfirmed) return;

    this.workoutLogService.removeExercise(this.logId, exercise.id).subscribe({
      next:  () => { this.dialog.toast('Ejercicio eliminado.', 'success'); this.loadLog(); },
      error: () => this.dialog.toast('No se pudo eliminar el ejercicio.', 'error'),
    });
  }
}
