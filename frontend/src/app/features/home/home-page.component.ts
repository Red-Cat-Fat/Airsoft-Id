import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-home-page',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './home-page.component.html',
  styleUrls: ['./home-page.component.scss']
})
export class HomePageComponent {
  readonly advantages = [
    {
      title: 'Комьюнити страйкболистов',
      description: 'Объединяем игроков, команды и организаторов в одном месте.'
    },
    {
      title: 'Удобный поиск',
      description: 'Находите игроков, мероприятия и площадки по интересам и локации.'
    },
    {
      title: 'Прозрачный профиль',
      description: 'Следите за историей игр, достижениями и репутацией участников.'
    }
  ];

  readonly onboardingSteps = [
    'Создайте аккаунт и заполните профиль игрока.',
    'Подпишитесь на интересные команды и организаторов.',
    'Выбирайте игры и присоединяйтесь к ближайшим событиям.'
  ];
}
