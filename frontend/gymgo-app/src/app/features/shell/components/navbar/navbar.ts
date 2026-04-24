import { Component, inject, output } from '@angular/core';
import { AuthService } from '../../../../core/services/auth.service';
import { UserRole } from '../../../../core/models/auth.models';

const ROLE_LABELS: Record<UserRole, string> = {
  [UserRole.PlatformAdmin]: 'Platform Admin',
  [UserRole.GymOwner]: 'Propietario',
  [UserRole.GymStaff]: 'Staff',
  [UserRole.Instructor]: 'Instructor',
  [UserRole.Member]: 'Socio',
};

@Component({
  selector: 'app-navbar',
  standalone: true,
  templateUrl: './navbar.html',
})
export class NavbarComponent {
  private readonly auth = inject(AuthService);

  toggleSidebar = output<void>();

  readonly session = this.auth.session;

  getRoleLabel(role: UserRole): string {
    return ROLE_LABELS[role] ?? 'Usuario';
  }
}
