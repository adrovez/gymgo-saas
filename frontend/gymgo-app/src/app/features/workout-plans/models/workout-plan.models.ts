// ─── Enums (mirror del dominio .NET) ─────────────────────────────────────────

export enum WorkoutPlanStatus {
  Active    = 0,
  Completed = 1,
  Cancelled = 2,
}

export enum WorkoutDayOfWeek {
  Monday    = 1,
  Tuesday   = 2,
  Wednesday = 3,
  Thursday  = 4,
  Friday    = 5,
  Saturday  = 6,
  Sunday    = 7,
}

export enum MuscleGroup {
  NotSpecified = 0,
  Chest        = 1,
  Back         = 2,
  Shoulders    = 3,
  Biceps       = 4,
  Triceps      = 5,
  Legs         = 6,
  Core         = 7,
  Glutes       = 8,
  Cardio       = 9,
  FullBody     = 10,
}

// ─── DTOs (mirror de Application layer) ──────────────────────────────────────

export interface WorkoutPlanExerciseDto {
  id:                     string;
  workoutPlanDayId:       string;
  exerciseName:           string;
  muscleGroup:            MuscleGroup;
  muscleGroupName:        string;
  sortOrder:              number;
  plannedSets:            number | null;
  plannedReps:            number | null;
  plannedWeightKg:        number | null;
  plannedDurationMinutes: number | null;
  plannedDistanceMeters:  number | null;
  notes:                  string | null;
}

export interface WorkoutPlanDayDto {
  id:            string;
  workoutPlanId: string;
  dayOfWeek:     WorkoutDayOfWeek;
  dayOfWeekName: string;
  notes:         string | null;
  exercises:     WorkoutPlanExerciseDto[];
}

export interface WorkoutPlanDto {
  id:                       string;
  memberId:                 string;
  objective:                string;
  startDate:                string; // "YYYY-MM-DD"
  endDate:                  string;
  notes:                    string | null;
  initialWeightKg:          number | null;
  initialHeightCm:          number | null;
  initialBodyFatPercentage: number | null;
  status:                   WorkoutPlanStatus;
  statusName:               string;
  days:                     WorkoutPlanDayDto[];
  createdAtUtc:             string;
  modifiedAtUtc:            string | null;
}

export interface WorkoutPlanSummaryDto {
  id:           string;
  memberId:     string;
  objective:    string;
  startDate:    string;
  endDate:      string;
  status:       WorkoutPlanStatus;
  statusName:   string;
  dayCount:     number;
  createdAtUtc: string;
}

// ─── Requests ─────────────────────────────────────────────────────────────────

export interface CreateWorkoutPlanRequest {
  memberId:                 string;
  objective:                string;
  startDate:                string;
  endDate:                  string;
  notes:                    string | null;
  initialWeightKg:          number | null;
  initialHeightCm:          number | null;
  initialBodyFatPercentage: number | null;
}

export interface UpdateWorkoutPlanRequest {
  objective: string;
  startDate: string;
  endDate:   string;
  notes:     string | null;
}

export interface AddPlanDayRequest {
  dayOfWeek: WorkoutDayOfWeek;
  notes:     string | null;
}

export interface AddPlanExerciseRequest {
  exerciseName:           string;
  muscleGroup:            MuscleGroup;
  sortOrder:              number;
  plannedSets:            number | null;
  plannedReps:            number | null;
  plannedWeightKg:        number | null;
  plannedDurationMinutes: number | null;
  plannedDistanceMeters:  number | null;
}

// ─── Helpers / opciones para selects ─────────────────────────────────────────

export const WORKOUT_DAY_LABELS: Record<WorkoutDayOfWeek, string> = {
  [WorkoutDayOfWeek.Monday]:    'Lunes',
  [WorkoutDayOfWeek.Tuesday]:   'Martes',
  [WorkoutDayOfWeek.Wednesday]: 'Miércoles',
  [WorkoutDayOfWeek.Thursday]:  'Jueves',
  [WorkoutDayOfWeek.Friday]:    'Viernes',
  [WorkoutDayOfWeek.Saturday]:  'Sábado',
  [WorkoutDayOfWeek.Sunday]:    'Domingo',
};

export const WORKOUT_PLAN_STATUS_LABELS: Record<WorkoutPlanStatus, string> = {
  [WorkoutPlanStatus.Active]:    'Activa',
  [WorkoutPlanStatus.Completed]: 'Completada',
  [WorkoutPlanStatus.Cancelled]: 'Cancelada',
};

export const DAY_OF_WEEK_OPTIONS: { value: WorkoutDayOfWeek; label: string }[] = [
  { value: WorkoutDayOfWeek.Monday,    label: 'Lunes' },
  { value: WorkoutDayOfWeek.Tuesday,   label: 'Martes' },
  { value: WorkoutDayOfWeek.Wednesday, label: 'Miércoles' },
  { value: WorkoutDayOfWeek.Thursday,  label: 'Jueves' },
  { value: WorkoutDayOfWeek.Friday,    label: 'Viernes' },
  { value: WorkoutDayOfWeek.Saturday,  label: 'Sábado' },
  { value: WorkoutDayOfWeek.Sunday,    label: 'Domingo' },
];

export const MUSCLE_GROUP_OPTIONS: { value: MuscleGroup; label: string }[] = [
  { value: MuscleGroup.NotSpecified, label: 'Sin especificar' },
  { value: MuscleGroup.Chest,        label: 'Pecho' },
  { value: MuscleGroup.Back,         label: 'Espalda' },
  { value: MuscleGroup.Shoulders,    label: 'Hombros' },
  { value: MuscleGroup.Biceps,       label: 'Bíceps' },
  { value: MuscleGroup.Triceps,      label: 'Tríceps' },
  { value: MuscleGroup.Legs,         label: 'Piernas' },
  { value: MuscleGroup.Core,         label: 'Core / Abdomen' },
  { value: MuscleGroup.Glutes,       label: 'Glúteos' },
  { value: MuscleGroup.Cardio,       label: 'Cardio' },
  { value: MuscleGroup.FullBody,     label: 'Cuerpo completo' },
];

export const MUSCLE_GROUP_LABELS: Record<MuscleGroup, string> = {
  [MuscleGroup.NotSpecified]: 'Sin especificar',
  [MuscleGroup.Chest]:        'Pecho',
  [MuscleGroup.Back]:         'Espalda',
  [MuscleGroup.Shoulders]:    'Hombros',
  [MuscleGroup.Biceps]:       'Bíceps',
  [MuscleGroup.Triceps]:      'Tríceps',
  [MuscleGroup.Legs]:         'Piernas',
  [MuscleGroup.Core]:         'Core / Abdomen',
  [MuscleGroup.Glutes]:       'Glúteos',
  [MuscleGroup.Cardio]:       'Cardio',
  [MuscleGroup.FullBody]:     'Cuerpo completo',
};

export function muscleGroupBadgeClass(group: MuscleGroup): string {
  switch (group) {
    case MuscleGroup.Chest:        return 'badge badge-chest';
    case MuscleGroup.Back:         return 'badge badge-back';
    case MuscleGroup.Shoulders:    return 'badge badge-shoulders';
    case MuscleGroup.Biceps:
    case MuscleGroup.Triceps:      return 'badge badge-arms';
    case MuscleGroup.Legs:
    case MuscleGroup.Glutes:       return 'badge badge-legs';
    case MuscleGroup.Core:         return 'badge badge-core';
    case MuscleGroup.Cardio:       return 'badge badge-cardio';
    case MuscleGroup.FullBody:     return 'badge badge-fullbody';
    default:                       return 'badge';
  }
}
