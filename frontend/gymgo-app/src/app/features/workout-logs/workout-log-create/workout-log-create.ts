import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { WorkoutPlanService } from '../../workout-plans/services/workout-plan.service';
import { WorkoutPlanDto, WorkoutPlanDayDto, muscleGroupBadgeClass } from '../../workout-plans/models/workout-plan.models';
import { WorkoutLogService } from '../services/workout-log.service';
import { DialogService } from '../../../core/services/dialog.service';

@Component({
  selector: 'app-workout-log-create',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './workout-log-create.html',
})
export class WorkoutLogCreateComponent implements OnInit {
  private readonly fb              = inject(FormBuilder);
  private readonly router          = inject(Router);
  private readonly route           = inject(ActivatedRoute);
  private readonly planService     = inject(WorkoutPlanService);
  private readonly workoutLogService = inject(WorkoutLogService);
  private readonly dialog          = inject(DialogService);

  readonly loading     = signal(false);
  readonly loadingPlan = signal(false);
  readonly error       = signal<string | null>(null);
  readonly plan        = signal<WorkoutPlanDto | null>(null);

  readonly muscleGroupBadgeClass = muscleGroupBadgeClass;

  readonly day = computed<WorkoutPlanDayDto | null>(() => {
    const p = this.plan();
    if (!p) return null;
    return p.days.find(d => d.id === this.dayId) ?? null;
  });

  readonly form = this.fb.nonNullable.group({
    date:  [this.todayString(), Validators.required],
    notes: [''],
  });

  private planId!: string;
  private dayId!: string;

  ngOnInit(): void {
    this.planId = this.route.snapshot.queryParamMap.get('planId') ?? '';
    this.dayId  = this.route.snapshot.queryParamMap.get('dayId')  ?? '';

    if (!this.planId || !this.dayId) {
      this.router.navigate(['/app/workout-plans']);
      return;
    }

    this.loadPlan();
  }

  private loadPlan(): void {
    this.loadingPlan.set(true);
    this.planService.getById(this.planId).subscribe({
      next:  (data) => { this.plan.set(data); this.loadingPlan.set(false); },
      error: ()     => {
        this.error.set('No se pudo cargar la rutina.');
        this.loadingPlan.set(false);
      },
    });
  }

  private todayString(): string {
    return new Date().toISOString().split('T')[0];
  }

  isInvalid(field: string): boolean {
    const ctrl = this.form.get(field);
    return !!(ctrl?.invalid && ctrl.touched);
  }

  async onSubmit(): Promise<void> {
    if (this.form.invalid || this.loading()) {
      this.form.markAllAsTouched();
      return;
    }

    const p = this.plan();
    if (!p) return;

    const raw = this.form.getRawValue();
    this.loading.set(true);
    this.error.set(null);

    this.workoutLogService.create({
      memberId:         p.memberId,
      workoutPlanId:    this.planId,
      workoutPlanDayId: this.dayId,
      date:             raw.date || null,
      notes:            raw.notes.trim() || null,
    }).subscribe({
      next: async (res) => {
        this.loading.set(false);
        this.dialog.toast('Sesión iniciada. Registra los ejercicios realizados.', 'success');
        this.router.navigate(['/app/workout-logs', res.id]);
      },
      error: (err) => {
        const detail = err?.error?.detail ?? err?.error?.title;
        this.error.set(detail ?? 'No se pudo crear la sesión. Verifica los datos e intenta nuevamente.');
        this.loading.set(false);
      },
    });
  }
}
