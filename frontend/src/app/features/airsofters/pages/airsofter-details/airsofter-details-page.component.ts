import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { finalize, forkJoin } from 'rxjs';
import { Airsofter, AirsofterGameHistoryItem, AirsofterViolation } from '../../data/airsofters.models';
import { AirsoftersService } from '../../data/airsofters.service';

type AirsofterTab = 'profile' | 'history' | 'violations';

@Component({
  selector: 'app-airsofter-details-page',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './airsofter-details-page.component.html',
  styleUrls: ['./airsofter-details-page.component.scss']
})
export class AirsofterDetailsPageComponent implements OnInit {
  readonly tabs: { key: AirsofterTab; label: string }[] = [
    { key: 'profile', label: 'Профиль' },
    { key: 'history', label: 'История игр' },
    { key: 'violations', label: 'Нарушения' }
  ];

  activeTab: AirsofterTab = 'profile';
  airsofterId = '';

  airsofter: Airsofter | null = null;
  gameHistory: AirsofterGameHistoryItem[] = [];
  violations: AirsofterViolation[] = [];

  isLoading = false;
  notFound = false;
  errorMessage: string | null = null;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly airsoftersService: AirsoftersService
  ) {}

  ngOnInit(): void {
    this.airsofterId = this.route.snapshot.paramMap.get('id') ?? '';
    this.loadAirsofterDetails();
  }

  setActiveTab(tab: AirsofterTab): void {
    this.activeTab = tab;
  }

  private loadAirsofterDetails(): void {
    if (!this.airsofterId) {
      this.notFound = true;
      return;
    }

    this.isLoading = true;
    this.notFound = false;
    this.errorMessage = null;

    forkJoin({
      airsofter: this.airsoftersService.getAirsofterById(this.airsofterId),
      gameHistory: this.airsoftersService.getAirsofterGameHistory(this.airsofterId),
      violations: this.airsoftersService.getAirsofterViolations(this.airsofterId)
    })
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        next: ({ airsofter, gameHistory, violations }) => {
          this.airsofter = airsofter;
          this.gameHistory = gameHistory;
          this.violations = violations;
        },
        error: (error: HttpErrorResponse) => {
          if (error.status === 404) {
            this.notFound = true;
            this.errorMessage = null;
            return;
          }

          this.errorMessage = 'Не удалось загрузить данные страйкболиста. Попробуйте позже.';
        }
      });
  }
}
