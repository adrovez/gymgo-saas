// ─── Enums (mirror del dominio .NET) ─────────────────────────────────────────

export enum Periodicity {
  Monthly   = 1,
  Quarterly = 2,
  Biannual  = 3,
  Annual    = 4,
}

// ─── DTOs ────────────────────────────────────────────────────────────────────

/** Detalle completo — respuesta de GET /membership-plans/{id} */
export interface MembershipPlanDto {
  id:                string;
  tenantId:          string;
  name:              string;
  description:       string | null;
  periodicity:       Periodicity;
  periodicityLabel:  string;
  durationDays:      number;
  daysPerWeek:       number;
  fixedDays:         boolean;
  monday:            boolean;
  tuesday:           boolean;
  wednesday:         boolean;
  thursday:          boolean;
  friday:            boolean;
  saturday:          boolean;
  sunday:            boolean;
  daysLabel:         string;
  freeSchedule:      boolean;
  timeFrom:          string | null;  // "HH:mm:ss"
  timeTo:            string | null;  // "HH:mm:ss"
  scheduleLabel:     string;
  amount:            number;
  allowsFreezing:    boolean;
  isActive:          boolean;
  deactivatedAtUtc:  string | null;
  createdAtUtc:      string;
  createdBy:         string | null;
  modifiedAtUtc:     string | null;
  modifiedBy:        string | null;
}

/** Vista resumida para listados — respuesta de GET /membership-plans */
export interface MembershipPlanSummaryDto {
  id:               string;
  name:             string;
  periodicity:      Periodicity;
  periodicityLabel: string;
  durationDays:     number;
  daysPerWeek:      number;
  daysLabel:        string;
  scheduleLabel:    string;
  amount:           number;
  allowsFreezing:   boolean;
  isActive:         boolean;
}

/** Body para POST /membership-plans */
export interface CreateMembershipPlanRequest {
  name:           string;
  description:    string | null;
  periodicity:    Periodicity;
  daysPerWeek:    number;
  fixedDays:      boolean;
  monday:         boolean;
  tuesday:        boolean;
  wednesday:      boolean;
  thursday:       boolean;
  friday:         boolean;
  saturday:       boolean;
  sunday:         boolean;
  freeSchedule:   boolean;
  timeFrom:       string | null;  // "HH:mm:ss"
  timeTo:         string | null;  // "HH:mm:ss"
  amount:         number;
  allowsFreezing: boolean;
}

/** Body para PUT /membership-plans/{id} */
export interface UpdateMembershipPlanRequest {
  name:           string;
  description:    string | null;
  periodicity:    Periodicity;
  daysPerWeek:    number;
  fixedDays:      boolean;
  monday:         boolean;
  tuesday:        boolean;
  wednesday:      boolean;
  thursday:       boolean;
  friday:         boolean;
  saturday:       boolean;
  sunday:         boolean;
  freeSchedule:   boolean;
  timeFrom:       string | null;  // "HH:mm:ss"
  timeTo:         string | null;  // "HH:mm:ss"
  amount:         number;
  allowsFreezing: boolean;
}

// ─── Helpers ─────────────────────────────────────────────────────────────────

export const PERIODICITY_OPTIONS: { value: Periodicity; label: string; days: number }[] = [
  { value: Periodicity.Monthly,   label: 'Mensual',    days: 30 },
  { value: Periodicity.Quarterly, label: 'Trimestral', days: 90 },
  { value: Periodicity.Biannual,  label: 'Semestral',  days: 180 },
  { value: Periodicity.Annual,    label: 'Anual',      days: 365 },
];

export const PERIODICITY_LABELS: Record<Periodicity, string> = {
  [Periodicity.Monthly]:   'Mensual',
  [Periodicity.Quarterly]: 'Trimestral',
  [Periodicity.Biannual]:  'Semestral',
  [Periodicity.Annual]:    'Anual',
};

export const DAYS_OF_WEEK: { key: keyof CreateMembershipPlanRequest; label: string }[] = [
  { key: 'monday',    label: 'Lun' },
  { key: 'tuesday',   label: 'Mar' },
  { key: 'wednesday', label: 'Mié' },
  { key: 'thursday',  label: 'Jue' },
  { key: 'friday',    label: 'Vie' },
  { key: 'saturday',  label: 'Sáb' },
  { key: 'sunday',    label: 'Dom' },
];

/** Convierte "HH:mm:ss" → "HH:mm" para <input type="time"> */
export function toTimeInput(value: string | null): string {
  if (!value) return '';
  return value.substring(0, 5);  // "HH:mm"
}

/** Convierte "HH:mm" → "HH:mm:ss" para la API */
export function toTimeApi(value: string): string | null {
  if (!value) return null;
  return value.length === 5 ? `${value}:00` : value;
}
