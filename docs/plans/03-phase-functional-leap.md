# Фаза 3: Функциональный рывок

## Цель

Добавить ключевые пользовательские фичи: интерактивную карту полигонов и игр, систему real-time уведомлений, полнотекстовый поиск, интернационализацию, обновить Angular до 19 и .NET до 9.

**Техдолг в этой фазе:** Переписать стейт-менеджмент на Angular Signals, обновить все устаревшие зависимости, исправить `lang="en"` в `index.html`.

---

## Задача 3.1: Интерактивная карта (Leaflet)

### Контекст
Leaflet уже есть в зависимостях (`^1.9.4`), но не используется. Полигоны имеют координаты (Latitude/Longitude) в БД. Нужно создать полноценный картографический интерфейс.

### Что нужно сделать

1. Создать `src/app/shared/map/` — переиспользуемый компонент карты:
   ```
   map/
   ├── map.component.ts            # Generic карта с маркерами
   ├── map.component.html
   └── map.component.scss
   ```

2. Компонент `map.component.ts` (standalone):
   - Input: `markers: MapMarker[]`, `center: [lat, lng]`, `zoom: number`, `clickable: boolean`
   - Output: `coordinatesChanged: EventEmitter<{lat: number, lng: number}>` — когда кликнули
   - Output: `markerClicked: EventEmitter<MapMarker>`
   - Интерфейс `MapMarker`: `{ id: string, lat: number, lng: number, title: string, popupContent?: string, icon?: string }`
   - Инициализация карты через `L.map()`, тайлы OpenStreetMap
   - Поддержка кластеризации маркеров (добавить `leaflet.markercluster`)
   - Адаптация под тёмную тему (использовать dark-тайлы OpenStreetMap при `.theme-dark`)

3. Где использовать:
   - `fields-map` — все полигоны с кластеризацией, клик по маркеру → попап с названием, ссылка на страницу полигона
   - `field-create` — карта в режиме выбора координат (clickable = true)
   - `field-details` — карта с одним маркером
   - `upcoming-games` — карта с маркерами игр (с датой, статусом в попапе)

4. Создать сервис `src/app/core/services/map-config.service.ts`:
   - Предоставляет конфигурацию тайлов (светлая/тёмная тема)
   - `getTileUrl(theme: 'light' | 'dark'): string`
   - Подписывается на `themeService.currentTheme`

5. Проверить, что на мобильных устройствах карта работает корректно (touch-события, reflow).

### Критерии завершения
- [ ] `map.component` создан и переиспользуется в 3+ местах
- [ ] Кластеризация маркеров работает при большом количестве точек
- [ ] Тёмная тема применяется к карте
- [ ] Выбор координат кликом работает на странице создания полигона

---

## Задача 3.2: Real-time уведомления (SignalR)

### Контекст
Пользователи должны получать уведомления в реальном времени: приглашение в команду, подтверждение регистрации на игру, новое нарушение. Используем SignalR (встроен в ASP.NET Core).

### Backend — что нужно сделать

1. Создать хаб `WebApi/Hubs/NotificationHub.cs`:
   ```csharp
   [Authorize]
   public class NotificationHub : Hub
   {
       public async Task JoinUserGroup(string userId)
       {
           await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
       }

       public async Task LeaveUserGroup(string userId)
       {
           await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
       }
   }
   ```

2. Создать `Application/Common/Interfaces/INotificationService.cs`:
   ```csharp
   public interface INotificationService
   {
       Task SendToUserAsync(Guid userId, NotificationDto notification, CancellationToken ct = default);
       Task SendToRoleAsync(string role, NotificationDto notification, CancellationToken ct = default);
   }
   ```

3. Реализовать в `Infrastructure/Services/SignalRNotificationService.cs`:
   - Через `IHubContext<NotificationHub>` отправлять сообщения в группу `user_{userId}`
   - Для ролей — в группу `role_{roleName}`

4. Создать `NotificationsController.cs`:
   ```
   GET /api/notifications          → список непрочитанных уведомлений (пагинированный)
   PATCH /api/notifications/{id}/read → отметить как прочитанное
   POST /api/notifications/read-all → отметить все как прочитанные
   ```

5. Создать `Features/Notifications/`:
   - Entity `Notification` в Domain (или хранить в Redis как временные, в PostgreSQL как постоянные)
   - CQRS-команды/запросы для CRUD уведомлений

6. Интегрировать отправку уведомлений в существующие CQRS-хендлеры:
   - `RegisterForGameCommandHandler` → уведомление организатору
   - `AddTeamMemberCommandHandler` → уведомление приглашённому
   - `RecordViolationCommandHandler` → уведомление игроку/команде

### Frontend — что нужно сделать

