// ── Enums ─────────────────────────────────────────────────────────────────────

export enum SaleStatus {
  Draft     = 0,
  Confirmed = 1,
  Voided    = 2,
}

export enum SaleItemType {
  Plan          = 0,
  Matricula     = 1,
  ServicioExtra = 2,
  Otro          = 3,
}

export enum SaleDiscountType {
  MontoFijo  = 0,
  Porcentaje = 1,
}

// Re-exportamos PaymentMethod desde aquí para comodidad del módulo
export enum SalePaymentMethod {
  Efectivo      = 0,
  Tarjeta       = 1,
  Transferencia = 2,
}

// ── DTOs ─────────────────────────────────────────────────────────────────────

export interface SaleItemDto {
  id:               string;
  itemType:         SaleItemType;
  itemTypeLabel:    string;
  description:      string;
  membershipPlanId: string | null;
  planName:         string | null;
  unitPrice:        number;
  quantity:         number;
  subtotal:         number;
}

export interface SaleDiscountDto {
  id:            string;
  discountType:  SaleDiscountType;
  discountTypeLabel: string;
  description:   string;
  value:         number;
  amount:        number;
}

export interface SaleDto {
  id:                     string;
  memberId:               string;
  memberFullName:         string | null;
  memberRut:              string | null;
  date:                   string; // 'YYYY-MM-DD'
  status:                 SaleStatus;
  statusLabel:            string;
  paymentMethod:          SalePaymentMethod;
  paymentMethodLabel:     string;
  subtotalAmount:         number;
  discountAmount:         number;
  totalAmount:            number;
  notes:                  string | null;
  membershipAssignmentId: string | null;
  cashTransactionId:      string | null;
  voidedAtUtc:            string | null;
  voidReason:             string | null;
  createdAtUtc:           string;
  items:                  SaleItemDto[];
  discounts:              SaleDiscountDto[];
}

export interface SaleSummaryDto {
  id:                 string;
  memberId:           string;
  memberFullName:     string | null;
  memberRut:          string | null;
  date:               string;
  status:             SaleStatus;
  statusLabel:        string;
  paymentMethod:      SalePaymentMethod;
  paymentMethodLabel: string;
  subtotalAmount:     number;
  discountAmount:     number;
  totalAmount:        number;
  itemCount:          number;
}

// ── Requests ─────────────────────────────────────────────────────────────────

export interface CreateSaleRequest {
  memberId:      string;
  date:          string | null;
  paymentMethod: SalePaymentMethod;
  notes:         string | null;
}

export interface AddSaleItemRequest {
  itemType:         SaleItemType;
  description:      string;
  membershipPlanId: string | null;
  unitPrice:        number;
  quantity:         number | null;
}

export interface UpdateSaleItemRequest {
  description: string;
  unitPrice:   number;
  quantity:    number;
}

export interface AddSaleDiscountRequest {
  discountType: SaleDiscountType;
  description:  string;
  value:        number;
}

export interface VoidSaleRequest {
  reason: string;
}

export interface CheckoutSaleItemRequest {
  itemType:         SaleItemType;
  description:      string;
  membershipPlanId: string | null;
  unitPrice:        number;
  quantity:         number | null;
}

export interface CheckoutSaleDiscountRequest {
  discountType: SaleDiscountType;
  description:  string;
  value:        number;
}

export interface CheckoutSaleRequest {
  memberId:      string;
  date:          string | null;
  paymentMethod: SalePaymentMethod;
  notes:         string | null;
  items:         CheckoutSaleItemRequest[];
  discount:      CheckoutSaleDiscountRequest | null;
}

// ── Helpers ───────────────────────────────────────────────────────────────────

export const SALE_STATUS_LABELS: Record<SaleStatus, string> = {
  [SaleStatus.Draft]:     'Borrador',
  [SaleStatus.Confirmed]: 'Confirmada',
  [SaleStatus.Voided]:    'Anulada',
};

export const SALE_ITEM_TYPE_LABELS: Record<SaleItemType, string> = {
  [SaleItemType.Plan]:          'Plan membresía',
  [SaleItemType.Matricula]:     'Matrícula',
  [SaleItemType.ServicioExtra]: 'Servicio extra',
  [SaleItemType.Otro]:          'Otro',
};

export const SALE_PAYMENT_METHOD_LABELS: Record<SalePaymentMethod, string> = {
  [SalePaymentMethod.Efectivo]:      'Efectivo',
  [SalePaymentMethod.Tarjeta]:       'Tarjeta',
  [SalePaymentMethod.Transferencia]: 'Transferencia',
};

export const SALE_DISCOUNT_TYPE_LABELS: Record<SaleDiscountType, string> = {
  [SaleDiscountType.MontoFijo]:  'Monto fijo',
  [SaleDiscountType.Porcentaje]: 'Porcentaje',
};

export const SALE_ITEM_TYPE_OPTIONS: { value: SaleItemType; label: string }[] = [
  { value: SaleItemType.Plan,          label: SALE_ITEM_TYPE_LABELS[SaleItemType.Plan] },
  { value: SaleItemType.Matricula,     label: SALE_ITEM_TYPE_LABELS[SaleItemType.Matricula] },
  { value: SaleItemType.ServicioExtra, label: SALE_ITEM_TYPE_LABELS[SaleItemType.ServicioExtra] },
  { value: SaleItemType.Otro,          label: SALE_ITEM_TYPE_LABELS[SaleItemType.Otro] },
];

export const SALE_PAYMENT_METHOD_OPTIONS: { value: SalePaymentMethod; label: string }[] = [
  { value: SalePaymentMethod.Efectivo,      label: SALE_PAYMENT_METHOD_LABELS[SalePaymentMethod.Efectivo] },
  { value: SalePaymentMethod.Tarjeta,       label: SALE_PAYMENT_METHOD_LABELS[SalePaymentMethod.Tarjeta] },
  { value: SalePaymentMethod.Transferencia, label: SALE_PAYMENT_METHOD_LABELS[SalePaymentMethod.Transferencia] },
];
