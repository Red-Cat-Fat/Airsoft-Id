# Фаза 4: Масштабирование

## Цель

Расширить платформу на мобильные устройства, добавить администрирование, платёжную систему для билетов на игры, и социальное взаимодействие через чат/мессенджер.

**Техдолг в этой фазе:** Оптимизация производительности (lazy loading изображений, кеширование Redis, CDN для статики).

---

## Задача 4.1: Мобильное приложение (Ionic + Capacitor)

### Контекст
Angular-приложение уже написано. Ionic позволяет переиспользовать ~80% кода для мобильных приложений. Capacitor — мост к нативным API (камера, геолокация, push-уведомления).

### Что нужно сделать

1. Создать отдельный проект `mobile/` в корне репозитория (монорепа):
   ```
   mobile/
   ├── package.json                # Зависимости Ionic + Capacitor
   ├── angular.json                # Конфигурация Angular для Ionic
   ├── src/
   │   ├── app/                    # Реэкспорт компонентов из frontend/
   │   │   ├── app.component.ts
   │   │   ├── app.routes.ts       # Мобильные маршруты (адаптированные)
   │   │   ├── pages/              # Мобильные страницы (оборачивают frontend-компоненты в Ionic)
   │   │   └── theme/              # Кастомные CSS-переменные Ionic
   │   ├── assets/                 # Splash screen, иконки
   │   └── environments/           # Окружения
   ├── android/                    # Capacitor Android (генерируется)
   └── ios/                        # Capacitor iOS (генерируется)
   ```

2. Стратегия переиспользования:
   - Создать библиотеку (Angular library) `shared-lib` в монорепе или настроить path alias
   - `frontend/src/app/core/` и `frontend/src/app/shared/` переиспользовать как есть
   - `frontend/src/app/features/` обернуть в Ionic-страницы
   - Адаптировать навигацию: таб-бар вместо хедера, swipe-жесты

3. Нативные возможности:
   - **Камера:** при создании/редактировании профиля, команды, поля — возможность сфотографировать
   - **Геолокация:** определение ближайших полей через `GetFieldsNearbyQuery`
   - **Push-уведомления:** Firebase Cloud Messaging + Capacitor Push Notifications
   - **Оффлайн:** кеширование через Service Worker (использовать `@angular/pwa`)

4. Темизация:
   - Ionic поддерживает CSS-переменные — перенести текущую тему (Desert Tan, Sand-Tactical, Amber Accent) в `mobile/src/app/theme/variables.scss`

5. Сборка и деплой:
   - Скрипты в `launcher/` для сборки мобильного приложения:
     - `bash launcher/build-mobile.sh` — сборка Android (.apk) и iOS (.ipa)
   - Интеграция с App Store и Google Play (инструкция)

6. CI/CD:
   - Добавить job в GitHub Actions для сборки мобильного приложения
   - Использовать Fastlane для автоматической публикации в stores

### Критерии завершения
- [ ] Проект `mobile/` создан и подключён к монорепе
- [ ] Основные страницы (игры, поля, игроки, профиль) работают в Ionic
- [ ] Камера и геолокация работают на мобильных устройствах
- [ ] Push-уведомления приходят на мобильное устройство
- [ ] Тема (светлая/тёмная) корректно отображается
- [ ] Сборка .apk/.ipa проходит

---

## Задача 4.2: Админ-панель

### Контекст
Сейчас нет интерфейса для администраторов. Все админ-функции доступны только через API. Нужно создать UI для управления пользователями, контентом и модерации.

### Что нужно сделать

1. Создать `frontend/src/app/features/admin/`:
   ```
   pages/
   ├── admin-dashboard/           # Общая статистика (количество игроков, команд, игр, полей)
   ├── users-list/               # Список пользователей (поиск, фильтр по роли, блокировка)
   ├── user-details/             # Детали пользователя + смена роли + история
   ├── pending-organizers/       # Заявки на роль организатора (из Фазы 2b.5)
   ├── content-moderation/       # Модерация контента (нарушения, жалобы)
   ├── fields-management/        # Управление полигонами (подтверждение, удаление)
   └── system-settings/          # Системные настройки (параметры, лимиты, баннеры)
   shared/
   ├── admin-sidebar/            # Левая панель навигации
   └── admin-table/              # Переиспользуемая таблица с сортировкой и фильтрацией
   ```

