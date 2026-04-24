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
          import('./features/dashboard/dashboard').then((m) => m.DashboardComponent), // placeholder
      },
      {
        path: 'assignments',
        loadComponent: () =>
          import('./features/dashboard/dashboard').then((m) => m.DashboardComponent), // placeholder
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
