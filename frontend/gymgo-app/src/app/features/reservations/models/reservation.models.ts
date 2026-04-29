// ─────────────────────────────────────────────────────────────────────────────
//  GymGo — Models: ClassReservations
// ─────────────────────────────────────────────────────────────────────────────

export enum ReservationStatus {
  Active            = 0,
  CancelledByMember = 1,
  CancelledByStaff  = 2,
  NoShow            = 3,
}

export const RESERVATION_STATUS_LABELS: Record<ReservationStatus, string> = {
  [ReservationStatus.Active]:            'Activa',
  [ReservationStatus.CancelledByMember]: 'Cancelada por socio',
  [ReservationStatus.CancelledByStaff]:  'Cancelada por staff',
  [ReservationStatus.NoShow]:            'No Show',
};

export const RESERVATION_STATUS_CSS: Record<ReservationStatus, string> = {
  [ReservationStatus.Active]:            'text-green-700 bg-green-100',
  [ReservationStatus.CancelledByMember]: 'text-gray-600 bg-gray-100',
  [ReservationStatus.CancelledByStaff]:  'text-orange-700 bg-orange-100',
  [ReservationStatus.NoShow]:            'text-red-700 bg-red-100',
};

// ─────────────────────────────────────────────────────────────────────────────
//  DTOs
// ─────────────────────────────────────────────────────────────────────────────

export interface ClassReservationDto {
  id: string;
  memberId: string;
  memberFullName: string;
  classScheduleId: string;
  sessionDate: string;      // "YYYY-MM-DD"
  reservedAtUtc: string;
  status: ReservationStatus;
  notes: string | null;
  cancelledAtUtc: string | null;
  cancelledBy: string | null;
  cancelReason: string | null;
}

// ─────────────────────────────────────────────────────────────────────────────
//  Requests
// ─────────────────────────────────────────────────────────────────────────────

export interface CreateReservationRequest {
  memberId: string;
  classScheduleId: string;
  sessionDate: string;   // "YYYY-MM-DD"
  notes: string | null;
}

export interface CancelReservationRequest {
  cancelStatus: ReservationStatus;
  reason: string | null;
}
