import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { SlicePipe } from '@angular/common';
import { WorkoutPlanService } from '../services/workout-plan.service';
import { DialogService } from '../../../core/services/dialog.service';
import {
  WorkoutPlanDto,
  WorkoutPlanDayDto,
  WorkoutPlanExerciseDto,
  WorkoutPlanStatus,
  WorkoutDayOfWeek,
  MuscleGroup,
  DAY_OF_WEEK_OPTIONS,
  MUSCLE_GROUP_OPTIONS,
  muscleGroupBadgeClass,
} from '../models/workout-plan.models';

@Component({
  selector: 'app-workout-plan-detail',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, SlicePipe],
  templateUrl: './workout-plan-detail.html',
})
export class WorkoutPlanDetailComponent implements OnInit {
  private readonly fb          = inject(FormBuilder);
  private readonly route       = inject(ActivatedRoute);
  private readonly router      = inject(Router);
  private readonly planService = inject(WorkoutPlanService);
  private readonly dialog      = inject(DialogService);

  readonly loading              = signal(false);
  readonly saving               = signal(false);
  readonly error                = signal<string | null>(null);
  readonly plan                 = signal<WorkoutPlanDto | null>(null);
  readonly expandedDayId        = signal<string | null>(null);
  readonly showAddDayForm       = signal(false);
  readonly addingExerciseDayId  = signal<string | null>(null);
  readonly editingExerciseId    = signal<string | null>(null);
  readonly savingDay            = signal(false);
  readonly savingExercise       = signal(false);
  readonly dayError             = signal<string | null>(null);
  readonly exerciseError        = signal<string | null>(null);

  readonly WorkoutPlanStatus   = WorkoutPlanStatus;
  readonly dayOptions          = DAY_OF_WEEK_OPTIONS;
  readonly muscleGroupOptions  = MUSCLE_GROUP_OPTIONS;
  readonly muscleGroupBadgeClass = muscleGroupBadgeClass;

  readonly addDayForm = this.fb.nonNullable.group({
    dayOfWeek: [WorkoutDayOfWeek.Monday as WorkoutDayOfWeek],
    notes:     [''],
  });

  readonly addExerciseForm = this.fb.nonNullable.group({
    exerciseName:           ['', [Validators.required, Validators.maxLength(200)]],
    muscleGroup:            [MuscleGroup.NotSpecified as MuscleGroup],
    plannedSets:            [null as number | null],
    plannedReps:            [null as number | null],
    plannedWeightKg:        [null as number | null],
    plannedDurationMinutes: [null as number | null],
    plannedDistanceMeters:  [null as number | null],
  });

  private planId!: string;

  ngOnInit(): void {
    this.planId = this.route.snapshot.paramMap.get('id')!;
    this.loadPlan();
  }

  loadPlan(): void {
    this.loading.set(true);
    this.error.set(null);
    this.planService.getById(this.planId).subscribe({
      next:  (data) => { this.plan.set(data); this.loading.set(false); },
      error: ()     => { this.error.set('No se pudo cargar la rutina.'); this.loading.set(false); },
    });
  }

  get isActive(): boolean {
    return this.plan()?.status === WorkoutPlanStatus.Active;
  }

  // ── Días accordion ────────────────────────────────────────────────────────────
  toggleDay(dayId: string): void {
    this.expandedDayId.set(this.expandedDayId() === dayId ? null : dayId);
    this.addingExerciseDayId.set(null);
    this.editingExerciseId.set(null);
    this.exerciseError.set(null);
  }

  // ── Agregar día ───────────────────────────────────────────────────────────────
  toggleAddDayForm(): void {
    this.showAddDayForm.update(v => !v);
    this.dayError.set(null);
    if (!this.showAddDayForm()) {
      this.addDayForm.reset({ dayOfWeek: WorkoutDayOfWeek.Monday });
    }
  }

  onAddDay(): void {
    if (this.addDayForm.invalid || this.savingDay()) {
      this.addDayForm.markAllAsTouched();
      return;
    }
    const raw = this.addDayForm.getRawValue();
    this.savingDay.set(true);
    this.dayError.set(null);

    this.planService.addDay(this.planId, {
      dayOfWeek: Number(raw.dayOfWeek) as WorkoutDayOfWeek,
      notes:     raw.notes.trim() || null,
    }).subscribe({
      next: () => {
        this.savingDay.set(false);
        this.showAddDayForm.set(false);
        this.addDayForm.reset({ dayOfWeek: WorkoutDayOfWeek.Monday });
        this.loadPlan();
      },
      error: (err) => {
        const detail = err?.error?.detail ?? err?.error?.title;
        this.dayError.set(detail ?? 'No se pudo agregar el día.');
        this.savingDay.set(false);
      },
    });
  }

  // ── Eliminar día ──────────────────────────────────────────────────────────────
  async onRemoveDay(day: WorkoutPlanDayDto): Promise<void> {
    const confirm = await this.dialog.confirmDanger(
      'Eliminar día',
      `¿Eliminar "${day.dayOfWeekName}" y todos sus ejercicios de la rutina?`,
      'Eliminar',
    );
    if (!confirm.isConfirmed) return;

    this.planService.removeDay(this.planId, day.id).subscribe({
      next:  () => { this.dialog.toast('Día eliminado.', 'success'); this.loadPlan(); },
      error: () => this.dialog.toast('No se pudo eliminar el día.', 'error'),
    });
  }

