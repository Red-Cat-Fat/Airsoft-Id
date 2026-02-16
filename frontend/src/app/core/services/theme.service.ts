import { Injectable, Renderer2, RendererFactory2, inject, signal } from '@angular/core';
import { DOCUMENT } from '@angular/common';

export type ThemeMode = 'light' | 'dark';

@Injectable({
  providedIn: 'root'
})
export class ThemeService {
  private readonly THEME_KEY = 'theme_mode';
  private readonly renderer: Renderer2;
  private readonly document = inject(DOCUMENT);

  readonly theme = signal<ThemeMode>(this.getInitialTheme());

  constructor(rendererFactory: RendererFactory2) {
    this.renderer = rendererFactory.createRenderer(null, null);
    this.applyTheme(this.theme());
  }

  toggleTheme(): void {
    const nextTheme: ThemeMode = this.theme() === 'light' ? 'dark' : 'light';
    this.setTheme(nextTheme);
  }

  setTheme(theme: ThemeMode): void {
    this.theme.set(theme);
    this.applyTheme(theme);
    localStorage.setItem(this.THEME_KEY, theme);
  }

  private getInitialTheme(): ThemeMode {
    const storedTheme = localStorage.getItem(this.THEME_KEY);
    return storedTheme === 'dark' ? 'dark' : 'light';
  }

  private applyTheme(theme: ThemeMode): void {
    const body = this.document.body;
    this.renderer.removeClass(body, 'theme-light');
    this.renderer.removeClass(body, 'theme-dark');
    this.renderer.addClass(body, theme === 'dark' ? 'theme-dark' : 'theme-light');
  }
}
