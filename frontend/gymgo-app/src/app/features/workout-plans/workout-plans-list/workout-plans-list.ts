import { Component, OnInit, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { MemberService } from '../../members/services/member.service';
import { MemberSummaryDto } from '../../members/models/member.models';
import { WorkoutPlanService } from '../services/workout-plan.service';
import {
  WorkoutPlanSummaryDto,
  WorkoutPlanStatus,
  WORKOUT_PLAN_STATUS_LABELS,
} from '../models/workout-plan.models';

@Component({
  selector: 'app-workout-plans-list',
  standalone: true,
  imports: [],
  templateUrl: './workout-plans-list.html',
})
export class WorkoutPlansListComponent implements OnInit {
  private readonly planService   = inject(WorkoutPlanService);
  private readonly memberService = inject(MemberService);
  private readonly router        = inject(Router);

  readonly loading        = signal(false);
  readonly loadingMembers = signal(false);
  readonly error          = signal<string | null>(null);
  readonly plans          = signal<WorkoutPlanSummaryDto[]>([]);
  readonly members        = signal<MemberSummaryDto[]>([]);

  readonly WorkoutPlanStatus = WorkoutPlanStatus;
  readonly statusLabels      = WORKOUT_PLAN_STATUS_LABELS;

  selectedMemberId: string = '';

  ngOnInit(): void {
    this.loadMembers();
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

  onMemberChange(value: string): void {
    this.selectedMemberId = value;
    if (value) this.loadPlans();
    else        this.plans.set([]);
  }

  loadPlans(): void {
    if (!this.selectedMemberId) return;
    this.loading.set(true);
    this.error.set(null);
    this.planService.getPlans({ memberId: this.selectedMemberId }).subscribe({
      next: (items) => {
        // Ordenar: activas primero, luego por fecha de creación desc
        items.sort((a, b) => {
          if (a.status === WorkoutPlanStatus.Active && b.status !== WorkoutPlanStatus.Active) return -1;
          if (a.status !== WorkoutPlanStatus.Active && b.status === WorkoutPlanStatus.Active) return 1;
          return b.createdAtUtc.localeCompare(a.createdAtUtc);
        });
        this.plans.set(items);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('No se pudo cargar las rutinas.');
        this.loading.set(false);
      },
    });
  }

  viewPlan(plan: WorkoutPlanSummaryDto): void {
    this.router.navigate(['/app/workout-plans', plan.id]);
  }

  newPlan(): void {
    this.router.navigate(['/app/workout-plans/new'], {
      queryParams: this.selectedMemberId ? { memberId: this.selectedMemberId } : {},
    });
  }

  statusBadgeClass(status: WorkoutPlanStatus): string {
    switch (status) {
      case WorkoutPlanStatus.Active:    return 'badge badge-active';
      case WorkoutPlanStatus.Completed: return 'badge badge-inactive';
      default:                          return 'badge badge-pending';
    }
  }

  formatDate(dateStr: string): string {
    if (!dateStr) return '-';
    const [y, m, d] = dateStr.split('-');
    return `${d}/${m}/${y}`;
  }

  get selectedMemberName(): string {
    const m = this.members().find(x => x.id === this.selectedMemberId);
    return m ? m.fullName : '';
  }
}
