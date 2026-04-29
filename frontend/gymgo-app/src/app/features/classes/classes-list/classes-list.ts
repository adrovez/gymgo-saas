import { Component, OnInit, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { ClassService } from '../services/class.service';
import { GymClassSummaryDto, CLASS_CATEGORY_LABELS, ClassCategory } from '../models/class.models';
import { DialogService } from '../../../core/services/dialog.service';

@Component({
  selector: 'app-classes-list',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './classes-list.html',
})
export class ClassesListComponent implements OnInit {
  private readonly classService = inject(ClassService);
  private readonly dialog       = inject(DialogService);
  private readonly router       = inject(Router);

  readonly loading = signal(false);
  readonly error   = signal<string | null>(null);
  readonly classes = signal<GymClassSummaryDto[]>([]);

  readonly ClassCategory   = ClassCategory;
  readonly categoryLabels  = CLASS_CATEGORY_LABELS;

  private filterActive: boolean | null = null;

  ngOnInit(): void {
    this.loadClasses();
  }

  loadClasses(): void {
    this.loading.set(true);
    this.error.set(null);

    this.classService.getClasses(this.filterActive).subscribe({
      next: (result) => {
        this.classes.set(result);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('No se pudo cargar la lista de clases. Intenta nuevamente.');
        this.loading.set(false);
      },
    });
  }

  onFilterChange(value: string): void {
    this.filterActive = value === '' ? null : value === 'true';
    this.loadClasses();
  }

  editClass(c: GymClassSummaryDto): void {
    this.router.navigate(['/app/classes', c.id, 'edit']);
  }

  async toggleActive(c: GymClassSummaryDto): Promise<void> {
    const action  = c.isActive ? 'desactivar' : 'reactivar';
    const confirm = await this.dialog.confirmAction(
      `¿${c.isActive ? 'Desactivar' : 'Reactivar'} clase?`,
      `Se ${action}á la clase "${c.name}" y sus horarios dejarán de aparecer en el calendario.`,
      c.isActive ? 'Desactivar' : 'Reactivar',
    );
    if (!confirm.isConfirmed) return;

    const call = c.isActive
      ? this.classService.deactivateClass(c.id)
      : this.classService.reactivateClass(c.id);

    call.subscribe({
      next: () => {
        this.dialog.toast(
          `Clase "${c.name}" ${c.isActive ? 'desactivada' : 'reactivada'}.`,
          'success',
        );
        this.loadClasses();
      },
      error: () => this.error.set(`No se pudo ${action} la clase.`),
    });
  }
}
