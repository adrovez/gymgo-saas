// Re-exportar MuscleGroup y helpers desde workout-plans (definición canónica)
export {
  MuscleGroup,
  MUSCLE_GROUP_OPTIONS,
  MUSCLE_GROUP_LABELS,
  muscleGroupBadgeClass,
} from '../../workout-plans/models/workout-plan.models';

import { MuscleGroup } from '../../workout-plans/models/workout-plan.models';

// ─── Enums propios de WorkoutLog ─────────────────────────────────────────────

export enum WorkoutLogStatus {
  Draft     = 0,
  Completed = 1,
}

// ─── DTOs (mirror de Application layer) ──────────────────────────────────────

export interface WorkoutLogExerciseDto {
  id:                    string;
  workoutLogId:          string;
  workoutPlanExerciseId: string | null;
  exerciseName:          string;
  muscleGroup:           MuscleGroup;
  muscleGroupName:       string;
  sortOrder:             number;
  isExtra:               boolean;
  actualSets:            number | null;
  actualReps:            number | null;
  actualWeightKg:        number | null;
  actualDurationMinutes: number | null;
  actualDistanceMeters:  number | null;
  notes:                 string | null;
}

export interface WorkoutLogSummaryDto {
  id:               string;
  memberId:         string;
  workoutPlanId:    string;
  workoutPlanDayId: string;
  dayOfWeekName:    string;
  date:             string; // "YYYY-MM-DD"
  status:           WorkoutLogStatus;
  statusName:       string;
  exerciseCount:    number;
  createdAtUtc:     string;
}

export interface WorkoutLogDto {
  id:               string;
  memberId:         string;
  workoutPlanId:    string;
  workoutPlanDayId: string;
  dayOfWeekName:    string;
  date:             string;
  notes:            string | null;
  status:           WorkoutLogStatus;
  statusName:       string;
  exercises:        WorkoutLogExerciseDto[];
  createdAtUtc:     string;
  modifiedAtUtc:    string | null;
}

// ─── Requests ─────────────────────────────────────────────────────────────────

export interface CreateWorkoutLogRequest {
  memberId:         string;
  workoutPlanId:    string;
  workoutPlanDayId: string;
  date:             string | null;
  notes:            string | null;
}

export interface UpdateWorkoutLogRequest {
  notes: string | null;
}

export interface AddExerciseRequest {
  workoutPlanExerciseId: string | null;
  isExtra:               boolean;
  exerciseName:          string;
  muscleGroup:           MuscleGroup;
  actualSets:            number | null;
  actualReps:            number | null;
  actualWeightKg:        number | null;
  actualDurationMinutes: number | null;
  actualDistanceMeters:  number | null;
  notes:                 string | null;
}

export interface UpdateExerciseRequest {
  exerciseName:          string;
  muscleGroup:           MuscleGroup;
  sortOrder:             number;
  actualSets:            number | null;
  actualReps:            number | null;
  actualWeightKg:        number | null;
  actualDurationMinutes: number | null;
  actualDistanceMeters:  number | null;
  notes:                 string | null;
}

// ─── Helpers ─────────────────────────────────────────────────────────────────

export const WORKOUT_STATUS_LABELS: Record<WorkoutLogStatus, string> = {
  [WorkoutLogStatus.Draft]:     'En curso',
  [WorkoutLogStatus.Completed]: 'Completada',
};
