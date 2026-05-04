// ─── Enums ────────────────────────────────────────────────────────────────────

export enum GymEntryMethod {
  Manual = 0,
  QR     = 1,
  Badge  = 2,
}

// ─── DTOs ────────────────────────────────────────────────────────────────────

export interface GymEntryDto {
  id:                    string;
  memberId:              string;
  memberFullName:        string;
  membershipAssignmentId: string;
  entryDate:             string;   // ISO date "YYYY-MM-DD"
  enteredAtUtc:          string;   // ISO datetime
  exitedAtUtc:           string | null;  // null = socio aún dentro del gimnasio
  method:                string;
  notes:                 string | null;
  createdAtUtc:          string;
}

// ─── Requests ─────────────────────────────────────────────────────────────────

export interface RegisterGymEntryRequest {
  memberId: string;
  method:   GymEntryMethod;
  notes:    string | null;
}

// ─── Helpers ─────────────────────────────────────────────────────────────────

export const GYM_ENTRY_METHOD_OPTIONS: { value: GymEntryMethod; label: string; icon: string }[] = [
  { value: GymEntryMethod.Manual, label: 'Manual',  icon: 'M15.232 5.232l3.536 3.536m-2.036-5.036a2.5 2.5 0 113.536 3.536L6.5 21.036H3v-3.572L16.732 3.732z' },
  { value: GymEntryMethod.QR,     label: 'Código QR', icon: 'M12 4v1m6 11h2m-6 0h-2v4m0-11v3m0 0h.01M12 12h4.01M16 20h4M4 12h4m12 4h.01M5 8h2a1 1 0 001-1V5a1 1 0 00-1-1H5a1 1 0 00-1 1v2a1 1 0 001 1zm12 0h2a1 1 0 001-1V5a1 1 0 00-1-1h-2a1 1 0 00-1 1v2a1 1 0 001 1zM5 20h2a1 1 0 001-1v-2a1 1 0 00-1-1H5a1 1 0 00-1 1v2a1 1 0 001 1z' },
  { value: GymEntryMethod.Badge,  label: 'Tarjeta',   icon: 'M3 10h18M7 15h1m4 0h1m-7 4h12a3 3 0 003-3V8a3 3 0 00-3-3H6a3 3 0 00-3 3v8a3 3 0 003 3z' },
];

export const GYM_ENTRY_METHOD_LABELS: Record<GymEntryMethod, string> = {
  [GymEntryMethod.Manual]: 'Manual',
  [GymEntryMethod.QR]:     'QR',
  [GymEntryMethod.Badge]:  'Tarjeta',
};
