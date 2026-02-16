import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';

type ThemeMode = 'light' | 'dark';

const passwordMatchValidator: ValidatorFn = (group: AbstractControl): ValidationErrors | null => {
  const password = group.get('password')?.value;
  const confirmPassword = group.get('confirmPassword')?.value;

  if (!password || !confirmPassword) {
    return null;
  }

  return password === confirmPassword ? null : { passwordMismatch: true };
};

@Component({
  selector: 'app-register-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './register-page.component.html',
  styleUrl: './register-page.component.scss'
})
export class RegisterPageComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly isSubmitting = signal(false);
  protected readonly apiError = signal<string | null>(null);
  protected readonly theme = signal<ThemeMode>('light');

  protected readonly registerForm = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    confirmPassword: ['', [Validators.required, Validators.minLength(6)]],
    firstName: ['', [Validators.required]],
    lastName: ['', [Validators.required]],
    middleName: [''],
    callsign: ['', [Validators.required]]
  }, { validators: passwordMatchValidator });

  protected readonly emailControl = this.registerForm.controls.email;
  protected readonly passwordControl = this.registerForm.controls.password;
  protected readonly confirmPasswordControl = this.registerForm.controls.confirmPassword;
  protected readonly firstNameControl = this.registerForm.controls.firstName;
  protected readonly lastNameControl = this.registerForm.controls.lastName;
  protected readonly callsignControl = this.registerForm.controls.callsign;

  protected readonly isFormInvalid = computed(
    () => this.registerForm.invalid || this.isSubmitting()
  );

  protected toggleTheme(): void {
    this.theme.set(this.theme() === 'light' ? 'dark' : 'light');
  }

  protected submit(): void {
    if (this.registerForm.invalid || this.isSubmitting()) {
      this.registerForm.markAllAsTouched();
      return;
    }

    const formValue = this.registerForm.getRawValue();
    if (!formValue.email || !formValue.password || !formValue.confirmPassword || !formValue.firstName || !formValue.lastName || !formValue.callsign) {
      return;
    }

    this.isSubmitting.set(true);
    this.apiError.set(null);

    this.authService.register({
      email: formValue.email,
      password: formValue.password,
      confirmPassword: formValue.confirmPassword,
      firstName: formValue.firstName,
      lastName: formValue.lastName,
      middleName: formValue.middleName || undefined,
      callsign: formValue.callsign
    }).subscribe({
      next: () => {
        this.router.navigateByUrl('/profile/me');
      },
      error: (error: HttpErrorResponse) => {
        this.apiError.set(this.getApiErrorMessage(error));
        this.isSubmitting.set(false);
      },
      complete: () => this.isSubmitting.set(false)
    });
  }

  private getApiErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 409) {
      return 'Пользователь с таким callsign или email уже существует.';
    }

    if (error.status === 400) {
      if (error.error?.errors) {
        const firstError = Object.values(error.error.errors).find((entry): entry is string[] => Array.isArray(entry) && entry.length > 0);
        if (firstError) {
          return firstError[0];
        }
      }

      const serverMessage = typeof error.error === 'string'
        ? error.error
        : error.error?.message || error.error?.title;

      return serverMessage || 'Проверьте корректность введённых данных.';
    }

    return 'Не удалось зарегистрироваться. Попробуйте снова позже.';
  }
}
