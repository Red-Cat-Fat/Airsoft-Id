import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-unauthorized-page',
  standalone: true,
  imports: [RouterLink],
  template: `
    <main class="error-page">
      <h1>401</h1>
      <p>Необходимо авторизоваться для доступа к этой странице.</p>
      <nav class="actions">
        <a routerLink="/" class="action-link">Вернуться на главную</a>
        <a routerLink="/login" class="action-link action-link--primary">Войти</a>
      </nav>
    </main>
  `,
  styles: [
    `
      .error-page {
        display: grid;
        gap: 1rem;
        justify-items: center;
        text-align: center;
        padding: 4rem 1rem;
      }

      h1 {
        margin: 0;
        font-size: 4rem;
      }

      .actions {
        display: flex;
        gap: 1rem;
      }

      .action-link {
        color: #1d4ed8;
      }

      .action-link--primary {
        font-weight: 600;
      }
    `
  ]
})
export class UnauthorizedPageComponent {}