1. Создать `src/app/core/services/signalr.service.ts`:
   ```typescript
   @Injectable({ providedIn: 'root' })
   export class SignalrService {
     private hubConnection: signalR.HubConnection | null = null;

     connect(token: string): void {
       this.hubConnection = new signalR.HubConnectionBuilder()
         .withUrl('/hubs/notifications', { accessTokenFactory: () => token })
         .withAutomaticReconnect()
         .build();

       this.hubConnection.on('ReceiveNotification', (notification: Notification) => {
         // emit через сигнал
       });

       this.hubConnection.start()
         .then(() => this.hubConnection?.invoke('JoinUserGroup', this.userId));
     }

     disconnect(): void { /* ... */ }
   }
   ```

2. Создать `src/app/shared/notification-bell/`:
   - Иконка колокольчика в хедере
   - Счётчик непрочитанных (бадж с количеством)
   - Выпадающий список последних 5 уведомлений
   - Ссылка "Смотреть все" на страницу уведомлений

3. Создать `src/app/features/notifications/`:
   ```
   pages/
   └── notifications-list/     # Все уведомления (пагинация, фильтр по типу, прочитанные/нет)
   ```

4. Автоматически подключаться к SignalR после логина (в `auth.service.ts` или `app.component.ts`).

### Критерии завершения
- [ ] SignalR хаб работает
- [ ] Уведомления отправляются при регистрации на игру, добавлении в команду, нарушении
- [ ] NotificationBell в хедере показывает счётчик и выпадающий список
- [ ] Подключение к SignalR происходит автоматически после логина

---

## Задача 3.3: Полнотекстовый поиск

### Контекст
Пользователи должны искать игроков по Callsign/имени, команды по названию, полигоны по адресу/названию. Использовать полнотекстовый поиск PostgreSQL (tsvector).

### Backend — что нужно сделать

1. Создать `Application/Common/Models/SearchResult.cs`:
   ```csharp
   public class SearchResult
   {
       public string EntityType { get; set; } // "airsofter", "team", "field"
       public Guid Id { get; set; }
       public string Title { get; set; }
       public string? Subtitle { get; set; }
       public string? AvatarUrl { get; set; }
       public double Rank { get; set; }
   }
   ```

2. Создать `Application/Features/Search/Queries/SearchQuery.cs`:
   ```csharp
   public record SearchQuery(string Query, int MaxResults = 10) : IRequest<IReadOnlyList<SearchResult>>;
   ```

3. Реализовать `SearchQueryHandler.cs`:
   - Использовать `IApplicationDbContext`
   - Для Airsofters: поиск по `Callsign`, `FirstName`, `LastName` через `ILike` или `EF.Functions.FreeText`
   - Для Teams: поиск по `FullName`, `Abbreviation`
   - Для Fields: поиск по `Name`, `Address`
   - Объединить результаты, отсортировать по релевантности
   - Лимит на каждый тип сущности: 5

4. Создать `Controllers/SearchController.cs`:
   ```
   GET /api/search?q=text&maxResults=10  → SearchQuery
   ```

5. Добавить GIN-индексы на поисковые поля через миграцию EF Core:
   - `Airsofters.Callsign`, `Teams.FullName`, `Fields.Name`

### Frontend — что нужно сделать

1. Обновить `header.component.ts`:
   - Добавить поле поиска (глобальный поиск)
   - При вводе текста (с debounce 300мс) отправлять запрос к `/api/search`
   - Отображать выпадающий список с результатами, сгруппированными по типу (Игроки, Команды, Поля)
   - Клик по результату → переход на соответствующую страницу

2. Создать `src/app/core/services/search.service.ts`:
   ```typescript
   search(query: string, maxResults: number = 10): Observable<SearchResult[]>
   ```

3. UI:
   - Поле поиска с иконкой лупы
   - Выпадающий список с разделителями между типами результатов
   - Каждый результат: аватар/иконка + название + подпись
   - Анимация загрузки при запросе
   - Состояние "ничего не найдено"

### Критерии завершения
- [ ] `GET /api/search` работает и возвращает результаты по всем трём типам
- [ ] GIN-индексы добавлены миграцией
- [ ] Глобальный поиск в хедере работает с debounce
- [ ] Результаты группируются по типу

---

## Задача 3.4: Интернационализация (i18n)

### Контекст
Проект ориентирован на русскоязычных пользователей, но в перспективе может выйти на международный рынок. Нужно заложить i18n с самого начала.

### Что нужно сделать

1. Установить `@angular/localize`:
   ```bash
   ng add @angular/localize
   ```

2. Настроить `angular.json` для сборки с локалями (ru, en):
   ```json
   "i18n": {
     "sourceLocale": "ru",
     "locales": {
       "en": { "translation": "src/locale/messages.en.xlf" }
     }
   }
   ```

3. Создать файлы переводов:
   - `src/locale/messages.xlf` — исходный (русский)
   - `src/locale/messages.en.xlf` — английский перевод

