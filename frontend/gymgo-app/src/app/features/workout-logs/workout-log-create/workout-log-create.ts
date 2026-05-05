import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MemberService } from '../../members/services/member.service';
import { MemberSummaryDto } from '../../members/models/member.models';
import { WorkoutLogService } from '../services/workout-log.service';
import { DialogService } from '../../../core/services/dialog.service';

@Component({
  selector: 'app-workout-log-create',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './workout-log-create.html',
})
export class WorkoutLogCreateComponent implements OnInit {
  private readonly fb                 = inject(FormBuilder);
  private readonly router             = inject(Router);
  private readonly route              = inject(ActivatedRoute);
  private readonly workoutLogService  = inject(WorkoutLogService);
  private readonly memberService      = inject(MemberService);
  private readonly dialog             = inject(DialogService);

  readonly loading        = signal(false);
  readonly loadingMembers = signal(false);
  readonly error          = signal<string | null>(null);
  readonly members        = signal<MemberSummaryDto[]>([]);

  readonly form = this.fb.nonNullable.group({
    memberId: ['', Validators.required],
    date:     [this.todayString(), Validators.required],
    title:    [''],
    notes:    [''],
  });

  ngOnInit(): void {
    this.loadMembers();
    // Pre-seleccionar socio si viene por query param
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

    this.workoutLogService.create({
      memberId: raw.memberId,
      date:     raw.date || null,
      title:    raw.title.trim() || null,
      notes:    raw.notes.trim() || null,
    }).subscribe({
      next: async (res) => {
        this.loading.set(false);
        this.dialog.toast('Sesión creada. Ahora agrega los ejercicios.', 'success');
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
