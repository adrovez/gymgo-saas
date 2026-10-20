import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';
import { UserRole } from './core/models/auth.models';

// ── Grupos de roles (espejo de las políticas del backend) ─────────────────────
const ADMIN_OR_OWNER      = [UserRole.PlatformAdmin, UserRole.GymOwner];
const STAFF_OR_ABOVE      = [UserRole.PlatformAdmin, UserRole.GymOwner, UserRole.GymStaff];
const INSTRUCTOR_OR_ABOVE = [UserRole.PlatformAdmin, UserRole.GymOwner, UserRole.GymStaff, UserRole.Instructor];

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/login/login').then((m) => m.LoginComponent),
  },
  {
    path: 'app',
    loadComponent: () =>
      import('./features/shell/shell').then((m) => m.ShellComponent),
    canActivate: [authGuard],
    children: [
      // ── Dashboard: cualquier usuario autenticado ──────────────────────────
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./features/dashboard/dashboard').then((m) => m.DashboardComponent),
      },

      // ── Socios: lista = Instructor+, escritura = Staff+ ───────────────────
      {
        path: 'members',
        canActivate: [roleGuard(INSTRUCTOR_OR_ABOVE)],
        loadComponent: () =>
          import('./features/members/members-list/members-list').then((m) => m.MembersListComponent),
      },
      {
        path: 'members/new',
        canActivate: [roleGuard(STAFF_OR_ABOVE)],
        loadComponent: () =>
          import('./features/members/member-create/member-create').then((m) => m.MemberCreateComponent),
      },
      {
        path: 'members/:id/edit',
        canActivate: [roleGuard(STAFF_OR_ABOVE)],
        loadComponent: () =>
          import('./features/members/member-edit/member-edit').then((m) => m.MemberEditComponent),
      },
      {
        path: 'members/:id/assignments',
        canActivate: [roleGuard(STAFF_OR_ABOVE)],
        loadComponent: () =>
          import('./features/members/member-assignments/member-assignments').then(
            (m) => m.MemberAssignmentsComponent,
          ),
      },

      // ── Usuarios: solo GymOwner y PlatformAdmin ───────────────────────────
      {
        path: 'users',
        canActivate: [roleGuard(ADMIN_OR_OWNER)],
        loadComponent: () =>
          import('./features/users/users-list/users-list').then((m) => m.UsersListComponent),
      },
      {
        path: 'users/new',
        canActivate: [roleGuard(ADMIN_OR_OWNER)],
        loadComponent: () =>
          import('./features/users/user-create/user-create').then((m) => m.UserCreateComponent),
      },
      {
        path: 'users/:id/edit',
        canActivate: [roleGuard(ADMIN_OR_OWNER)],
        loadComponent: () =>
          import('./features/users/user-edit/user-edit').then((m) => m.UserEditComponent),
      },

      // ── Planes de membresía: Staff+ ───────────────────────────────────────
      {
        path: 'membership-plans',
        canActivate: [roleGuard(STAFF_OR_ABOVE)],
        loadComponent: () =>
          import('./features/membership-plans/membership-plans-list/membership-plans-list').then(
            (m) => m.MembershipPlansListComponent,
          ),
      },
      {
        path: 'membership-plans/new',
        canActivate: [roleGuard(STAFF_OR_ABOVE)],
        loadComponent: () =>
          import('./features/membership-plans/membership-plan-create/membership-plan-create').then(
            (m) => m.MembershipPlanCreateComponent,
          ),
      },
      {
        path: 'membership-plans/:id/edit',
        canActivate: [roleGuard(STAFF_OR_ABOVE)],
        loadComponent: () =>
          import('./features/membership-plans/membership-plan-edit/membership-plan-edit').then(
            (m) => m.MembershipPlanEditComponent,
          ),
      },

      // ── Asignaciones de membresía: Staff+ ────────────────────────────────
      {
        path: 'assignments',
        canActivate: [roleGuard(STAFF_OR_ABOVE)],
        loadComponent: () =>
          import('./features/assignments/assignments-list/assignments-list').then(
            (m) => m.AssignmentsListComponent,
          ),
      },
      // ── Ingreso al gimnasio: Staff+ ───────────────────────────────────────
      {
        path: 'gym-entry',
        canActivate: [roleGuard(STAFF_OR_ABOVE)],
        loadComponent: () =>
          import('./features/gym-entry/gym-entry/gym-entry').then(
            (m) => m.GymEntryComponent,
          ),
      },

      // ── Reservas de clases: Instructor+ ──────────────────────────────────
      {
        path: 'reservations',
        canActivate: [roleGuard(INSTRUCTOR_OR_ABOVE)],
        loadComponent: () =>
          import('./features/reservations/reservations/reservations').then(
            (m) => m.ReservationsComponent,
          ),
      },

      // ── Maquinaria: Staff+ ────────────────────────────────────────────────
      {
        path: 'equipment',
        canActivate: [roleGuard(STAFF_OR_ABOVE)],
        loadComponent: () =>
          import('./features/maintenance/equipment-list/equipment-list').then(
            (m) => m.EquipmentListComponent,
          ),
      },
      {
        path: 'equipment/new',
        canActivate: [roleGuard(STAFF_OR_ABOVE)],
        loadComponent: () =>
          import('./features/maintenance/equipment-form/equipment-form').then(
            (m) => m.EquipmentFormComponent,
          ),
      },
      {
        path: 'equipment/:id/edit',
        canActivate: [roleGuard(STAFF_OR_ABOVE)],
        loadComponent: () =>
          import('./features/maintenance/equipment-form/equipment-form').then(
            (m) => m.EquipmentFormComponent,
          ),
      },

      // ── Mantención: Staff+ ────────────────────────────────────────────────
      {
        path: 'maintenance',
        canActivate: [roleGuard(STAFF_OR_ABOVE)],
        loadComponent: () =>
          import('./features/maintenance/maintenance-list/maintenance-list').then(
            (m) => m.MaintenanceListComponent,
          ),
      },
      {
        path: 'maintenance/new',
        canActivate: [roleGuard(STAFF_OR_ABOVE)],
        loadComponent: () =>
          import('./features/maintenance/maintenance-form/maintenance-form').then(
            (m) => m.MaintenanceFormComponent,
          ),
      },
      {
        path: 'maintenance/:id',
        canActivate: [roleGuard(STAFF_OR_ABOVE)],
        loadComponent: () =>
          import('./features/maintenance/maintenance-detail/maintenance-detail').then(
            (m) => m.MaintenanceDetailComponent,
          ),
      },

      // ── Clases: lista/calendario = Instructor+, escritura = Staff+ ────────
      {
        path: 'classes',
        canActivate: [roleGuard(INSTRUCTOR_OR_ABOVE)],
        loadComponent: () =>
          import('./features/classes/classes-list/classes-list').then(
            (m) => m.ClassesListComponent,
          ),
      },
      {
        path: 'classes/new',
        canActivate: [roleGuard(STAFF_OR_ABOVE)],
        loadComponent: () =>
          import('./features/classes/class-create/class-create').then(
            (m) => m.ClassCreateComponent,
          ),
      },
      {
        path: 'classes/calendar',
        canActivate: [roleGuard(INSTRUCTOR_OR_ABOVE)],
        loadComponent: () =>
          import('./features/classes/class-calendar/class-calendar').then(
            (m) => m.ClassCalendarComponent,
          ),
      },
      {
        path: 'classes/:id/edit',
        canActivate: [roleGuard(STAFF_OR_ABOVE)],
        loadComponent: () =>
          import('./features/classes/class-edit/class-edit').then(
            (m) => m.ClassEditComponent,
          ),
      },

      // ── Rutinas de entrenamiento: Instructor+ ─────────────────────────────
      {
        path: 'workout-plans',
        canActivate: [roleGuard(INSTRUCTOR_OR_ABOVE)],
        loadComponent: () =>
          import('./features/workout-plans/workout-plans-list/workout-plans-list').then(
            (m) => m.WorkoutPlansListComponent,
          ),
      },
      {
        path: 'workout-plans/new',
        canActivate: [roleGuard(INSTRUCTOR_OR_ABOVE)],
        loadComponent: () =>
          import('./features/workout-plans/workout-plan-create/workout-plan-create').then(
            (m) => m.WorkoutPlanCreateComponent,
          ),
      },
      {
        path: 'workout-plans/:id',
        canActivate: [roleGuard(INSTRUCTOR_OR_ABOVE)],
        loadComponent: () =>
          import('./features/workout-plans/workout-plan-detail/workout-plan-detail').then(
            (m) => m.WorkoutPlanDetailComponent,
          ),
      },

      // ── Ventas: Staff+ ───────────────────────────────────────────────────
      {
        path: 'sales',
        canActivate: [roleGuard(STAFF_OR_ABOVE)],
        loadComponent: () =>
          import('./features/sales/sales-list/sales-list').then((m) => m.SalesListComponent),
      },
      {
        path: 'sales/new',
        canActivate: [roleGuard(STAFF_OR_ABOVE)],
        loadComponent: () =>
          import('./features/sales/sale-create/sale-create').then((m) => m.SaleCreateComponent),
      },
      {
        path: 'sales/:id',
        canActivate: [roleGuard(STAFF_OR_ABOVE)],
        loadComponent: () =>
          import('./features/sales/sale-detail/sale-detail').then((m) => m.SaleDetailComponent),
      },

      // ── Caja: Staff+ ──────────────────────────────────────────────────────
      {
        path: 'cash',
        canActivate: [roleGuard(STAFF_OR_ABOVE)],
        loadComponent: () =>
          import('./features/cash/cash-list/cash-list').then((m) => m.CashListComponent),
      },
      {
        path: 'cash/new',
        canActivate: [roleGuard(STAFF_OR_ABOVE)],
        loadComponent: () =>
          import('./features/cash/cash-form/cash-form').then((m) => m.CashFormComponent),
      },

      // ── Historial de sesiones de entrenamiento: Instructor+ ───────────────
      {
        path: 'workout-logs',
        canActivate: [roleGuard(INSTRUCTOR_OR_ABOVE)],
        loadComponent: () =>
          import('./features/workout-logs/workout-logs-list/workout-logs-list').then(
            (m) => m.WorkoutLogsListComponent,
          ),
      },
      {
        path: 'workout-logs/new',
        canActivate: [roleGuard(INSTRUCTOR_OR_ABOVE)],
        loadComponent: () =>
          import('./features/workout-logs/workout-log-create/workout-log-create').then(
            (m) => m.WorkoutLogCreateComponent,
          ),
      },
      {
        path: 'workout-logs/:id',
        canActivate: [roleGuard(INSTRUCTOR_OR_ABOVE)],
        loadComponent: () =>
          import('./features/workout-logs/workout-log-detail/workout-log-detail').then(
            (m) => m.WorkoutLogDetailComponent,
          ),
      },

      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full',
      },
    ],
  },
  {
    path: '',
    redirectTo: 'app',
    pathMatch: 'full',
  },
  {
    path: '**',
    redirectTo: 'app',
  },
];