2. Маршруты:
   - `/admin` → admin-dashboard (authGuard + роль Administrator)
   - `/admin/users` → users-list
   - `/admin/users/:id` → user-details
   - `/admin/organizers` → pending-organizers
   - `/admin/moderation` → content-moderation
   - `/admin/fields` → fields-management
   - `/admin/settings` → system-settings

3. Backend — создать `Controllers/AdminController.cs`:
   ```
   GET    /api/admin/stats                → Статистика (количество сущностей)
   GET    /api/admin/users                 → Список пользователей (пагинированный)
   GET    /api/admin/users/{id}            → Детали пользователя
   PATCH  /api/admin/users/{id}/role       → Смена роли
   PATCH  /api/admin/users/{id}/ban        → Блокировка пользователя
   GET    /api/admin/reports               → Список жалоб/нарушений
   PATCH  /api/admin/reports/{id}/resolve  → Решить жалобу
   ```

4. Все админ-эндпоинты защищены ролью `Administrator`.

### Критерии завершения
- [ ] Админ-панель доступна по `/admin` для пользователей с ролью Administrator
- [ ] CRUD пользователей, смена ролей, блокировка работают
- [ ] Модерация контента работает
- [ ] Статистика на dashboard отображается
- [ ] UI соответствует общей тематике (Desert Tan цвета)

---

## Задача 4.3: Платёжная система и билеты на игры

### Контекст
Страйкбол — платное развлечение. Организаторы хотят продавать билеты на игры через платформу. Нужно интегрировать платёжный шлюз.

### Backend — что нужно сделать

1. Создать сущности в `Domain/Entities/`:
   ```csharp
   public class Ticket
   {
       public Guid Id { get; set; }
       public Guid GameId { get; set; }
       public Game Game { get; set; }
       public Guid? AirsofterId { get; set; }
       public Airsofter? Airsofter { get; set; }
       public decimal Price { get; set; }
       public TicketStatus Status { get; set; } // Reserved, Paid, Cancelled, Refunded
       public string? PaymentReference { get; set; } // ID транзакции в платёжной системе
       public DateTime CreatedAt { get; set; }
       public DateTime? PaidAt { get; set; }
   }

   public enum TicketStatus { Reserved, Paid, Cancelled, Refunded }
   ```

2. Создать CQRS в `Application/Features/Tickets/`:
   ```
   Commands/
   ├── ReserveTicketCommand.cs / Handler       # Зарезервировать билет (заблокировать место)
   ├── ConfirmPaymentCommand.cs / Handler      # Подтвердить оплату (webhook от платёжки)
   ├── CancelTicketCommand.cs / Handler        # Отменить билет
   └── RefundTicketCommand.cs / Handler        # Возврат (админ)
   Queries/
   ├── GetGameTicketsQuery.cs / Handler        # Билеты на конкретную игру
   └── GetMyTicketsQuery.cs / Handler          # Мои билеты
   ```

3. Интеграция с платёжным шлюзом:
   - Создать интерфейс `IPaymentGateway` в Application
   - Реализовать первый провайдер (например, Stripe):
     - `StripePaymentGateway.cs` в Infrastructure
     - Webhook-эндпоинт `POST /api/payments/webhook` для подтверждения
   - Stripe Checkout Session при оплате, Webhook для подтверждения

4. Обновить `Game`:
   - Добавить `decimal? TicketPrice` — если null, игра бесплатная
   - Добавить `bool RequiresPayment` — если true, регистрация через билеты

