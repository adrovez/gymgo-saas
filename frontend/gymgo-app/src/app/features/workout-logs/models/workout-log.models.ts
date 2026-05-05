// ─── Enums (mirror del dominio .NET) ─────────────────────────────────────────

export enum WorkoutLogStatus {
  Draft     = 0,
  Completed = 1,
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

export interface WorkoutLogExerciseDto {
  id:              string;
  workoutLogId:    string;
  exerciseName:    string;
  muscleGroup:     MuscleGroup;
  muscleGroupName: string;
  sortOrder:       number;
  sets:            number | null;
  reps:            number | null;
  weightKg:        number | null;
  durationSeconds: number | null;
  distanceMeters:  number | null;
  notes:           string | null;
}

export interface WorkoutLogSummaryDto {
  id:            string;
  memberId:      string;
  date:          string;   // "YYYY-MM-DD"
  title:         string | null;
  status:        WorkoutLogStatus;
  statusName:    string;
  exerciseCount: number;
  createdAtUtc:  string;
}

export interface WorkoutLogDto {
  id:           string;
  memberId:     string;
  date:         string;
  title:        string | null;
  notes:        string | null;
  status:       WorkoutLogStatus;
  statusName:   string;
  exercises:    WorkoutLogExerciseDto[];
  createdAtUtc: string;
  modifiedAtUtc: string | null;
}

// ─── Requests ─────────────────────────────────────────────────────────────────

export interface CreateWorkoutLogRequest {
  memberId: string;
  date:     string | null;
  title:    string | null;
  notes:    string | null;
}

export interface UpdateWorkoutLogRequest {
  title: string | null;
  notes: string | null;
}

export interface AddExerciseRequest {
  exerciseName:    string;
  muscleGroup:     MuscleGroup;
  sets:            number | null;
  reps:            number | null;
  weightKg:        number | null;
  durationSeconds: number | null;
  distanceMeters:  number | null;
  notes:           string | null;
}

export interface UpdateExerciseRequest {
  exerciseName:    string;
  muscleGroup:     MuscleGroup;
  sortOrder:       number;
  sets:            number | null;
  reps:            number | null;
  weightKg:        number | null;
  durationSeconds: number | null;
  distanceMeters:  number | null;
  notes:           string | null;
}

// ─── Helpers / opciones para selects ─────────────────────────────────────────

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

export const WORKOUT_STATUS_LABELS: Record<WorkoutLogStatus, string> = {
  [WorkoutLogStatus.Draft]:     'En curso',
  [WorkoutLogStatus.Completed]: 'Completada',
};

/** Color del badge de grupo muscular para las exercise cards */
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
