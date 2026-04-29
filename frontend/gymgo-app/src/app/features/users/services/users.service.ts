import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  User,
  CreateUserRequest,
  UpdateUserRequest,
  ChangePasswordRequest,
} from '../models/user.models';
import { UserRole } from '../../../core/models/auth.models';

export interface GetUsersParams {
  search?: string;
  role?:   UserRole | null;
}

export interface GetUsersResult {
  items:      User[];
  totalCount: number;
}

@Injectable({ providedIn: 'root' })
export class UsersService {
  private readonly http   = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/users`;

  /** GET /api/v1/users — listado con filtros opcionales */
  getUsers(params: GetUsersParams = {}): Observable<GetUsersResult> {
    let httpParams = new HttpParams();

    if (params.search?.trim()) {
      httpParams = httpParams.set('search', params.search.trim());
    }
    if (params.role != null) {
      httpParams = httpParams.set('role', params.role.toString());
    }

    return this.http.get<GetUsersResult>(this.apiUrl, { params: httpParams });
  }

  /** GET /api/v1/users/{id} */
  getUserById(id: string): Observable<User> {
    return this.http.get<User>(`${this.apiUrl}/${id}`);
  }

  /** POST /api/v1/users */
  createUser(request: CreateUserRequest): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(this.apiUrl, request);
  }

  /** PUT /api/v1/users/{id} */
  updateUser(id: string, request: UpdateUserRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, request);
  }

  /** PATCH /api/v1/users/{id}/toggle-active */
  toggleActive(id: string, isActive: boolean): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/toggle-active`, { isActive });
  }

  /** PATCH /api/v1/users/{id}/password */
  changePassword(id: string, request: ChangePasswordRequest): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/password`, request);
  }
}
