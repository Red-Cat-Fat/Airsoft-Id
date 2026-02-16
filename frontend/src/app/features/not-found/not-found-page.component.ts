import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-not-found-page',
  standalone: true,
  imports: [RouterLink],
  template: `
    <main class="not-found">
      <h1>404</h1>
      <p>Страница не найдена.</p>
      <a routerLink="/">Вернуться на главную</a>
    </main>
  `,
  styles: [
    `
      .not-found {
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

      a {
        color: #1d4ed8;
      }
    `
  ]
})
export class NotFoundPageComponent {}
