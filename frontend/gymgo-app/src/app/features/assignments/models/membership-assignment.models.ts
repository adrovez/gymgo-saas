// ─── Enums (mirror del dominio .NET) ─────────────────────────────────────────

export enum AssignmentStatus {
  Active    = 0,
  Expired   = 1,
  Cancelled = 2,
  Frozen    = 3,
}

export enum PaymentStatus {
  Pending = 0,
  Paid    = 1,
  Overdue = 2,
}

// ─── DTOs ────────────────────────────────────────────────────────────────────

/** Detalle completo — respuesta de GET /members/{memberId}/assignments/active */
export interface MembershipAssignmentDto {
  id:                     string;
  tenantId:               string;
  memberId:               string;
  membershipPlanId:       string;
  startDate:              string;   // "YYYY-MM-DD"
  endDate:                string;   // "YYYY-MM-DD"
  daysRemaining:          number;
  amountSnapshot:         number;
  status:                 AssignmentStatus;
  statusLabel:            string;
  paymentStatus:          PaymentStatus;
  paymentStatusLabel:     string;
  paidAtUtc:              string | null;
  frozenSince:            string | null;  // "YYYY-MM-DD"
  frozenDaysAccumulated:  number;
  notes:                  string | null;
  createdAtUtc:           string;
  createdBy:              string | null;
  modifiedAtUtc:          string | null;
  modifiedBy:             string | null;
}

/** Vista resumida para listados — respuesta de GET /members/{memberId}/assignments, GET /assignments/overdue y GET /assignments/search */
export interface MembershipAssignmentSummaryDto {
  id:                 string;
  memberId:           string;
  /** Nombre completo del socio (vacío si el contexto ya lo conoce) */
  memberFullName:     string;
  /** RUT del socio (vacío si el contexto ya lo conoce) */
  memberRut:          string;
  membershipPlanId:   string;
  /** Nombre del plan de membresía */
  planName:           string;
  startDate:          string;   // "YYYY-MM-DD"
  endDate:            string;   // "YYYY-MM-DD"
  daysRemaining:      number;
  amountSnapshot:     number;
  status:             AssignmentStatus;
  statusLabel:        string;
  paymentStatus:      PaymentStatus;
  paymentStatusLabel: string;
}

/** Body para POST /members/{memberId}/assignments */
export interface AssignMembershipPlanRequest {
  membershipPlanId: string;
  startDate:        string | null;  // "YYYY-MM-DD" o null para usar hoy
  notes:            string | null;
}

// ─── Helpers ─────────────────────────────────────────────────────────────────

export const ASSIGNMENT_STATUS_LABELS: Record<AssignmentStatus, string> = {
  [AssignmentStatus.Active]:    'Activa',
  [AssignmentStatus.Expired]:   'Vencida',
  [AssignmentStatus.Cancelled]: 'Cancelada',
  [AssignmentStatus.Frozen]:    'Congelada',
};

export const PAYMENT_STATUS_LABELS: Record<PaymentStatus, string> = {
  [PaymentStatus.Pending]: 'Pendiente',
  [PaymentStatus.Paid]:    'Pagada',
  [PaymentStatus.Overdue]: 'Morosa',
};

export const ASSIGNMENT_STATUS_OPTIONS: { value: AssignmentStatus; label: string }[] = [
  { value: AssignmentStatus.Active,    label: 'Activa' },
  { value: AssignmentStatus.Expired,   label: 'Vencida' },
  { value: AssignmentStatus.Cancelled, label: 'Cancelada' },
  { value: AssignmentStatus.Frozen,    label: 'Congelada' },
];

export const PAYMENT_STATUS_OPTIONS: { value: PaymentStatus; label: string }[] = [
  { value: PaymentStatus.Pending, label: 'Pendiente' },
  { value: PaymentStatus.Paid,    label: 'Pagada' },
  { value: PaymentStatus.Overdue, label: 'Morosa' },
];
