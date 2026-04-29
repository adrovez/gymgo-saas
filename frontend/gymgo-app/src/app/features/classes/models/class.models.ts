// ─────────────────────────────────────────────────────────────────────────────
//  GymGo — Models: GymClasses / ClassSchedules
// ─────────────────────────────────────────────────────────────────────────────

export enum ClassCategory {
  Other       = 0,
  Cardio      = 1,
  Strength    = 2,
  Flexibility = 3,
  Martial     = 4,
  Dance       = 5,
  Aquatic     = 6,
  Mind        = 7,
}

export const CLASS_CATEGORY_LABELS: Record<ClassCategory, string> = {
  [ClassCategory.Other]:       'Otro',
  [ClassCategory.Cardio]:      'Cardio',
  [ClassCategory.Strength]:    'Fuerza',
  [ClassCategory.Flexibility]: 'Flexibilidad',
  [ClassCategory.Martial]:     'Artes marciales',
  [ClassCategory.Dance]:       'Baile',
  [ClassCategory.Aquatic]:     'Acuático',
  [ClassCategory.Mind]:        'Mente y cuerpo',
};

export const CLASS_CATEGORY_OPTIONS = Object.values(ClassCategory)
  .filter((v): v is ClassCategory => typeof v === 'number')
  .map(v => ({ value: v, label: CLASS_CATEGORY_LABELS[v] }));

export const DAY_OF_WEEK_LABELS: Record<number, string> = {
  0: 'Domingo',
  1: 'Lunes',
  2: 'Martes',
  3: 'Miércoles',
  4: 'Jueves',
  5: 'Viernes',
  6: 'Sábado',
};

// Lunes a Domingo ordenado para la vista de calendario
export const CALENDAR_DAYS = [1, 2, 3, 4, 5, 6, 0];

// ─────────────────────────────────────────────────────────────────────────────
//  DTOs (espejo del backend)
// ─────────────────────────────────────────────────────────────────────────────

export interface GymClassDto {
  id: string;
  tenantId: string;
  name: string;
  description: string | null;
  category: ClassCategory;
  categoryLabel: string;
  color: string | null;
  durationMinutes: number;
  maxCapacity: number;
  isActive: boolean;
  createdAtUtc: string;
  createdBy: string | null;
  modifiedAtUtc: string | null;
  modifiedBy: string | null;
  schedules: ClassScheduleDto[];
}

export interface GymClassSummaryDto {
  id: string;
  name: string;
  description: string | null;
  category: ClassCategory;
  categoryLabel: string;
  color: string | null;
  durationMinutes: number;
  maxCapacity: number;
  isActive: boolean;
  scheduleCount: number;
}

export interface ClassScheduleDto {
  id: string;
  gymClassId: string;
  gymClassName: string;
  gymClassColor: string | null;
  dayOfWeek: number;
  dayLabel: string;
  startTime: string;   // "HH:mm"
  endTime: string;     // "HH:mm"
  instructorName: string | null;
  room: string | null;
  maxCapacity: number | null;
  isActive: boolean;
}

// ─────────────────────────────────────────────────────────────────────────────
//  Requests
// ─────────────────────────────────────────────────────────────────────────────

export interface CreateGymClassRequest {
  name: string;
  description: string | null;
  category: ClassCategory;
  color: string | null;
  durationMinutes: number;
  maxCapacity: number;
}

export interface CreateClassScheduleRequest {
  dayOfWeek: number;
  startTime: string;
  endTime: string;
  instructorName: string | null;
  room: string | null;
  maxCapacity: number | null;
}

// ─────────────────────────────────────────────────────────────────────────────
//  Colores predefinidos para el color picker
// ─────────────────────────────────────────────────────────────────────────────
export const PRESET_COLORS = [
  { value: '#3B82F6', label: 'Azul' },
  { value: '#10B981', label: 'Verde' },
  { value: '#F59E0B', label: 'Amarillo' },
  { value: '#EF4444', label: 'Rojo' },
  { value: '#8B5CF6', label: 'Violeta' },
  { value: '#EC4899', label: 'Rosa' },
  { value: '#F97316', label: 'Naranja' },
  { value: '#06B6D4', label: 'Celeste' },
  { value: '#64748B', label: 'Gris' },
];
