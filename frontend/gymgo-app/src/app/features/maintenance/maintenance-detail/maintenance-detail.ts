import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { DecimalPipe, SlicePipe } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MaintenanceService } from '../services/maintenance.service';
import { DialogService } from '../../../core/services/dialog.service';
import { ReplacePipe } from '../../../shared/pipes/replace.pipe';
import {
  MaintenanceRecordDto,
  MaintenanceStatus,
  MaintenanceType,
  ResponsibleType,
} from '../models/maintenance.models';

@Component({
  selector: 'app-maintenance-detail',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, SlicePipe, DecimalPipe, ReplacePipe],
  templateUrl: './maintenance-detail.html',
})
export class MaintenanceDetailComponent implements OnInit {
  private readonly maintenanceService = inject(MaintenanceService);
  private readonly dialog             = inject(DialogService);
  private readonly route              = inject(ActivatedRoute);
  private readonly router             = inject(Router);
  private readonly fb                 = inject(FormBuilder);

  readonly loading        = signal(false);
  readonly actionLoading  = signal(false);
  readonly error          = signal<string | null>(null);
  readonly record         = signal<MaintenanceRecordDto | null>(null);
  readonly showCompleteForm = signal(false);

  readonly MaintenanceStatus = MaintenanceStatus;
  readonly MaintenanceType   = MaintenanceType;
  readonly ResponsibleType   = ResponsibleType;

  readonly completeForm = this.fb.nonNullable.group({
    notes: ['', Validators.maxLength(1000)],
    cost:  [null as number | null],
  });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.loadRecord(id);
  }

  private loadRecord(id: string): void {
    this.loading.set(true);
    this.maintenanceService.getById(id).subscribe({
      next: (rec) => {
        this.record.set(rec);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('No se pudo cargar el registro de mantención.');
        this.loading.set(false);
      },
    });
  }

  async startMaintenance(): Promise<void> {
    const rec = this.record();
    if (!rec) return;

    const result = await this.dialog.confirmDanger(
      '¿Iniciar mantención?',
      `La mantención de "${rec.equipmentName}" pasará al estado En Proceso.`,
      'Sí, iniciar',
    );
    if (!result.isConfirmed) return;

    this.actionLoading.set(true);
    this.maintenanceService.start(rec.id).subscribe({
      next: () => {
        this.dialog.toast('Mantención iniciada', 'success');
        this.loadRecord(rec.id);
        this.actionLoading.set(false);
      },
      error: () => {
        this.error.set('No se pudo iniciar la mantención.');
        this.actionLoading.set(false);
      },
    });
  }

  toggleCompleteForm(): void {
    this.showCompleteForm.update(v => !v);
    this.completeForm.reset();
  }

  async completeMaintenance(): Promise<void> {
    const rec = this.record();
    if (!rec || this.completeForm.invalid) return;

    this.actionLoading.set(true);
    const raw = this.completeForm.getRawValue();

    this.maintenanceService.complete(rec.id, {
      notes: raw.notes?.trim() || null,
      cost:  raw.cost ?? null,
    }).subscribe({
      next: () => {
        this.dialog.toast('Mantención completada', 'success');
        this.showCompleteForm.set(false);
        this.loadRecord(rec.id);
        this.actionLoading.set(false);
      },
      error: () => {
        this.error.set('No se pudo completar la mantención.');
        this.actionLoading.set(false);
      },
    });
  }

  async cancelMaintenance(): Promise<void> {
    const rec = this.record();
    if (!rec) return;

    const result = await this.dialog.confirmDanger(
      '¿Cancelar mantención?',
      `La mantención de "${rec.equipmentName}" será marcada como Cancelada.`,
      'Sí, cancelar',
    );
    if (!result.isConfirmed) return;

    this.actionLoading.set(true);
    this.maintenanceService.cancel(rec.id).subscribe({
      next: () => {
        this.dialog.toast('Mantención cancelada', 'success');
        this.loadRecord(rec.id);
        this.actionLoading.set(false);
      },
      error: () => {
        this.error.set('No se pudo cancelar la mantención.');
        this.actionLoading.set(false);
      },
    });
  }

  statusBadgeClass(status: MaintenanceStatus): string {
    switch (status) {
      case MaintenanceStatus.Pending:    return 'badge badge-pending';
      case MaintenanceStatus.InProgress: return 'badge badge-inprogress';
      case MaintenanceStatus.Completed:  return 'badge badge-active';
      case MaintenanceStatus.Cancelled:  return 'badge badge-suspended';
      default: return 'badge';
    }
  }
}
