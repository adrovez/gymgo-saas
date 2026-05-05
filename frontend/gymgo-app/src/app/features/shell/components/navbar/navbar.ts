import {
  Component,
  HostListener,
  inject,
  signal,
} from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';
import { UserRole } from '../../../../core/models/auth.models';

const ROLE_LABELS: Record<UserRole, string> = {
  [UserRole.PlatformAdmin]: 'Platform Admin',
  [UserRole.GymOwner]: 'Propietario',
  [UserRole.GymStaff]: 'Staff',
  [UserRole.Instructor]: 'Instructor',
  [UserRole.Member]: 'Socio',
};

interface NavLink {
  type: 'link';
  label: string;
  icon: string;
  route: string;
}

interface NavGroupItem {
  label: string;
  icon: string;
  route: string;
}

interface NavGroup {
  type: 'group';
  label: string;
  icon: string;
  items: NavGroupItem[];
}

type NavEntry = NavLink | NavGroup;

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './navbar.html',
})
export class NavbarComponent {
  private readonly auth   = inject(AuthService);
  private readonly router = inject(Router);

  readonly session        = this.auth.session;
  readonly mobileMenuOpen = signal(false);
  readonly openGroup      = signal<string | null>(null);

  readonly navEntries: NavEntry[] = [
    {
      type: 'link',
      label: 'Dashboard',
      icon: 'M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6',
      route: '/app/dashboard',
    },
    {
      type: 'link',
      label: 'Ingreso',
      icon: 'M11 16l-4-4m0 0l4-4m-4 4h14m-5 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h7a3 3 0 013 3v1',
      route: '/app/gym-entry',
    },
    {
      type: 'group',
      label: 'Personas',
      icon: 'M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0z',
      items: [
        {
          label: 'Socios',
          icon: 'M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z',
          route: '/app/members',
        },
        {
          label: 'Usuarios',
          icon: 'M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z',
          route: '/app/users',
        },
      ],
    },
    {
      type: 'group',
      label: 'Finanzas',
      icon: 'M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z',
      items: [
        {
          label: 'Caja',
          icon: 'M3 10h18M7 15h1m4 0h1m-7 4h12a3 3 0 003-3V8a3 3 0 00-3-3H6a3 3 0 00-3 3v8a3 3 0 003 3z',
          route: '/app/cash',
        },
        {
          label: 'Membresías',
          icon: 'M15 5v2m0 4v2m0 4v2M5 5a2 2 0 00-2 2v3a2 2 0 110 4v3a2 2 0 002 2h14a2 2 0 002-2v-3a2 2 0 110-4V7a2 2 0 00-2-2H5z',
          route: '/app/assignments',
        },
        {
          label: 'Planes',
          icon: 'M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2',
          route: '/app/membership-plans',
        },
      ],
    },
    {
      type: 'group',
      label: 'Gimnasio',
      icon: 'M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z',
      items: [
        {
          label: 'Clases',
          icon: 'M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z',
          route: '/app/classes',
        },
        {
          label: 'Maquinaria',
          icon: 'M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z M15 12a3 3 0 11-6 0 3 3 0 016 0z',
          route: '/app/equipment',
        },
        {
          label: 'Mantención',
          icon: 'M11 4a2 2 0 114 0v1a1 1 0 001 1h3a1 1 0 011 1v3a1 1 0 01-1 1h-1a2 2 0 100 4h1a1 1 0 011 1v3a1 1 0 01-1 1h-3a1 1 0 01-1-1v-1a2 2 0 10-4 0v1a1 1 0 01-1 1H7a1 1 0 01-1-1v-3a1 1 0 00-1-1H4a2 2 0 110-4h1a1 1 0 001-1V7a1 1 0 011-1h3a1 1 0 001-1V4z',
          route: '/app/maintenance',
        },
        {
          label: 'Reservas',
          icon: 'M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z',
          route: '/app/reservations',
        },
        {
          label: 'Rutinas',
          icon: 'M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4',
          route: '/app/workout-logs',
        },
      ],
    },
  ];

  /** Devuelve true si algún hijo del grupo está activo en el router actual */
  isGroupActive(group: NavGroup): boolean {
    const url = this.router.url;
    return group.items.some((item) => url.startsWith(item.route));
  }

  toggleGroup(label: string): void {
    this.openGroup.update((current) => (current === label ? null : label));
  }

  /** Cierra el dropdown al hacer clic fuera del navbar */
  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    const navbar = document.querySelector('app-navbar');
    if (navbar && !navbar.contains(event.target as Node)) {
      this.openGroup.set(null);
    }
  }

  getRoleLabel(role: UserRole): string {
    return ROLE_LABELS[role] ?? 'Usuario';
  }

  toggleMobileMenu(): void {
    this.mobileMenuOpen.update((v) => !v);
  }

  closeMobileMenu(): void {
    this.mobileMenuOpen.set(false);
    this.openGroup.set(null);
  }

  logout(): void {
    this.closeMobileMenu();
    this.auth.logout();
  }
}
