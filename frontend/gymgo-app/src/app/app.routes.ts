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
          import('./features/dashboard/dashboard').then((m) => m.DashboardComponent), // placeholder
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
