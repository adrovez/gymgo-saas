import { UserRole } from '../../../core/models/auth.models';

// ─── DTOs ────────────────────────────────────────────────────────────────────

export interface User {
  id: string;
  tenantId: string;
  email: string;
  fullName: string;
  role: UserRole;
  isActive: boolean;
  lastLoginUtc: string | null;
  createdAtUtc: string;
  createdBy: string | null;
}

export interface CreateUserRequest {
  fullName: string;
  email: string;
  password: string;
  role: UserRole;
}

export interface UpdateUserRequest {
  fullName: string;
  role: UserRole;
  isActive: boolean;
}

export interface ChangePasswordRequest {
  newPassword: string;
}

// ─── Helpers ─────────────────────────────────────────────────────────────────

export const USER_ROLE_LABELS: Record<UserRole, string> = {
  [UserRole.PlatformAdmin]: 'Platform Admin',
  [UserRole.GymOwner]: 'Dueño',
  [UserRole.GymStaff]: 'Staff',
  [UserRole.Instructor]: 'Instructor',
  [UserRole.Member]: 'Socio',
};

export const ASSIGNABLE_ROLES: { value: UserRole; label: string }[] = [
  { value: UserRole.GymStaff,   label: 'Staff' },
  { value: UserRole.Instructor, label: 'Instructor' },
];
