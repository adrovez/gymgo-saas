import { Injectable } from '@angular/core';
import { AuthSession } from '../models/auth.models';

const SESSION_KEY = 'gymgo_session';

@Injectable({ providedIn: 'root' })
export class StorageService {
  saveSession(session: AuthSession): void {
    localStorage.setItem(SESSION_KEY, JSON.stringify(session));
  }

  getSession(): AuthSession | null {
    const raw = localStorage.getItem(SESSION_KEY);
    if (!raw) return null;
    try {
      return JSON.parse(raw) as AuthSession;
    } catch {
      return null;
    }
  }

  clearSession(): void {
    localStorage.removeItem(SESSION_KEY);
  }

  getToken(): string | null {
    return this.getSession()?.token ?? null;
  }

  getTenantId(): string | null {
    return this.getSession()?.tenantId ?? null;
  }

  isTokenValid(): boolean {
    const session = this.getSession();
    if (!session) return false;
    return new Date(session.expiresAtUtc) > new Date();
  }
}