4. Создать сервис `src/app/core/services/translation.service.ts`:
   - Хранит выбранный язык в `localStorage`
   - Предоставляет сигнал `currentLang: Signal<'ru' | 'en'>`
   - Переключает `document.documentElement.lang` при смене языка

5. Обновить маршрутизацию:
   - Поддержка префикса `/en/...` и `/ru/...`
   - Редирект с `/` на язык браузера или язык по умолчанию

6. Обновить компоненты:
   - Заменить все текстовые строки на `$localize` или `i18n` атрибуты в шаблонах
   - Особое внимание: статические страницы (about, contacts, privacy-policy, terms)

7. Добавить переключатель языка в `header.component.html` (флаг/кнопка).

### Критерии завершения
- [ ] `@angular/localize` установлен и настроен
- [ ] `ru` и `en` локали работают
- [ ] Переключатель языка в хедере переключает весь UI
- [ ] `document.documentElement.lang` обновляется при смене языка
- [ ] Статические страницы переведены

---

## Задача 3.5: Обновление Angular 17 → 19 и .NET 8 → 9

### Контекст
Технологии устаревают быстро. Angular 19 и .NET 9 содержат значительные улучшения производительности и новые возможности.

### Angular 17 → 19

1. Обновить Angular:
   ```bash
   ng update @angular/core @angular/cli @angular/material @angular/cdk
   ```

2. После обновления:
   - Перевести существующие сервисы на Signals (в первую очередь `auth.service.ts`, `theme.service.ts`)
   - Заменить `async` pipe на `@let` или сигналы в шаблонах
   - Внедрить `inject()` везде вместо constructor injection
   - Перейти на новую управляющую структуру `@if`/`@for`/`@defer` вместо `*ngIf`/`*ngFor`

3. Обновить TypeScript до версии, поддерживаемой Angular 19.

4. Проверить:
   - `ng build --configuration production` проходит
   - `ng test` проходит
   - Все страницы работают корректно

### .NET 8 → 9

1. Обновить target framework во всех `.csproj`:
   ```xml
   <TargetFramework>net9.0</TargetFramework>
   ```

2. Обновить все NuGet-пакеты до последних стабильных версий для .NET 9:
   - Microsoft.AspNetCore.Identity, EntityFrameworkCore, Npgsql, MediatR, etc.

3. Обновить `Dockerfile`:
   ```
   FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
   FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
   ```

4. Проверить:
   - `dotnet build` проходит
   - `dotnet test` проходит
   - API в Docker работает корректно

### Критерии завершения
- [ ] Angular 19, `.NET 9`, TypeScript последней совместимой версии
- [ ] Все зависимости обновлены до совместимых версий
- [ ] Сервисы переведены на Signals
- [ ] Новые шаблонные конструкции Angular 19 используются
- [ ] `ng build`, `dotnet build`, `dotnet test` проходят

---

## Задача 3.6: Техдолг — Signals, lang, недочеты

### Что нужно сделать

1. **Переписать сервисы на Signals:**
   - `authService`: `currentUser: Signal<UserInfo | null>`, `isAuthenticated: Signal<boolean>`
   - `themeService`: `currentTheme: Signal<'light' | 'dark'>`
   - Убрать `BehaviorSubject` и `Observable` там, где Signals являются заменой

2. **Исправить `index.html`:**
   - `lang="ru"` (сейчас `lang="en"`)

3. **Добавить `theme-dark` класс:**
   - В `main.ts` при инициализации приложения считывать тему из localStorage
   - Если сохранена тёмная тема, устанавливать класс `theme-dark` на body до первого рендера (чтобы избежать flash белого фона)

4. **Удалить неиспользуемый `features/players/`** (если не сделано в 2b.6).

### Критерии завершения
- [ ] AuthService и ThemeService используют Signals
- [ ] `index.html` имеет `lang="ru"`
- [ ] Тёмная тема применяется без flash белого фона
- [ ] Весь старый Observable-код в сервисах заменён на Signals

---

## Зависимости

- Задача 3.1 (карта) — может начаться после 2b.2 (Fields API)
- Задача 3.2 (уведомления) — зависит от 2b.1, 2b.3, 2b.4 (интеграция с Teams/Games/Violations)
- Задача 3.3 (поиск) — зависит от 2b.1, 2b.2, 2b.3 (нужен API Teams/Fields/Games)
- Задача 3.4 (i18n) — может выполняться параллельно с другими, но лучше после завершения UI
- Задача 3.5 (обновление версий) — лучше делать после завершения фич, чтобы не было конфликтов
- Задача 3.6 (техдолг) — может распределяться по всей фазе

## Критерии завершения фазы

- [ ] Интерактивная карта Leaflet работает на всех страницах
- [ ] Real-time уведомления приходят на ключевые события
- [ ] Глобальный поиск работает и выдаёт результаты
- [ ] i18n поддерживает ru и en
- [ ] Angular 19 и .NET 9 работают в development и production
- [ ] `ng build`, `dotnet build`, `dotnet test` проходят
