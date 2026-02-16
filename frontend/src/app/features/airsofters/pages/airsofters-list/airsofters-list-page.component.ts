import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { Airsofter } from '../../data/airsofters.models';
import { AirsoftersService } from '../../data/airsofters.service';

@Component({
  selector: 'app-airsofters-list-page',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './airsofters-list-page.component.html',
  styleUrls: ['./airsofters-list-page.component.scss']
})
export class AirsoftersListPageComponent implements OnInit {
  allAirsofters: Airsofter[] = [];
  filteredAirsofters: Airsofter[] = [];

  query = '';
  isLoading = false;
  errorMessage: string | null = null;

  constructor(private readonly airsoftersService: AirsoftersService) {}

  ngOnInit(): void {
    this.loadAirsofters();
  }

  onQueryChange(): void {
    const normalizedQuery = this.query.trim().toLowerCase();

    this.filteredAirsofters = normalizedQuery
      ? this.allAirsofters.filter((item) => item.callsign.toLowerCase().includes(normalizedQuery))
      : [...this.allAirsofters];
  }

  onSearch(): void {
    this.loadAirsofters(this.query);
  }

  private loadAirsofters(callsign?: string): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.airsoftersService
      .getAirsofters(callsign)
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        next: (data) => {
          this.allAirsofters = data;
          this.onQueryChange();
        },
        error: () => {
          this.errorMessage = 'Не удалось загрузить список страйкболистов. Попробуйте позже.';
          this.allAirsofters = [];
          this.filteredAirsofters = [];
        }
      });
  }
}
