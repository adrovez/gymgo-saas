import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MaintenanceService } from '../services/maintenance.service';
import { EquipmentService } from '../services/equipment.service';
import { DialogService } from '../../../core/services/dialog.service';
import {
  EquipmentSummaryDto,
  MaintenanceType,
  ResponsibleType,
  MAINTENANCE_TYPE_OPTIONS,
  RESPONSIBLE_TYPE_OPTIONS,
} from '../models/maintenance.models';

@Component({
  selector: 'app-maintenance-form',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './maintenance-form.html',
})
export class MaintenanceFormComponent implements OnInit {
  private readonly fb                 = inject(FormBuilder);
  private readonly router             = inject(Router);
  private readonly maintenanceService = inject(MaintenanceService);
  private readonly equipmentService   = inject(EquipmentService);
  private readonly dialog             = inject(DialogService);

  readonly loading           = signal(false);
  readonly error             = signal<string | null>(null);
  readonly equipment         = signal<EquipmentSummaryDto[]>([]);
  readonly typeOptions       = MAINTENANCE_TYPE_OPTIONS;
  readonly responsibleOptions = RESPONSIBLE_TYPE_OPTIONS;
  readonly ResponsibleType   = ResponsibleType;

  readonly form = this.fb.nonNullable.group({
    equipmentId:             ['', Validators.required],
    type:                    [MaintenanceType.Preventive, Validators.required],
    scheduledDate:           ['', Validators.required],
    description:             ['', [Validators.required, Validators.maxLength(500)]],
    responsibleType:         [ResponsibleType.Internal, Validators.required],
    responsibleUserId:       [''],
    externalProviderName:    ['', Validators.maxLength(200)],
    externalProviderContact: ['', Validators.maxLength(200)],
  });

  get isInternal(): boolean {
    return Number(this.form.get('responsibleType')?.value) === ResponsibleType.Internal;
  }

  ngOnInit(): void {
    this.equipmentService.getEquipment(true).subscribe({
      next: (items) => this.equipment.set(items),
    });
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
    const isInternal = Number(raw.responsibleType) === ResponsibleType.Internal;

    this.loading.set(true);
    this.error.set(null);

    this.maintenanceService.create({
      equipmentId:             raw.equipmentId,
      type:                    Number(raw.type) as MaintenanceType,
      scheduledDate:           raw.scheduledDate,
      description:             raw.description.trim(),
      responsibleType:         Number(raw.responsibleType) as ResponsibleType,
      responsibleUserId:       isInternal ? (raw.responsibleUserId || null) : null,
      externalProviderName:    !isInternal ? (raw.externalProviderName.trim() || null) : null,
      externalProviderContact: !isInternal ? (raw.externalProviderContact.trim() || null) : null,
    }).subscribe({
      next: async (res) => {
        this.loading.set(false);
        await this.dialog.success('¡Mantención registrada!', 'El registro fue creado en estado Pendiente.');
        this.router.navigate(['/app/maintenance', res.id]);
      },
      error: () => {
        this.error.set('Ocurrió un error al guardar. Verifica los datos e intenta nuevamente.');
        this.loading.set(false);
      },
    });
  }
}
