import { Component, signal } from '@angular/core';
import { SlicePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { UserRole } from '../../../core/models/auth.models';
import { ASSIGNABLE_ROLES, User, USER_ROLE_LABELS } from '../models/user.models';

@Component({
  selector: 'app-users-list',
  standalone: true,
  imports: [RouterLink, SlicePipe],
  templateUrl: './users-list.html',
})
export class UsersListComponent {
  readonly loading = signal(false);
  readonly search  = signal('');
  readonly users   = signal<User[]>([]);

  readonly roleLabels      = USER_ROLE_LABELS;
  readonly assignableRoles = ASSIGNABLE_ROLES;
  readonly UserRole        = UserRole;

  onSearch(value: string): void {
    this.search.set(value);
    // TODO: filtrar lista
  }

  onRoleFilterChange(value: string): void {
    // TODO: aplicar filtro de rol
  }

  activateUser(_user: User): void {
    // TODO: llamar al servicio
  }

  deactivateUser(_user: User): void {
    // TODO: llamar al servicio
  }
}