5. Создать `Controllers/TicketsController.cs`:
   ```
   POST   /api/games/{id}/tickets/reserve    → ReserveTicketCommand
   POST   /api/payments/webhook              → ConfirmPaymentCommand (без авторизации, проверка signature)
   GET    /api/tickets/my                    → GetMyTicketsQuery
   GET    /api/games/{id}/tickets             → GetGameTicketsQuery (организатор/админ)
   POST   /api/tickets/{id}/cancel           → CancelTicketCommand
   ```

### Frontend — что нужно сделать

1. Обновить `game-details`:
   - Если `requiresPayment` — показывать цену билета и кнопку "Купить билет"
   - После нажатия — редирект на Stripe Checkout (или модальное окно с оплатой)

2. Создать страницу "Мои билеты" `/tickets/my`:
   - Список билетов с QR-кодом (для сканирования на входе)
   - Статус: Reserved, Paid, Cancelled, Refunded

3. Обновить админ-панель:
   - Организатор видит список оплаченных билетов на игру
   - Возможность отметить участника как "прибыл" (check-in) — уже есть в GameParticipants

### Критерии завершения
- [ ] Модель Ticket создана в Domain и БД
- [ ] Платежи через Stripe работают (тестовый режим)
- [ ] Webhook обрабатывает подтверждение оплаты
- [ ] Страница "Мои билеты" с QR-кодом работает
- [ ] Организатор видит оплаченные билеты на игру

---

## Задача 4.4: Чат/мессенджер

### Контекст
Игрокам нужно общаться: командам — внутренний чат, организаторам — связь с участниками, всем — личные сообщения.

### Backend — что нужно сделать

1. Выбрать архитектуру:
   - **Вариант A:** SignalR + PostgreSQL (сложнее, но без внешних зависимостей)
   - **Вариант B:** Firebase Realtime Database / Firestore (проще, но внешняя зависимость)
   - **Рекомендация:** SignalR + PostgreSQL для сообщений, Redis Pub/Sub для real-time

2. Создать сущности в `Domain/Entities/`:
   ```csharp
   public class ChatMessage
   {
       public Guid Id { get; set; }
       public Guid ChatRoomId { get; set; }
       public Guid SenderId { get; set; }
       public string Content { get; set; }
       public DateTime SentAt { get; set; }
       public DateTime? EditedAt { get; set; }
   }

   public class ChatRoom
   {
       public Guid Id { get; set; }
       public ChatRoomType Type { get; set; } // Direct, Team, Game
       public string? Name { get; set; } // Для Team/Game — название
       public ICollection<ChatRoomMember> Members { get; set; }
   }

   public class ChatRoomMember
   {
       public Guid ChatRoomId { get; set; }
       public Guid AirsofterId { get; set; }
   }

   public enum ChatRoomType { Direct, Team, Game }
   ```

3. Создать хаб `Hubs/ChatHub.cs`:
   ```csharp
   [Authorize]
   public class ChatHub : Hub
   {
       public async Task JoinRoom(string roomId) { /* ... */ }
       public async Task LeaveRoom(string roomId) { /* ... */ }
       public async Task SendMessage(string roomId, string content) { /* ... */ }
       public async Task EditMessage(string messageId, string newContent) { /* ... */ }
   }
   ```

4. Создать CQRS `Application/Features/Chat/`:
   ```
   Commands/
   ├── SendMessageCommand.cs / Handler
   ├── EditMessageCommand.cs / Handler
   └── CreateChatRoomCommand.cs / Handler
   Queries/
   ├── GetChatRoomsQuery.cs / Handler          # Мои чаты
   ├── GetChatMessagesQuery.cs / Handler        # Сообщения в чате (пагинация)
   └── GetUnreadCountQuery.cs / Handler         # Непрочитанные сообщения
   ```

5. Интеграция:
   - При создании команды — автоматически создаётся Team chat room
   - При регистрации на игру — Game chat room (опционально)
   - Личные сообщения — по запросу

### Frontend — что нужно сделать

