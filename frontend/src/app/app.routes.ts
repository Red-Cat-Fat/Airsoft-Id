import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    redirectTo: '/home',
    pathMatch: 'full'
  },
  {
    path: 'home',
    loadComponent: () => import('./features/home/home.component').then(m => m.HomeComponent)
  },
  {
    path: 'auth',
    loadChildren: () => import('./features/auth/auth.routes').then(m => m.AUTH_ROUTES)
  },
  {
    path: 'games',
    loadChildren: () => import('./features/games/games.routes').then(m => m.GAMES_ROUTES)
  },
  {
    path: 'teams',
    loadChildren: () => import('./features/teams/teams.routes').then(m => m.TEAMS_ROUTES)
  },
  {
    path: 'fields',
    loadChildren: () => import('./features/fields/fields.routes').then(m => m.FIELDS_ROUTES)
  },
  {
    path: 'profile',
    loadChildren: () => import('./features/profile/profile.routes').then(m => m.PROFILE_ROUTES)
  },
  {
    path: '**',
    redirectTo: '/home'
  }
];
