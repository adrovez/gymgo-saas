import { Component, OnInit, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { EquipmentSummaryDto } from '../models/maintenance.models';
import { EquipmentService } from '../services/equipment.service';
import { DialogService } from '../../../core/services/dialog.service';

@Component({
  selector: 'app-equipment-list',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './equipment-list.html',
})
export class EquipmentListComponent implements OnInit {
  private readonly equipmentService = inject(EquipmentService);
  private readonly dialog           = inject(DialogService);
  private readonly router           = inject(Router);

  readonly loading   = signal(false);
  readonly error     = signal<string | null>(null);
  readonly equipment = signal<EquipmentSummaryDto[]>([]);

  private activeFilter: boolean | null = null;

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);

    this.equipmentService.getEquipment(this.activeFilter).subscribe({
      next: (items) => {
        this.equipment.set(items);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('No se pudo cargar la maquinaria. Intenta nuevamente.');
        this.loading.set(false);
      },
    });
  }

  onFilterChange(value: string): void {
    this.activeFilter = value === '' ? null : value === 'true';
    this.load();
  }

  edit(item: EquipmentSummaryDto): void {
    this.router.navigate(['/app/equipment', item.id, 'edit']);
  }

  viewMaintenance(item: EquipmentSummaryDto): void {
    this.router.navigate(['/app/maintenance'], {
      queryParams: { equipmentId: item.id },
    });
  }

  async toggleActive(item: EquipmentSummaryDto): Promise<void> {
    const action   = item.isActive ? 'desactivar' : 'reactivar';
    const actionPp = item.isActive ? 'desactivada' : 'reactivada';

    const result = await this.dialog.confirmDanger(
      `¿${item.isActive ? 'Desactivar' : 'Reactivar'} máquina?`,
      `La máquina "${item.name}" será ${actionPp}.`,
      `Sí, ${action}`,
    );
    if (!result.isConfirmed) return;

    const call$ = item.isActive
      ? this.equipmentService.deactivate(item.id)
      : this.equipmentService.reactivate(item.id);

    call$.subscribe({
      next: () => {
        this.dialog.toast(`"${item.name}" ${actionPp} correctamente`, 'success');
        this.load();
      },
      error: () => this.error.set(`No se pudo ${action} la máquina. Intenta nuevamente.`),
    });
  }
}
