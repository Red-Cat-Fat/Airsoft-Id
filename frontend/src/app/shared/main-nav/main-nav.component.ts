import { Component, inject } from '@angular/core';
import { NgIf } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-main-nav',
  standalone: true,
  imports: [NgIf, RouterLink, RouterLinkActive],
  templateUrl: './main-nav.component.html',
  styleUrls: ['./main-nav.component.scss']
})
export class MainNavComponent {
  protected readonly authService = inject(AuthService);

  logout(): void {
    this.authService.logout();
  }
}
