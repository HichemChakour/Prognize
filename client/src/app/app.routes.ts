import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth-guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login').then((m) => m.Login),
  },
  {
    path: 'register',
    loadComponent: () => import('./features/auth/register/register').then((m) => m.Register),
  },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('./shared/layout/shell/shell').then((m) => m.Shell),
    children: [
      {
        path: '',
        loadComponent: () => import('./features/dashboard/dashboard').then((m) => m.Dashboard),
      },
      {
        path: 'resources',
        loadComponent: () =>
          import('./features/resources/resource-list/resource-list').then((m) => m.ResourceList),
      },
      {
        path: 'resources/new',
        loadComponent: () =>
          import('./features/resources/resource-form/resource-form').then((m) => m.ResourceForm),
      },
      {
        path: 'resources/:id/edit',
        loadComponent: () =>
          import('./features/resources/resource-form/resource-form').then((m) => m.ResourceForm),
      },
      {
        path: 'activities',
        loadComponent: () =>
          import('./features/activities/activity-list/activity-list').then((m) => m.ActivityList),
      },
      {
        path: 'activities/new',
        loadComponent: () =>
          import('./features/activities/activity-form/activity-form').then((m) => m.ActivityForm),
      },
      {
        path: 'activities/:id/edit',
        loadComponent: () =>
          import('./features/activities/activity-form/activity-form').then((m) => m.ActivityForm),
      },
    ],
  },
  { path: '**', redirectTo: '' },
];
