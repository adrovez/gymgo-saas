// ─── Enums (mirror del dominio .NET) ─────────────────────────────────────────

export enum MaintenanceType {
  Preventive = 0,
  Corrective  = 1,
}

export enum MaintenanceStatus {
  Pending    = 0,
  InProgress = 1,
  Completed  = 2,
  Cancelled  = 3,
}

export enum ResponsibleType {
  Internal = 0,
  External = 1,
}

// ─── Equipment DTOs ───────────────────────────────────────────────────────────

export interface EquipmentDto {
  id:           string;
  tenantId:     string;
  name:         string;
  brand:        string | null;
  model:        string | null;
  serialNumber: string | null;
  purchaseDate: string | null;   // ISO date "YYYY-MM-DD"
  imageUrl:     string | null;
  isActive:     boolean;
  createdAtUtc: string;
  createdBy:    string | null;
  modifiedAtUtc: string | null;
  modifiedBy:   string | null;
}

export interface EquipmentSummaryDto {
  id:           string;
  name:         string;
  brand:        string | null;
  model:        string | null;
  serialNumber: string | null;
  purchaseDate: string | null;
  imageUrl:     string | null;
  isActive:     boolean;
}

export interface CreateEquipmentRequest {
  name:         string;
  brand:        string | null;
  model:        string | null;
  serialNumber: string | null;
  purchaseDate: string | null;
  imageUrl:     string | null;
}

// ─── Maintenance DTOs ─────────────────────────────────────────────────────────

export interface MaintenanceRecordDto {
  id:                     string;
  tenantId:               string;
  equipmentId:            string;
  equipmentName:          string;
  type:                   MaintenanceType;
  typeLabel:              string;
  status:                 MaintenanceStatus;
  statusLabel:            string;
  scheduledDate:          string;   // ISO date "YYYY-MM-DD"
  startedAtUtc:           string | null;
  completedAtUtc:         string | null;
  description:            string;
  notes:                  string | null;
  cost:                   number | null;
  responsibleType:        ResponsibleType;
  responsibleTypeLabel:   string;
  responsibleUserId:      string | null;
  externalProviderName:   string | null;
  externalProviderContact: string | null;
  createdAtUtc:           string;
  createdBy:              string | null;
  modifiedAtUtc:          string | null;
  modifiedBy:             string | null;
}

export interface MaintenanceRecordSummaryDto {
  id:                   string;
  equipmentId:          string;
  equipmentName:        string;
  type:                 MaintenanceType;
  typeLabel:            string;
  status:               MaintenanceStatus;
  statusLabel:          string;
  scheduledDate:        string;
  description:          string;
  responsibleType:      ResponsibleType;
  responsibleTypeLabel: string;
  externalProviderName: string | null;
  cost:                 number | null;
}

export interface CreateMaintenanceRecordRequest {
  equipmentId:             string;
  type:                    MaintenanceType;
  scheduledDate:           string;
  description:             string;
  responsibleType:         ResponsibleType;
  responsibleUserId:       string | null;
  externalProviderName:    string | null;
  externalProviderContact: string | null;
}

export interface CompleteMaintenanceRequest {
  notes: string | null;
  cost:  number | null;
}

// ─── Helpers ─────────────────────────────────────────────────────────────────

export const MAINTENANCE_TYPE_OPTIONS: { value: MaintenanceType; label: string }[] = [
  { value: MaintenanceType.Preventive, label: 'Preventiva' },
  { value: MaintenanceType.Corrective,  label: 'Correctiva' },
];

export const MAINTENANCE_STATUS_OPTIONS: { value: MaintenanceStatus; label: string }[] = [
  { value: MaintenanceStatus.Pending,    label: 'Pendiente' },
  { value: MaintenanceStatus.InProgress, label: 'En Proceso' },
  { value: MaintenanceStatus.Completed,  label: 'Completada' },
  { value: MaintenanceStatus.Cancelled,  label: 'Cancelada' },
];

export const RESPONSIBLE_TYPE_OPTIONS: { value: ResponsibleType; label: string }[] = [
  { value: ResponsibleType.Internal, label: 'Staff interno' },
  { value: ResponsibleType.External, label: 'Proveedor externo' },
];

export const MAINTENANCE_STATUS_LABELS: Record<MaintenanceStatus, string> = {
  [MaintenanceStatus.Pending]:    'Pendiente',
  [MaintenanceStatus.InProgress]: 'En Proceso',
  [MaintenanceStatus.Completed]:  'Completada',
  [MaintenanceStatus.Cancelled]:  'Cancelada',
};

export const MAINTENANCE_TYPE_LABELS: Record<MaintenanceType, string> = {
  [MaintenanceType.Preventive]: 'Preventiva',
  [MaintenanceType.Corrective]:  'Correctiva',
};
