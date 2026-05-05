import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

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
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./features/dashboard/dashboard').then((m) => m.DashboardComponent),
      },
      {
        path: 'members',
        loadComponent: () =>
          import('./features/members/members-list/members-list').then((m) => m.MembersListComponent),
      },
      {
        path: 'members/new',
        loadComponent: () =>
          import('./features/members/member-create/member-create').then((m) => m.MemberCreateComponent),
      },
      {
        path: 'members/:id/edit',
        loadComponent: () =>
          import('./features/members/member-edit/member-edit').then((m) => m.MemberEditComponent),
      },
      {
        path: 'members/:id/assignments',
        loadComponent: () =>
          import('./features/members/member-assignments/member-assignments').then(
            (m) => m.MemberAssignmentsComponent,
          ),
      },
      {
        path: 'users',
        loadComponent: () =>
          import('./features/users/users-list/users-list').then((m) => m.UsersListComponent),
      },
      {
        path: 'users/new',
        loadComponent: () =>
          import('./features/users/user-create/user-create').then((m) => m.UserCreateComponent),
      },
      {
        path: 'users/:id/edit',
        loadComponent: () =>
          import('./features/users/user-edit/user-edit').then((m) => m.UserEditComponent),
      },
      {
        path: 'membership-plans',
        loadComponent: () =>
          import('./features/membership-plans/membership-plans-list/membership-plans-list').then(
            (m) => m.MembershipPlansListComponent,
          ),
      },
      {
        path: 'membership-plans/new',
        loadComponent: () =>
          import('./features/membership-plans/membership-plan-create/membership-plan-create').then(
            (m) => m.MembershipPlanCreateComponent,
          ),
      },
      {
        path: 'membership-plans/:id/edit',
        loadComponent: () =>
          import('./features/membership-plans/membership-plan-edit/membership-plan-edit').then(
            (m) => m.MembershipPlanEditComponent,
          ),
      },
      {
        path: 'assignments',
        loadComponent: () =>
          import('./features/assignments/assignments-list/assignments-list').then(
            (m) => m.AssignmentsListComponent,
          ),
      },
      {
        path: 'assignments/new',
        loadComponent: () =>
          import('./features/assignments/assignment-create/assignment-create').then(
            (m) => m.AssignmentCreateComponent,
          ),
      },
      // ── Registro de Ingreso ───────────────────────────────────────────────
      {
        path: 'gym-entry',
        loadComponent: () =>
          import('./features/gym-entry/gym-entry/gym-entry').then(
            (m) => m.GymEntryComponent,
          ),
      },
      // ── Reservas de Clases ────────────────────────────────────────────────
      {
        path: 'reservations',
        loadComponent: () =>
          import('./features/reservations/reservations/reservations').then(
            (m) => m.ReservationsComponent,
          ),
      },
      // ── Maquinaria & Mantención ────────────────────────────────────────
      {
        path: 'equipment',
        loadComponent: () =>
          import('./features/maintenance/equipment-list/equipment-list').then(
            (m) => m.EquipmentListComponent,
          ),
      },
      {
        path: 'equipment/new',
        loadComponent: () =>
          import('./features/maintenance/equipment-form/equipment-form').then(
            (m) => m.EquipmentFormComponent,
          ),
      },
      {
        path: 'equipment/:id/edit',
        loadComponent: () =>
          import('./features/maintenance/equipment-form/equipment-form').then(
            (m) => m.EquipmentFormComponent,
          ),
      },
      {
        path: 'maintenance',
        loadComponent: () =>
          import('./features/maintenance/maintenance-list/maintenance-list').then(
            (m) => m.MaintenanceListComponent,
          ),
      },
      {
        path: 'maintenance/new',
        loadComponent: () =>
          import('./features/maintenance/maintenance-form/maintenance-form').then(
            (m) => m.MaintenanceFormComponent,
          ),
      },
      {
        path: 'maintenance/:id',
        loadComponent: () =>
          import('./features/maintenance/maintenance-detail/maintenance-detail').then(
            (m) => m.MaintenanceDetailComponent,
          ),
      },
      // ── Classes & Calendar (Sprint 3) ──────────────────────────────────
      {
        path: 'classes',
        loadComponent: () =>
          import('./features/classes/classes-list/classes-list').then(
            (m) => m.ClassesListComponent,
          ),
      },
      {
        path: 'classes/new',
        loadComponent: () =>
          import('./features/classes/class-create/class-create').then(
            (m) => m.ClassCreateComponent,
          ),
      },
      {
        path: 'classes/calendar',
        loadComponent: () =>
          import('./features/classes/class-calendar/class-calendar').then(
            (m) => m.ClassCalendarComponent,
          ),
      },
      {
        path: 'classes/:id/edit',
        loadComponent: () =>
          import('./features/classes/class-edit/class-edit').then(
            (m) => m.ClassEditComponent,
          ),
      },
      // ── Rutinas de entrenamiento ───────────────────────────────────────
      {
        path: 'workout-logs',
        loadComponent: () =>
          import('./features/workout-logs/workout-logs-list/workout-logs-list').then(
            (m) => m.WorkoutLogsListComponent,
          ),
      },
      {
        path: 'workout-logs/new',
        loadComponent: () =>
          import('./features/workout-logs/workout-log-create/workout-log-create').then(
            (m) => m.WorkoutLogCreateComponent,
          ),
      },
      {
        path: 'workout-logs/:id',
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
