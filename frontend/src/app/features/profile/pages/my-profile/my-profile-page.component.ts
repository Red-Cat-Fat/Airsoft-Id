import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize, switchMap } from 'rxjs';
import { UserInfo } from '../../../../core/models/auth.models';
import { AuthService } from '../../../../core/services/auth.service';
import { Airsofter } from '../../../airsofters/data/airsofters.models';
import { AirsoftersService, UpdateAirsofterRequest } from '../../../airsofters/data/airsofters.service';

@Component({
  selector: 'app-my-profile-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './my-profile-page.component.html',
  styleUrl: './my-profile-page.component.scss'
})
export class MyProfilePageComponent implements OnInit {
  isLoading = false;
  isSaving = false;
  errorMessage: string | null = null;
  successMessage: string | null = null;

  currentUser: UserInfo | null = null;
  airsofter: Airsofter | null = null;

  readonly profileForm = this.fb.group({
    firstName: ['', [Validators.required]],
    lastName: ['', [Validators.required]],
    middleName: [''],
    kitDescription: [''],
    airsoftGunsList: ['']
  });

  constructor(
    private readonly fb: FormBuilder,
    private readonly authService: AuthService,
    private readonly airsoftersService: AirsoftersService
  ) {}

  ngOnInit(): void {
    this.loadProfile();
  }

  saveProfile(): void {
    if (!this.airsofter || this.profileForm.invalid || this.isSaving) {
      this.profileForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;
    this.successMessage = null;
    this.errorMessage = null;

    const formValue = this.profileForm.getRawValue();
    const payload: UpdateAirsofterRequest = {
      firstName: formValue.firstName?.trim() ?? '',
      lastName: formValue.lastName?.trim() ?? '',
      middleName: this.normalizeOptional(formValue.middleName),
      kitDescription: this.normalizeOptional(formValue.kitDescription),
      airsoftGunsList: this.normalizeOptional(formValue.airsoftGunsList),
      teamId: this.airsofter.teamId ?? null
    };

    this.airsoftersService
      .updateAirsofter(this.airsofter.id, payload)
      .pipe(finalize(() => (this.isSaving = false)))
      .subscribe({
        next: () => {
          this.successMessage = 'Профиль успешно обновлён.';
          this.loadProfile();
        },
        error: (error: HttpErrorResponse) => {
          this.errorMessage = this.getApiErrorMessage(error);
        }
      });
  }

  private loadProfile(): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.authService.getCurrentUser()
      .pipe(
        switchMap((user) => {
          this.currentUser = user;
          return this.airsoftersService.getAirsofterByCallsign(user.callsign);
        }),
        finalize(() => (this.isLoading = false))
      )
      .subscribe({
        next: (airsofter) => {
          this.airsofter = airsofter;
          this.profileForm.patchValue({
            firstName: airsofter.firstName,
            lastName: airsofter.lastName,
            middleName: airsofter.middleName ?? '',
            kitDescription: airsofter.kitDescription ?? '',
            airsoftGunsList: airsofter.airsoftGunsList ?? ''
          });
        },
        error: (error: HttpErrorResponse) => {
          this.errorMessage = this.getApiErrorMessage(error);
        }
      });
  }

  private normalizeOptional(value: string | null | undefined): string | null {
    const normalized = value?.trim();
    return normalized ? normalized : null;
  }

  private getApiErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 401) {
      return 'Требуется авторизация для просмотра профиля.';
    }

    if (error.status === 404) {
      return 'Профиль страйкболиста не найден.';
    }

    return 'Не удалось выполнить операцию. Попробуйте позже.';
  }
}
