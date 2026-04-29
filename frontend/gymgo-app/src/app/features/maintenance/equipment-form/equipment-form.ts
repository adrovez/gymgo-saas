import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Observable } from 'rxjs';
import { EquipmentService } from '../services/equipment.service';
import { DialogService } from '../../../core/services/dialog.service';

@Component({
  selector: 'app-equipment-form',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './equipment-form.html',
})
export class EquipmentFormComponent implements OnInit {
  private readonly fb               = inject(FormBuilder);
  private readonly route            = inject(ActivatedRoute);
  private readonly router           = inject(Router);
  private readonly equipmentService = inject(EquipmentService);
  private readonly dialog           = inject(DialogService);

  readonly loading   = signal(false);
  readonly error     = signal<string | null>(null);
  readonly isEdit    = signal(false);
  private equipmentId: string | null = null;

  readonly form = this.fb.nonNullable.group({
    name:         ['', [Validators.required, Validators.maxLength(100)]],
    brand:        ['', Validators.maxLength(100)],
    model:        ['', Validators.maxLength(100)],
    serialNumber: ['', Validators.maxLength(50)],
    purchaseDate: [''],
    imageUrl:     ['', Validators.maxLength(500)],
  });

  ngOnInit(): void {
    this.equipmentId = this.route.snapshot.paramMap.get('id');
    if (this.equipmentId) {
      this.isEdit.set(true);
      this.loadEquipment(this.equipmentId);
    }
  }

  private loadEquipment(id: string): void {
    this.loading.set(true);
    this.equipmentService.getEquipmentById(id).subscribe({
      next: (equipment) => {
        this.form.patchValue({
          name:         equipment.name,
          brand:        equipment.brand ?? '',
          model:        equipment.model ?? '',
          serialNumber: equipment.serialNumber ?? '',
          purchaseDate: equipment.purchaseDate ?? '',
          imageUrl:     equipment.imageUrl ?? '',
        });
        this.loading.set(false);
      },
      error: () => {
        this.error.set('No se pudo cargar la máquina.');
        this.loading.set(false);
      },
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

    this.loading.set(true);
    this.error.set(null);

    const raw = this.form.getRawValue();
    const request = {
      name:         raw.name.trim(),
      brand:        raw.brand.trim()        || null,
      model:        raw.model.trim()        || null,
      serialNumber: raw.serialNumber.trim() || null,
      purchaseDate: raw.purchaseDate        || null,
      imageUrl:     raw.imageUrl.trim()     || null,
    };

    const call$: Observable<unknown> = this.isEdit()
      ? this.equipmentService.updateEquipment(this.equipmentId!, request)
      : this.equipmentService.createEquipment(request);

    call$.subscribe({
      next: async () => {
        this.loading.set(false);
        await this.dialog.success(
          this.isEdit() ? '¡Máquina actualizada!' : '¡Máquina registrada!',
          this.isEdit()
            ? `Los datos de "${raw.name}" fueron actualizados.`
            : `"${raw.name}" fue agregada al catálogo.`,
        );
        this.router.navigate(['/app/equipment']);
      },
      error: () => {
        this.error.set('Ocurrió un error al guardar. Intenta nuevamente.');
        this.loading.set(false);
      },
    });
  }
}
