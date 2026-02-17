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
import { AboutPageComponent } from './features/static/pages/about/about-page.component';
import { ContactsPageComponent } from './features/static/pages/contacts/contacts-page.component';
import { PrivacyPolicyPageComponent } from './features/static/pages/privacy-policy/privacy-policy-page.component';
import { TermsPageComponent } from './features/static/pages/terms/terms-page.component';

export const routes: Routes = [
  {
    path: '',
    component: HomePageComponent,
    data: {
      title: 'Airsoft ID — Социальная платформа для страйкболистов',
      description:
        'Airsoft ID помогает страйкболистам находить игроков, команды и мероприятия в одном месте.'
    }
  },
  {
    path: 'about',
    component: AboutPageComponent,
    data: {
      title: 'О проекте Airsoft ID',
      description: 'Узнайте о миссии и возможностях платформы Airsoft ID для страйкбольного сообщества.'
    }
  },
  {
    path: 'contacts',
    component: ContactsPageComponent,
    data: {
      title: 'Контакты Airsoft ID',
      description: 'Свяжитесь с командой Airsoft ID по вопросам сотрудничества, поддержки и предложений.'
    }
  },
  {
    path: 'privacy-policy',
    component: PrivacyPolicyPageComponent,
    data: {
      title: 'Политика конфиденциальности — Airsoft ID',
      description: 'Информация о том, как Airsoft ID обрабатывает и защищает персональные данные пользователей.'
    }
  },
  {
    path: 'terms',
    component: TermsPageComponent,
    data: {
      title: 'Пользовательское соглашение — Airsoft ID',
      description: 'Правила использования сервиса Airsoft ID и условия взаимодействия с платформой.'
    }
  },
  {
    path: 'login',
    component: LoginPageComponent,
    canActivate: [guestGuard],
    data: {
      title: 'Вход — Airsoft ID',
      description: 'Авторизуйтесь в Airsoft ID для доступа к профилю и возможностям сообщества.'
    }
  },
  {
    path: 'register',
    component: RegisterPageComponent,
    canActivate: [guestGuard],
    data: {
      title: 'Регистрация — Airsoft ID',
      description: 'Создайте аккаунт Airsoft ID, чтобы участвовать в страйкбольном сообществе.'
    }
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
    component: AirsoftersListPageComponent,
    data: {
      title: 'Игроки страйкбола — Airsoft ID',
      description: 'Список профилей страйкболистов на платформе Airsoft ID.'
    }
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