1. Создать `src/app/features/chat/`:
   ```
   pages/
   ├── chat-list/              # Список чатов (с предпросмотром последнего сообщения)
   └── chat-room/              # Комната чата (список сообщений, поле ввода)
   shared/
   └── chat-message/           # Компонент одного сообщения (с аватаром, временем)
   ```

2. UI:
   - Иконка чата в хедере с счётчиком непрочитанных
   - Chat-list: аватар/название комнаты, последнее сообщение, время, бейдж непрочитанных
   - Chat-room: сообщения, индикатор печатания (опционально), отправка по Enter
   - Время: относительное (5 мин назад, вчера, 2 дня назад)

3. SignalR integration:
   - Отдельный `chat.signalr.service.ts`
   - Автоматически подключаться к комнатам при открытии

### Критерии завершения
- [ ] ChatMessage, ChatRoom, ChatRoomMember сущности созданы и в БД
- [ ] ChatHub работает (SignalR)
- [ ] Отправка и получение сообщений в реальном времени
- [ ] Список чатов с непрочитанными работает
- [ ] Автоматические комнаты для команд и игр

---

## Задача 4.5: Техдолг — производительность и кеширование

### Контекст
С ростом функционала производительность становится критичной. Нужно оптимизировать ключевые узлы.

### Что нужно сделать

1. **Redis-кеширование:**
   - Кешировать списки: `GetAirsoftersListQuery`, `GetTeamsListQuery`, `GetFieldsListQuery`
   - Инвалидация кеша при создании/обновлении/удалении сущностей
   - Использовать `IDistributedCache` (встроенная абстракция .NET)
   - TTL: 5 минут для списков, 1 час для статичных данных (полигоны)

2. **CDN для статики:**
   - Настроить Nginx или CloudFront для аватаров, логотипов команд
   - Хранить загруженные изображения в S3-совместимом хранилище (MinIO для self-hosted или DigitalOcean Spaces)

3. **Lazy loading на фронтенде:**
   - Все feature pages уже должны быть ленивыми (через `loadComponent`)
   - Добавить `@defer` для блоков, которые не в viewport (карта, список участников)
   - Оптимизировать изображения: WebP формат, responsive sizes

4. **API оптимизация:**
   - Добавить `AsNoTracking()` для read-only запросов EF Core
   - Добавить `response caching` атрибуты на GET-эндпоинты
   - Добавить ETag / If-None-Match для кеширования на стороне клиента
   - Использовать `Select` проекции вместо загрузки целых сущностей

5. **Database:**
   - Проверить планы запросов (EXPLAIN ANALYZE) для ключевых запросов
   - Добавить индексы для часто фильтруемых полей (Callsign, FullName, Game.Date, etc.)
   - Настроить `Npgsql.EntityFrameworkCore.PostgreSQL` для работы с JSONB

### Критерии завершения
- [ ] Redis кеш работает для списков (проверить уменьшение времени ответа)
- [ ] Изображения загружаются через CDN
- [ ] Lazy loading внедрён на фронтенде
- [ ] `AsNoTracking()` используется во всех read-only запросах
- [ ] EXPLAIN ANALYZE показывает Index Only Scan для ключевых запросов

---

## Зависимости

- Задача 4.1 (мобильное приложение) — зависит от завершения фронтенда (Фаза 2b + 3)
- Задача 4.2 (админ-панель) — зависит от Фазы 2b (нужны все API)
- Задача 4.3 (платежи) — зависит от Games API (Фаза 2b.3)
- Задача 4.4 (чат) — зависит от SignalR (Фаза 3.2) и авторизации
- Задача 4.5 (производительность) — может выполняться параллельно

## Критерии завершения фазы

- [ ] Мобильное приложение для Android и iOS собрано и работает
- [ ] Админ-панель с полным функционалом готова
- [ ] Платежи через Stripe работают (тестовый режим)
- [ ] Чат работает (личные, командные, игровые комнаты)
- [ ] Производительность оптимизирована (Redis кеш, CDN, индексы)
