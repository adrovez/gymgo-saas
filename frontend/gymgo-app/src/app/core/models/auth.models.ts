export enum UserRole {
  PlatformAdmin = 0,
  GymOwner = 1,
  GymStaff = 2,
  Instructor = 3,
  Member = 4,
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  expiresAtUtc: string;
  userId: string;
  fullName: string;
  email: string;
  role: UserRole;
  tenantId: string;
}

export interface AuthSession {
  token: string;
  expiresAtUtc: string;
  userId: string;
  fullName: string;
  email: string;
  role: UserRole;
  tenantId: string;
}
