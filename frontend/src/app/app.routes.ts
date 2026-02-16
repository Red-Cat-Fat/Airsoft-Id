import { Routes } from '@angular/router';
import { HomePageComponent } from './features/home/home-page.component';
import { authGuard, guestGuard } from './core/guards/auth.guard';
import { LoginPageComponent } from './features/auth/pages/login/login-page.component';
import { RegisterPageComponent } from './features/auth/pages/register/register-page.component';
import { DashboardPageComponent } from './features/dashboard/dashboard-page.component';
import { ProfileMePageComponent } from './features/profile/pages/me/profile-me-page.component';

export const routes: Routes = [
  {
    path: '',
    component: HomePageComponent
  },
  {
    path: 'auth/login',
    component: LoginPageComponent,
    canActivate: [guestGuard]
  },
  {
    path: 'auth/register',
    component: RegisterPageComponent,
    canActivate: [guestGuard]
  },
  {
    path: 'dashboard',
    component: DashboardPageComponent,
    canActivate: [authGuard]
  },
  {
    path: 'profile/me',
    component: ProfileMePageComponent,
    canActivate: [authGuard]
  },
  {
    path: '**',
    redirectTo: ''
  }
];
