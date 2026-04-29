import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  MembershipPlanSummaryDto,
  Periodicity,
  PERIODICITY_OPTIONS,
  PERIODICITY_LABELS,
} from '../models/membership-plan.models';
import {
  MembershipPlanService,
  GetMembershipPlansParams,
} from '../services/membership-plan.service';
import { DialogService } from '../../../core/services/dialog.service';

@Component({
  selector: 'app-membership-plans-list',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './membership-plans-list.html',
})
export class MembershipPlansListComponent implements OnInit {
  private readonly planService = inject(MembershipPlanService);
  private readonly dialog      = inject(DialogService);

  readonly loading           = signal(false);
  readonly error             = signal<string | null>(null);
  readonly plans             = signal<MembershipPlanSummaryDto[]>([]);

  readonly Periodicity        = Periodicity;
  readonly periodicityOptions = PERIODICITY_OPTIONS;
  readonly periodicityLabels  = PERIODICITY_LABELS;

  private searchValue:    string           = '';
  private periodicityFilter: Periodicity | null = null;
  private isActiveFilter: boolean | null   = null;

  private readonly searchSubject = new Subject<string>();

  constructor() {
    this.searchSubject
      .pipe(debounceTime(350), distinctUntilChanged(), takeUntilDestroyed())
      .subscribe((value) => {
        this.searchValue = value;
        this.loadPlans();
      });
  }

  ngOnInit(): void {
    this.loadPlans();
  }

  loadPlans(): void {
    this.loading.set(true);
    this.error.set(null);

    const params: GetMembershipPlansParams = {
      search:      this.searchValue,
      periodicity: this.periodicityFilter,
      isActive:    this.isActiveFilter,
    };

    this.planService.getMembershipPlans(params).subscribe({
      next: (result) => {
        this.plans.set(result);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('No se pudo cargar la lista de planes. Intenta nuevamente.');
        this.loading.set(false);
      },
    });
  }

  onSearch(value: string): void {
    this.searchSubject.next(value);
  }

  onPeriodicityFilterChange(value: string): void {
    this.periodicityFilter = value === '' ? null : (Number(value) as Periodicity);
    this.loadPlans();
  }

  onActiveFilterChange(value: string): void {
    if (value === '')       this.isActiveFilter = null;
    else if (value === '1') this.isActiveFilter = true;
    else                    this.isActiveFilter = false;
    this.loadPlans();
  }

  formatAmount(amount: number): string {
    return new Intl.NumberFormat('es-CL', { style: 'currency', currency: 'CLP' }).format(amount);
  }

  async toggleActive(plan: MembershipPlanSummaryDto): Promise<void> {
    if (plan.isActive) {
      const result = await this.dialog.confirmDanger(
        '¿Desactivar plan?',
        `El plan "${plan.name}" dejará de estar disponible para nuevas asignaciones. Los socios actuales no se ven afectados.`,
        'Sí, desactivar',
      );
      if (!result.isConfirmed) return;

      this.planService.deactivateMembershipPlan(plan.id).subscribe({
        next: () => {
          this.dialog.toast(`Plan "${plan.name}" desactivado`, 'success');
          this.loadPlans();
        },
        error: () => this.error.set('No se pudo desactivar el plan. Intenta nuevamente.'),
      });
    } else {
      const result = await this.dialog.confirmAction(
        '¿Reactivar plan?',
        `El plan "${plan.name}" volverá a estar disponible para nuevas asignaciones.`,
        'Sí, reactivar',
      );
      if (!result.isConfirmed) return;

      this.planService.reactivateMembershipPlan(plan.id).subscribe({
        next: () => {
          this.dialog.toast(`Plan "${plan.name}" reactivado`, 'success');
          this.loadPlans();
        },
        error: () => this.error.set('No se pudo reactivar el plan. Intenta nuevamente.'),
      });
    }
  }
}
