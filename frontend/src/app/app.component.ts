import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs/operators';
import { Meta, Title } from '@angular/platform-browser';
import { HeaderComponent } from './shared/header/header.component';
import { FooterComponent } from './shared/footer/footer.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, HeaderComponent, FooterComponent],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  private readonly router = inject(Router);
  private readonly activatedRoute = inject(ActivatedRoute);
  private readonly titleService = inject(Title);
  private readonly meta = inject(Meta);

  ngOnInit(): void {
    this.router.events
      .pipe(filter((event): event is NavigationEnd => event instanceof NavigationEnd))
      .subscribe(() => this.updateSeoFromRoute());

    this.updateSeoFromRoute();
  }

  private updateSeoFromRoute(): void {
    let currentRoute = this.activatedRoute.firstChild;

    while (currentRoute?.firstChild) {
      currentRoute = currentRoute.firstChild;
    }

    const seoTitle = currentRoute?.snapshot.data['title'] as string | undefined;
    const seoDescription = currentRoute?.snapshot.data['description'] as string | undefined;

    this.titleService.setTitle(seoTitle ?? 'Airsoft ID');

    this.meta.updateTag({
      name: 'description',
      content:
        seoDescription ??
        'Airsoft ID — социальный сервис для страйкбольного сообщества: игроки, команды и организаторы в одном месте.'
    });
  }
}
