import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MemberService } from '../../members/services/member.service';
import { MemberSummaryDto } from '../../members/models/member.models';
import { WorkoutPlanService } from '../services/workout-plan.service';
import { DialogService } from '../../../core/services/dialog.service';

@Component({
  selector: 'app-workout-plan-create',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './workout-plan-create.html',
})
export class WorkoutPlanCreateComponent implements OnInit {
  private readonly fb          = inject(FormBuilder);
  private readonly router      = inject(Router);
  private readonly route       = inject(ActivatedRoute);
  private readonly planService = inject(WorkoutPlanService);
  private readonly memberService = inject(MemberService);
  private readonly dialog      = inject(DialogService);

  readonly loading        = signal(false);
  readonly loadingMembers = signal(false);
  readonly error          = signal<string | null>(null);
  readonly members        = signal<MemberSummaryDto[]>([]);
  readonly showMedidas    = signal(false);

  readonly form = this.fb.nonNullable.group({
    memberId:                 ['', Validators.required],
    objective:                ['', [Validators.required, Validators.maxLength(200)]],
    startDate:                [this.todayString(), Validators.required],
    endDate:                  ['', Validators.required],
    notes:                    [''],
    initialWeightKg:          [null as number | null],
    initialHeightCm:          [null as number | null],
    initialBodyFatPercentage: [null as number | null],
  });

  ngOnInit(): void {
    this.loadMembers();
    const memberId = this.route.snapshot.queryParamMap.get('memberId');
    if (memberId) {
      this.form.patchValue({ memberId });
    }
  }

  private loadMembers(): void {
    this.loadingMembers.set(true);
    this.memberService.getMembers({ page: 1, pageSize: 200 }).subscribe({
      next: (result) => {
        this.members.set(result.items);
        this.loadingMembers.set(false);
      },
      error: () => this.loadingMembers.set(false),
    });
  }

  private todayString(): string {
    return new Date().toISOString().split('T')[0];
  }

  isInvalid(field: string): boolean {
    const ctrl = this.form.get(field);
    return !!(ctrl?.invalid && ctrl.touched);
  }

  hasError(field: string, error: string): boolean {
    return !!(this.form.get(field)?.hasError(error) && this.form.get(field)?.touched);
  }

  async onSubmit(): Promise<void> {
    if (this.form.invalid || this.loading()) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    this.loading.set(true);
    this.error.set(null);

    this.planService.create({
      memberId:                 raw.memberId,
      objective:                raw.objective.trim(),
      startDate:                raw.startDate,
      endDate:                  raw.endDate,
      notes:                    raw.notes.trim() || null,
      initialWeightKg:          raw.initialWeightKg          ? Number(raw.initialWeightKg)          : null,
      initialHeightCm:          raw.initialHeightCm          ? Number(raw.initialHeightCm)          : null,
      initialBodyFatPercentage: raw.initialBodyFatPercentage ? Number(raw.initialBodyFatPercentage) : null,
    }).subscribe({
      next: async (res) => {
        this.loading.set(false);
        this.dialog.toast('Rutina creada. Ahora agrega los días de entrenamiento.', 'success');
        this.router.navigate(['/app/workout-plans', res.id]);
      },
      error: (err) => {
        const detail = err?.error?.detail ?? err?.error?.title;
        this.error.set(detail ?? 'No se pudo crear la rutina. Verifica los datos e intenta nuevamente.');
        this.loading.set(false);
      },
    });
  }
}