  // ── Agregar ejercicio ─────────────────────────────────────────────────────────
  toggleAddExerciseForm(dayId: string): void {
    if (this.addingExerciseDayId() === dayId) {
      this.addingExerciseDayId.set(null);
    } else {
      this.addingExerciseDayId.set(dayId);
      this.editingExerciseId.set(null);
      this.addExerciseForm.reset({ muscleGroup: MuscleGroup.NotSpecified });
    }
    this.exerciseError.set(null);
  }

  getNextSortOrder(day: WorkoutPlanDayDto): number {
    return day.exercises.length > 0
      ? Math.max(...day.exercises.map(e => e.sortOrder)) + 1
      : 1;
  }

  onAddExercise(day: WorkoutPlanDayDto): void {
    if (this.addExerciseForm.invalid || this.savingExercise()) {
      this.addExerciseForm.markAllAsTouched();
      return;
    }
    const raw = this.addExerciseForm.getRawValue();
    this.savingExercise.set(true);
    this.exerciseError.set(null);

    this.planService.addExercise(day.id, {
      exerciseName:           raw.exerciseName.trim(),
      muscleGroup:            Number(raw.muscleGroup) as MuscleGroup,
      sortOrder:              this.getNextSortOrder(day),
      plannedSets:            raw.plannedSets            ? Number(raw.plannedSets)            : null,
      plannedReps:            raw.plannedReps            ? Number(raw.plannedReps)            : null,
      plannedWeightKg:        raw.plannedWeightKg        ? Number(raw.plannedWeightKg)        : null,
      plannedDurationMinutes: raw.plannedDurationMinutes ? Number(raw.plannedDurationMinutes) : null,
      plannedDistanceMeters:  raw.plannedDistanceMeters  ? Number(raw.plannedDistanceMeters)  : null,
    }).subscribe({
      next: () => {
        this.savingExercise.set(false);
        this.addingExerciseDayId.set(null);
        this.addExerciseForm.reset({ muscleGroup: MuscleGroup.NotSpecified });
        this.loadPlan();
      },
      error: (err) => {
        const detail = err?.error?.detail ?? err?.error?.title;
        this.exerciseError.set(detail ?? 'No se pudo agregar el ejercicio.');
        this.savingExercise.set(false);
      },
    });
  }

  // ── Eliminar ejercicio ────────────────────────────────────────────────────────
  async onRemoveExercise(day: WorkoutPlanDayDto, exercise: WorkoutPlanExerciseDto): Promise<void> {
    const confirm = await this.dialog.confirmDanger(
      'Eliminar ejercicio',
      `¿Eliminar "${exercise.exerciseName}" del plan?`,
      'Eliminar',
    );
    if (!confirm.isConfirmed) return;

    this.planService.removeExercise(day.id, exercise.id).subscribe({
      next:  () => { this.dialog.toast('Ejercicio eliminado.', 'success'); this.loadPlan(); },
      error: () => this.dialog.toast('No se pudo eliminar el ejercicio.', 'error'),
    });
  }

  // ── Registrar sesión ──────────────────────────────────────────────────────────
  registerSession(day: WorkoutPlanDayDto): void {
    this.router.navigate(['/app/workout-logs/new'], {
      queryParams: { planId: this.planId, dayId: day.id },
    });
  }

  // ── Completar/Cancelar/Eliminar plan ─────────────────────────────────────────
  async onComplete(): Promise<void> {
    const confirm = await this.dialog.confirmDanger(
      'Completar rutina',
      '¿Marcar esta rutina como completada? No podrá registrar más sesiones.',
      'Completar',
    );
    if (!confirm.isConfirmed) return;
    // PATCH endpoint no existe aún — se puede agregar más adelante
    this.dialog.toast('Funcionalidad próximamente disponible.', 'info' as any);
  }

  async onDelete(): Promise<void> {
    const confirm = await this.dialog.confirmDanger(
      'Eliminar rutina',
      'Se eliminará la rutina y todos sus días y ejercicios. ¿Deseas continuar?',
      'Eliminar',
    );
    if (!confirm.isConfirmed) return;

    this.saving.set(true);
    this.planService.delete(this.planId).subscribe({
      next: async () => {
        this.saving.set(false);
        await this.dialog.success('Rutina eliminada', '');
        this.router.navigate(['/app/workout-plans']);
      },
      error: () => {
        this.dialog.toast('No se pudo eliminar la rutina.', 'error');
        this.saving.set(false);
      },
    });
  }

  // ── Helpers ───────────────────────────────────────────────────────────────────
  formatDate(dateStr: string): string {
    if (!dateStr) return '-';
    const [y, m, d] = dateStr.split('-');
    return `${d}/${m}/${y}`;
  }

  planMetrics(day: WorkoutPlanDayDto): string {
    const ex = day.exercises;
    if (ex.length === 0) return 'Sin ejercicios';
    return `${ex.length} ejercicio${ex.length !== 1 ? 's' : ''}`;
  }

  plannedChips(exercise: WorkoutPlanExerciseDto): string[] {
    const chips: string[] = [];
    if (exercise.plannedSets && exercise.plannedReps) {
      chips.push(`${exercise.plannedSets} × ${exercise.plannedReps}`);
    } else if (exercise.plannedSets) {
      chips.push(`${exercise.plannedSets} series`);
    }
    if (exercise.plannedWeightKg)        chips.push(`${exercise.plannedWeightKg} kg`);
    if (exercise.plannedDurationMinutes) chips.push(`${exercise.plannedDurationMinutes} min`);
    if (exercise.plannedDistanceMeters)  chips.push(`${exercise.plannedDistanceMeters} m`);
    return chips;
  }

  isAddExInvalid(field: string): boolean {
    const ctrl = this.addExerciseForm.get(field);
    return !!(ctrl?.invalid && ctrl.touched);
  }
}
