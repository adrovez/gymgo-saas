import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ClassService } from '../services/class.service';
import {
  ClassScheduleDto,
  DAY_OF_WEEK_LABELS,
  CALENDAR_DAYS,
} from '../models/class.models';

@Component({
  selector: 'app-class-calendar',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './class-calendar.html',
})
export class ClassCalendarComponent implements OnInit {
  private readonly classService = inject(ClassService);

  readonly loading   = signal(false);
  readonly error     = signal<string | null>(null);
  readonly schedules = signal<ClassScheduleDto[]>([]);

  readonly dayLabels    = DAY_OF_WEEK_LABELS;
  readonly CALENDAR_DAYS = CALENDAR_DAYS; // Lun–Dom

  /** Agrupa los horarios por día de la semana (0–6) */
  readonly schedulesByDay = computed<Map<number, ClassScheduleDto[]>>(() => {
    const map = new Map<number, ClassScheduleDto[]>();
    for (const day of CALENDAR_DAYS) map.set(day, []);

    for (const s of this.schedules()) {
      const list = map.get(s.dayOfWeek);
      if (list) list.push(s);
    }
    return map;
  });

  ngOnInit(): void {
    this.loadSchedule();
  }

  loadSchedule(): void {
    this.loading.set(true);
    this.error.set(null);

    this.classService.getWeeklySchedule().subscribe({
      next: (result) => {
        this.schedules.set(result);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('No se pudo cargar el calendario. Intenta nuevamente.');
        this.loading.set(false);
      },
    });
  }

  get totalSchedules(): number {
    return this.schedules().length;
  }

  /** Total de clases distintas con horarios */
  get uniqueClassCount(): number {
    return new Set(this.schedules().map(s => s.gymClassId)).size;
  }
}
