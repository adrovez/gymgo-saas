export enum CashTransactionType {
  Ingreso = 0,
  Egreso  = 1,
}

export enum CashPaymentMethod {
  Efectivo      = 0,
  Tarjeta       = 1,
  Transferencia = 2,
}

export enum TransactionConcept {
  // Ingresos (0–3)
  CuotaMembresia  = 0,
  Matricula       = 1,
  ProductoServicio = 2,
  OtroIngreso     = 3,
  // Egresos (10–13)
  Servicios  = 10,
  Mantencion = 11,
  Insumos    = 12,
  OtroEgreso = 13,
}

// ── DTOs ─────────────────────────────────────────────────────────────────────

export interface CashTransactionDto {
  id:                     string;
  date:                   string; // 'YYYY-MM-DD'
  type:                   CashTransactionType;
  amount:                 number;
  paymentMethod:          CashPaymentMethod;
  concept:                TransactionConcept;
  description:            string | null;
  memberId:               string | null;
  memberFullName:         string | null;
  membershipAssignmentId: string | null;
  isVoided:               boolean;
  voidedAtUtc:            string | null;
  voidReason:             string | null;
  createdAtUtc:           string;
}

export interface CashSummaryDto {
  totalIncome:              number;
  totalExpenses:            number;
  netBalance:               number;
  incomeByPaymentMethod:    Record<string, number>;
  expensesByPaymentMethod:  Record<string, number>;
  incomeByConcept:          Record<string, number>;
  expensesByConcept:        Record<string, number>;
  transactionCount:         number;
  voidedCount:              number;
}

// ── Requests ─────────────────────────────────────────────────────────────────

export interface RegisterTransactionRequest {
  date:                   string;
  type:                   CashTransactionType;
  amount:                 number;
  paymentMethod:          CashPaymentMethod;
  concept:                TransactionConcept;
  description:            string | null;
  memberId:               string | null;
  membershipAssignmentId: string | null;
}

export interface VoidTransactionRequest {
  reason: string;
}

// ── Helpers ───────────────────────────────────────────────────────────────────

export const PAYMENT_METHOD_LABELS: Record<CashPaymentMethod, string> = {
  [CashPaymentMethod.Efectivo]:      'Efectivo',
  [CashPaymentMethod.Tarjeta]:       'Tarjeta',
  [CashPaymentMethod.Transferencia]: 'Transferencia',
};

export const CONCEPT_LABELS: Record<TransactionConcept, string> = {
  [TransactionConcept.CuotaMembresia]:  'Cuota membresía',
  [TransactionConcept.Matricula]:       'Matrícula',
  [TransactionConcept.ProductoServicio]:'Producto / Servicio',
  [TransactionConcept.OtroIngreso]:     'Otro ingreso',
  [TransactionConcept.Servicios]:       'Servicios',
  [TransactionConcept.Mantencion]:      'Mantención',
  [TransactionConcept.Insumos]:         'Insumos',
  [TransactionConcept.OtroEgreso]:      'Otro egreso',
};

export const INCOME_CONCEPTS: { value: TransactionConcept; label: string }[] = [
  { value: TransactionConcept.CuotaMembresia,   label: CONCEPT_LABELS[TransactionConcept.CuotaMembresia] },
  { value: TransactionConcept.Matricula,         label: CONCEPT_LABELS[TransactionConcept.Matricula] },
  { value: TransactionConcept.ProductoServicio,  label: CONCEPT_LABELS[TransactionConcept.ProductoServicio] },
  { value: TransactionConcept.OtroIngreso,       label: CONCEPT_LABELS[TransactionConcept.OtroIngreso] },
];

export const EXPENSE_CONCEPTS: { value: TransactionConcept; label: string }[] = [
  { value: TransactionConcept.Servicios,  label: CONCEPT_LABELS[TransactionConcept.Servicios] },
  { value: TransactionConcept.Mantencion, label: CONCEPT_LABELS[TransactionConcept.Mantencion] },
  { value: TransactionConcept.Insumos,    label: CONCEPT_LABELS[TransactionConcept.Insumos] },
  { value: TransactionConcept.OtroEgreso, label: CONCEPT_LABELS[TransactionConcept.OtroEgreso] },
];

export const PAYMENT_METHOD_OPTIONS: { value: CashPaymentMethod; label: string }[] = [
  { value: CashPaymentMethod.Efectivo,      label: PAYMENT_METHOD_LABELS[CashPaymentMethod.Efectivo] },
  { value: CashPaymentMethod.Tarjeta,       label: PAYMENT_METHOD_LABELS[CashPaymentMethod.Tarjeta] },
  { value: CashPaymentMethod.Transferencia, label: PAYMENT_METHOD_LABELS[CashPaymentMethod.Transferencia] },
];
