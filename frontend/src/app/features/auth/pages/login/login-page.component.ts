import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';

type ThemeMode = 'light' | 'dark';

@Component({
  selector: 'app-login-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './login-page.component.html',
  styleUrl: './login-page.component.scss'
})
export class LoginPageComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly isSubmitting = signal(false);
  protected readonly apiError = signal<string | null>(null);
  protected readonly theme = signal<ThemeMode>('light');

  protected readonly loginForm = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });

  protected readonly emailControl = this.loginForm.controls.email;
  protected readonly passwordControl = this.loginForm.controls.password;

  protected readonly isFormInvalid = computed(
    () => this.loginForm.invalid || this.isSubmitting()
  );

  protected toggleTheme(): void {
    this.theme.set(this.theme() === 'light' ? 'dark' : 'light');
  }

  protected submit(): void {
    if (this.loginForm.invalid || this.isSubmitting()) {
      this.loginForm.markAllAsTouched();
      return;
    }

    const { email, password } = this.loginForm.getRawValue();
    if (!email || !password) {
      return;
    }

    this.isSubmitting.set(true);
    this.apiError.set(null);

    this.authService.login({ email, password }).subscribe({
      next: (response) => {
        const redirectUrl = response.user.roles.includes('Admin') ? '/dashboard' : '/profile/me';
        this.router.navigateByUrl(redirectUrl);
      },
      error: (error: HttpErrorResponse) => {
        this.apiError.set(this.getApiErrorMessage(error));
        this.isSubmitting.set(false);
      },
      complete: () => this.isSubmitting.set(false)
    });
  }

  private getApiErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 401) {
      return 'Неверный email или пароль.';
    }

    if (error.status === 400) {
      const serverMessage = typeof error.error === 'string'
        ? error.error
        : error.error?.message || error.error?.title;

      return serverMessage || 'Проверьте корректность введённых данных.';
    }

    return 'Не удалось выполнить вход. Попробуйте снова позже.';
  }
}
