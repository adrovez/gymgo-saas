// ─── Enums (mirror del dominio .NET) ─────────────────────────────────────────

export enum Gender {
  NotSpecified = 0,
  Male         = 1,
  Female       = 2,
  Other        = 3,
}

export enum MemberStatus {
  Active     = 0,
  Suspended  = 1,
  Delinquent = 2,
}

// ─── DTOs ────────────────────────────────────────────────────────────────────

/** Detalle completo — respuesta de GET /members/{id}  */
export interface MemberDto {
  id:                    string;
  tenantId:              string;
  rut:                   string;
  firstName:             string;
  lastName:              string;
  fullName:              string;
  birthDate:             string;   // ISO date "YYYY-MM-DD"
  age:                   number;
  gender:                Gender;
  genderLabel:           string;
  email:                 string | null;
  phone:                 string | null;
  address:               string | null;
  emergencyContactName:  string | null;
  emergencyContactPhone: string | null;
  status:                MemberStatus;
  statusLabel:           string;
  registrationDate:      string;   // ISO date "YYYY-MM-DD"
  notes:                 string | null;
  createdAtUtc:          string;
  createdBy:             string | null;
  modifiedAtUtc:         string | null;
  modifiedBy:            string | null;
}

/** Resumen para listados paginados */
export interface MemberSummaryDto {
  id:               string;
  rut:              string;
  fullName:         string;
  email:            string | null;
  phone:            string | null;
  status:           MemberStatus;
  statusLabel:      string;
  registrationDate: string;   // ISO date "YYYY-MM-DD"
}

/** Resultado paginado de GET /members */
export interface GetMembersResult {
  items:      MemberSummaryDto[];
  totalCount: number;
  page:       number;
  pageSize:   number;
  totalPages: number;
}

/** Body para POST /members */
export interface CreateMemberRequest {
  rut:                   string;
  firstName:             string;
  lastName:              string;
  birthDate:             string;
  gender:                Gender;
  email:                 string | null;
  phone:                 string | null;
  address:               string | null;
  emergencyContactName:  string | null;
  emergencyContactPhone: string | null;
  registrationDate:      string | null;
  notes:                 string | null;
}

/** Body para PUT /members/{id} */
export interface UpdateMemberRequest {
  firstName:             string;
  lastName:              string;
  birthDate:             string;
  gender:                Gender;
  email:                 string | null;
  phone:                 string | null;
  address:               string | null;
  emergencyContactName:  string | null;
  emergencyContactPhone: string | null;
  notes:                 string | null;
}

/** Body para PATCH /members/{id}/status */
export interface ChangeMemberStatusRequest {
  newStatus: MemberStatus;
}

// ─── Helpers ─────────────────────────────────────────────────────────────────

export const GENDER_OPTIONS: { value: Gender; label: string }[] = [
  { value: Gender.NotSpecified, label: 'No especificado' },
  { value: Gender.Male,         label: 'Masculino' },
  { value: Gender.Female,       label: 'Femenino' },
  { value: Gender.Other,        label: 'Otro' },
];

export const MEMBER_STATUS_OPTIONS: { value: MemberStatus; label: string }[] = [
  { value: MemberStatus.Active,     label: 'Activo' },
  { value: MemberStatus.Suspended,  label: 'Suspendido' },
  { value: MemberStatus.Delinquent, label: 'Moroso' },
];

export const MEMBER_STATUS_LABELS: Record<MemberStatus, string> = {
  [MemberStatus.Active]:     'Activo',
  [MemberStatus.Suspended]:  'Suspendido',
  [MemberStatus.Delinquent]: 'Moroso',
};
