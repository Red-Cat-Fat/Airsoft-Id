import { Routes } from '@angular/router';
import { HomePageComponent } from './features/home/home-page.component';
import { authGuard, guestGuard } from './core/guards/auth.guard';
import { LoginPageComponent } from './features/auth/pages/login/login-page.component';
import { RegisterPageComponent } from './features/auth/pages/register/register-page.component';
import { DashboardPageComponent } from './features/dashboard/dashboard-page.component';
import { MyProfilePageComponent } from './features/profile/pages/my-profile/my-profile-page.component';
import { AirsoftersListPageComponent } from './features/airsofters/pages/airsofters-list/airsofters-list-page.component';
import { AirsofterDetailsPageComponent } from './features/airsofters/pages/airsofter-details/airsofter-details-page.component';
import { NotFoundPageComponent } from './features/not-found/not-found-page.component';
import { UnauthorizedPageComponent } from './features/unauthorized/unauthorized-page.component';
import { ForbiddenPageComponent } from './features/forbidden/forbidden-page.component';

export const routes: Routes = [
  {
    path: '',
    component: HomePageComponent
  },
  {
    path: 'login',
    component: LoginPageComponent,
    canActivate: [guestGuard]
  },
  {
    path: 'register',
    component: RegisterPageComponent,
    canActivate: [guestGuard]
  },
  {
    path: 'auth/login',
    redirectTo: 'login',
    pathMatch: 'full'
  },
  {
    path: 'auth/register',
    redirectTo: 'register',
    pathMatch: 'full'
  },
  {
    path: 'unauthorized',
    component: UnauthorizedPageComponent
  },
  {
    path: 'forbidden',
    component: ForbiddenPageComponent
  },
  {
    path: 'dashboard',
    component: DashboardPageComponent,
    canActivate: [authGuard]
  },
  {
    path: 'profile/me',
    component: MyProfilePageComponent,
    canActivate: [authGuard]
  },
  {
    path: 'airsofters',
    component: AirsoftersListPageComponent
  },
  {
    path: 'airsofters/:id',
    component: AirsofterDetailsPageComponent
  },
  {
    path: 'players',
    redirectTo: 'airsofters',
    pathMatch: 'full'
  },
  {
    path: '**',
    component: NotFoundPageComponent
  }
];
