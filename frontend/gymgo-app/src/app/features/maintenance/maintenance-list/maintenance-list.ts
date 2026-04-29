import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import {
  MaintenanceRecordSummaryDto,
  MaintenanceType,
  MaintenanceStatus,
  MAINTENANCE_TYPE_OPTIONS,
  MAINTENANCE_STATUS_OPTIONS,
  MAINTENANCE_STATUS_LABELS,
} from '../models/maintenance.models';
import { MaintenanceService } from '../services/maintenance.service';
import { EquipmentService } from '../services/equipment.service';
import { EquipmentSummaryDto } from '../models/maintenance.models';

@Component({
  selector: 'app-maintenance-list',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './maintenance-list.html',
})
export class MaintenanceListComponent implements OnInit {
  private readonly maintenanceService = inject(MaintenanceService);
  private readonly equipmentService   = inject(EquipmentService);
  private readonly router             = inject(Router);
  private readonly route              = inject(ActivatedRoute);

  readonly loading   = signal(false);
  readonly error     = signal<string | null>(null);
  readonly records   = signal<MaintenanceRecordSummaryDto[]>([]);
  readonly equipment = signal<EquipmentSummaryDto[]>([]);

  readonly typeOptions   = MAINTENANCE_TYPE_OPTIONS;
  readonly statusOptions = MAINTENANCE_STATUS_OPTIONS;
  readonly statusLabels  = MAINTENANCE_STATUS_LABELS;
  readonly MaintenanceStatus = MaintenanceStatus;
  readonly MaintenanceType   = MaintenanceType;

  private equipmentFilter: string | null  = null;
  private typeFilter:      MaintenanceType | null   = null;
  private statusFilter:    MaintenanceStatus | null = null;

  ngOnInit(): void {
    // Soporte para ?equipmentId= desde la lista de maquinaria
    const qp = this.route.snapshot.queryParamMap.get('equipmentId');
    if (qp) this.equipmentFilter = qp;

    this.equipmentService.getEquipment().subscribe({ next: (items) => this.equipment.set(items) });
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);

    this.maintenanceService.getRecords({
      equipmentId: this.equipmentFilter,
      type:        this.typeFilter,
      status:      this.statusFilter,
    }).subscribe({
      next: (items) => {
        this.records.set(items);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('No se pudo cargar los registros de mantención.');
        this.loading.set(false);
      },
    });
  }

  onEquipmentFilter(value: string): void {
    this.equipmentFilter = value || null;
    this.load();
  }

  onTypeFilter(value: string): void {
    this.typeFilter = value === '' ? null : Number(value) as MaintenanceType;
    this.load();
  }

  onStatusFilter(value: string): void {
    this.statusFilter = value === '' ? null : Number(value) as MaintenanceStatus;
    this.load();
  }

  viewDetail(record: MaintenanceRecordSummaryDto): void {
    this.router.navigate(['/app/maintenance', record.id]);
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

  typeBadgeClass(type: MaintenanceType): string {
    return type === MaintenanceType.Preventive ? 'badge badge-preventive' : 'badge badge-corrective';
  }
}
