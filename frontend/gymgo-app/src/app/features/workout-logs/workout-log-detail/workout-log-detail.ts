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
    this.log()?.exercises.reduce((s, e) => s + (e.actualSets ?? 0), 0) ?? 0
  );

  readonly totalReps = computed(() =>
    this.log()?.exercises.reduce((s, e) => s + ((e.actualSets ?? 0) * (e.actualReps ?? 0)), 0) ?? 0
  );

  readonly maxWeight = computed(() => {
    const weights = this.log()?.exercises.map(e => e.actualWeightKg ?? 0) ?? [];
    return weights.length > 0 ? Math.max(...weights) : 0;
  });

  // ── Formulario agregar ejercicio ─────────────────────────────────────────────
  readonly addForm = this.fb.nonNullable.group({
    exerciseName:          ['', [Validators.required, Validators.maxLength(200)]],
    muscleGroup:           [MuscleGroup.NotSpecified],
    actualSets:            [null as number | null],
    actualReps:            [null as number | null],
    actualWeightKg:        [null as number | null],
    actualDurationMinutes: [null as number | null],
    actualDistanceMeters:  [null as number | null],
    notes:                 [''],
  });

  // ── Formulario editar ejercicio ──────────────────────────────────────────────
  readonly editForm = this.fb.nonNullable.group({
    exerciseName:          ['', [Validators.required, Validators.maxLength(200)]],
    muscleGroup:           [MuscleGroup.NotSpecified],
    sortOrder:             [0],
    actualSets:            [null as number | null],
    actualReps:            [null as number | null],
    actualWeightKg:        [null as number | null],
    actualDurationMinutes: [null as number | null],
    actualDistanceMeters:  [null as number | null],
    notes:                 [''],
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

  formatDuration(minutes: number): string {
    if (minutes < 60) return `${minutes} min`;
    const h = Math.floor(minutes / 60);
    const m = minutes % 60;
    return m > 0 ? `${h}h ${m}min` : `${h}h`;
  }

  // ── Tipo de ejercicio ─────────────────────────────────────────────────────────
  private detectExerciseType(e: WorkoutLogExerciseDto): ExerciseType {
    const hasStrength = e.actualWeightKg != null || (e.actualSets != null && e.actualReps != null);
    if (e.actualDistanceMeters != null || (e.actualDurationMinutes != null && !hasStrength)) return 'cardio';
    if (e.actualDurationMinutes != null && e.actualSets != null && !e.actualReps) return 'timed';
    return 'strength';
  }

  setAddType(type: ExerciseType): void {
    this.addFormType.set(type);
    if (type === 'cardio')   this.addForm.patchValue({ actualSets: null, actualReps: null, actualWeightKg: null });
    if (type === 'strength') this.addForm.patchValue({ actualDurationMinutes: null, actualDistanceMeters: null });
    if (type === 'timed')    this.addForm.patchValue({ actualReps: null, actualWeightKg: null, actualDistanceMeters: null });
  }

  setEditType(type: ExerciseType): void {
    this.editFormType.set(type);
    if (type === 'cardio')   this.editForm.patchValue({ actualSets: null, actualReps: null, actualWeightKg: null });
    if (type === 'strength') this.editForm.patchValue({ actualDurationMinutes: null, actualDistanceMeters: null });
    if (type === 'timed')    this.editForm.patchValue({ actualReps: null, actualWeightKg: null, actualDistanceMeters: null });
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
        const planId = this.log()?.workoutPlanId;
        if (planId) {
          this.router.navigate(['/app/workout-plans', planId]);
        } else {
          this.router.navigate(['/app/workout-plans']);
        }
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
      workoutPlanExerciseId: null,
      isExtra:               true,
      exerciseName:          raw.exerciseName.trim(),
      muscleGroup:           Number(raw.muscleGroup) as MuscleGroup,
      actualSets:            raw.actualSets            ? Number(raw.actualSets)            : null,
      actualReps:            raw.actualReps            ? Number(raw.actualReps)            : null,
      actualWeightKg:        raw.actualWeightKg        ? Number(raw.actualWeightKg)        : null,
      actualDurationMinutes: raw.actualDurationMinutes ? Number(raw.actualDurationMinutes) : null,
      actualDistanceMeters:  raw.actualDistanceMeters  ? Number(raw.actualDistanceMeters)  : null,
      notes:                 raw.notes.trim() || null,
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
      exerciseName:          exercise.exerciseName,
      muscleGroup:           exercise.muscleGroup,
      sortOrder:             exercise.sortOrder,
      actualSets:            exercise.actualSets,
      actualReps:            exercise.actualReps,
      actualWeightKg:        exercise.actualWeightKg,
      actualDurationMinutes: exercise.actualDurationMinutes,
      actualDistanceMeters:  exercise.actualDistanceMeters,
      notes:                 exercise.notes ?? '',
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
      exerciseName:          raw.exerciseName.trim(),
      muscleGroup:           Number(raw.muscleGroup) as MuscleGroup,
      sortOrder:             raw.sortOrder,
      actualSets:            raw.actualSets            ? Number(raw.actualSets)            : null,
      actualReps:            raw.actualReps            ? Number(raw.actualReps)            : null,
      actualWeightKg:        raw.actualWeightKg        ? Number(raw.actualWeightKg)        : null,
      actualDurationMinutes: raw.actualDurationMinutes ? Number(raw.actualDurationMinutes) : null,
      actualDistanceMeters:  raw.actualDistanceMeters  ? Number(raw.actualDistanceMeters)  : null,
      notes:                 raw.notes.trim() || null,
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
