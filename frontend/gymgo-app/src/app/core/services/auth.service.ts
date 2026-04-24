import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { LoginRequest, LoginResponse, AuthSession } from '../models/auth.models';
import { StorageService } from './storage.service';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly storage = inject(StorageService);

  private readonly _session = signal<AuthSession | null>(this.storage.getSession());

  readonly session = this._session.asReadonly();
  readonly isAuthenticated = computed(() => {
    const s = this._session();
    if (!s) return false;
    return new Date(s.expiresAtUtc) > new Date();
  });
  readonly currentUser = computed(() => this._session());

  login(credentials: LoginRequest, tenantId?: string): Observable<LoginResponse> {
    let headers = new HttpHeaders({ 'Content-Type': 'application/json' });
    if (tenantId?.trim()) {
      headers = headers.set('X-Tenant-Id', tenantId.trim());
    }

    return this.http
      .post<LoginResponse>(`${environment.apiUrl}/auth/login`, credentials, { headers })
      .pipe(
        tap((response) => {
          const session: AuthSession = { ...response };
          this.storage.saveSession(session);
          this._session.set(session);
        })
      );
  }

  logout(): void {
    this.storage.clearSession();
    this._session.set(null);
    this.router.navigate(['/login']);
  }
}
