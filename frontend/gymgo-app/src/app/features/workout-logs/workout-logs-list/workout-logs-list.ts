import { Component, OnInit, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { MemberService } from '../../members/services/member.service';
import { MemberSummaryDto } from '../../members/models/member.models';
import { WorkoutLogService } from '../services/workout-log.service';
import {
  WorkoutLogSummaryDto,
  WorkoutLogStatus,
  WORKOUT_STATUS_LABELS,
} from '../models/workout-log.models';

@Component({
  selector: 'app-workout-logs-list',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './workout-logs-list.html',
})
export class WorkoutLogsListComponent implements OnInit {
  private readonly workoutLogService = inject(WorkoutLogService);
  private readonly memberService     = inject(MemberService);
  private readonly router            = inject(Router);

  readonly loading        = signal(false);
  readonly loadingMembers = signal(false);
  readonly error          = signal<string | null>(null);
  readonly logs           = signal<WorkoutLogSummaryDto[]>([]);
  readonly members        = signal<MemberSummaryDto[]>([]);

  readonly WorkoutLogStatus  = WorkoutLogStatus;
  readonly statusLabels      = WORKOUT_STATUS_LABELS;

  selectedMemberId: string = '';
  fromDate:         string = '';
  toDate:           string = '';

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
      error: () => {
        this.loadingMembers.set(false);
      },
    });
  }

  onMemberChange(value: string): void {
    this.selectedMemberId = value;
    if (value) this.loadLogs();
    else        this.logs.set([]);
  }

  onFromDateChange(value: string): void {
    this.fromDate = value;
    if (this.selectedMemberId) this.loadLogs();
  }

  onToDateChange(value: string): void {
    this.toDate = value;
    if (this.selectedMemberId) this.loadLogs();
  }

  loadLogs(): void {
    if (!this.selectedMemberId) return;
    this.loading.set(true);
    this.error.set(null);

    this.workoutLogService.getLogs({
      memberId: this.selectedMemberId,
      from:     this.fromDate || null,
      to:       this.toDate   || null,
    }).subscribe({
      next: (items) => {
        this.logs.set(items);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('No se pudo cargar el historial de rutinas.');
        this.loading.set(false);
      },
    });
  }

  viewDetail(log: WorkoutLogSummaryDto): void {
    this.router.navigate(['/app/workout-logs', log.id]);
  }

  newLog(): void {
    const url = this.selectedMemberId
      ? ['/app/workout-logs/new']
      : ['/app/workout-logs/new'];
    this.router.navigate(url, {
      queryParams: this.selectedMemberId ? { memberId: this.selectedMemberId } : {},
    });
  }

  statusBadgeClass(status: WorkoutLogStatus): string {
    return status === WorkoutLogStatus.Completed
      ? 'badge badge-active'
      : 'badge badge-pending';
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
