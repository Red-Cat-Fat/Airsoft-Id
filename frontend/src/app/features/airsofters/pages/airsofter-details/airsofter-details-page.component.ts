import { Component } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';

@Component({
  selector: 'app-airsofter-details-page',
  standalone: true,
  imports: [RouterLink],
  template: `
    <main style="padding: 2rem; max-width: 960px; margin: 0 auto;">
      <h1>Профиль страйкболиста</h1>
      <p>Идентификатор: {{ airsofterId }}</p>
      <a routerLink="/airsofters">← К списку</a>
    </main>
  `
})
export class AirsofterDetailsPageComponent {
  airsofterId = this.route.snapshot.paramMap.get('id') ?? '';

  constructor(private readonly route: ActivatedRoute) {}
}
