import { Component, inject, signal, OnInit } from '@angular/core';
import { SlicePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { UserRole } from '../../../core/models/auth.models';
import { DialogService } from '../../../core/services/dialog.service';
import { ASSIGNABLE_ROLES, User, USER_ROLE_LABELS } from '../models/user.models';
import { UsersService } from '../services/users.service';

@Component({
  selector: 'app-users-list',
  standalone: true,
  imports: [RouterLink, SlicePipe],
  templateUrl: './users-list.html',
})
export class UsersListComponent implements OnInit {
  private readonly usersService = inject(UsersService);
  private readonly dialog       = inject(DialogService);

  readonly loading         = signal(false);
  readonly search          = signal('');
  readonly roleFilter      = signal<UserRole | null>(null);
  readonly users           = signal<User[]>([]);

  readonly roleLabels      = USER_ROLE_LABELS;
  readonly assignableRoles = ASSIGNABLE_ROLES;
  readonly UserRole        = UserRole;

  ngOnInit(): void {
    this.loadUsers();
  }

  private loadUsers(): void {
    this.loading.set(true);
    this.usersService.getUsers({
      search: this.search() || undefined,
      role:   this.roleFilter(),
    }).subscribe({
      next:  (result) => { this.users.set(result.items); this.loading.set(false); },
      error: () => { this.loading.set(false); },
    });
  }

  onSearch(value: string): void {
    this.search.set(value);
    this.loadUsers();
  }

  onRoleFilterChange(value: string): void {
    this.roleFilter.set(value !== '' ? (Number(value) as UserRole) : null);
    this.loadUsers();
  }

  activateUser(user: User): void {
    this.usersService.toggleActive(user.id, true).subscribe({
      next: () => {
        this.dialog.toast('Usuario activado correctamente.', 'success');
        this.loadUsers();
      },
      error: () => this.dialog.toast('No se pudo activar el usuario.', 'error'),
    });
  }

  deactivateUser(user: User): void {
    this.dialog.confirmDanger(
      'Desactivar usuario',
      `¿Estás seguro de que deseas desactivar a "${user.fullName}"? No podrá iniciar sesión.`,
      'Desactivar'
    ).then((result) => {
      if (!result.isConfirmed) return;
      this.usersService.toggleActive(user.id, false).subscribe({
        next: () => {
          this.dialog.toast('Usuario desactivado.', 'success');
          this.loadUsers();
        },
        error: () => this.dialog.toast('No se pudo desactivar el usuario.', 'error'),
      });
    });
  }
}
