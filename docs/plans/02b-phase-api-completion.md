# Фаза 2b: Завершение API (Teams, Games, Fields, Violations, Organizers)

## Цель

Реализовать CRUD API для всех сущностей домена, добавить соответствующие страницы на фронтенде. Фаза опирается на Application слой и CQRS из Фазы 2a.

**Техдолг в этой фазе:** Рефакторинг `frontend/src/app/features/` — удалить дублирующиеся компоненты `me/` и `players/`, привести маршруты к единому стилю.

---

## Задача 2b.1: API + Frontend для Teams

### Контекст
Сущность `Team` существует в Domain, но не имеет API. Команды — это ядро страйкбола: игроки объединяются в команды, участвуют в играх как команды, поля могут банить команды.

### Backend — что нужно сделать

1. Создать CQRS в `Application/Features/Teams/`:
   ```
   Commands/
   ├── CreateTeamCommand.cs / Handler
   ├── UpdateTeamCommand.cs / Handler
   ├── DeleteTeamCommand.cs / Handler
   ├── AddTeamMemberCommand.cs / Handler       # Добавить игрока в команду
   ├── RemoveTeamMemberCommand.cs / Handler    # Удалить игрока из команды
   └── ChangeTeamLeaderCommand.cs / Handler    # Сменить лидера команды
   Queries/
   ├── GetTeamsListQuery.cs / Handler          # Пагинированный список
   ├── GetTeamByIdQuery.cs / Handler           # Детали команды + участники
   └── GetTeamMembersQuery.cs / Handler        # Список участников
   Validators/
   ├── CreateTeamValidator.cs
   └── UpdateTeamValidator.cs
   DTOs/
   ├── TeamDto.cs
   ├── TeamDetailsDto.cs
   └── TeamMemberDto.cs
   ```

2. Создать `Controllers/TeamsController.cs` в WebApi:
   ```
   GET    /api/teams              → GetTeamsListQuery       (пагинация, поиск по названию)
   GET    /api/teams/{id}         → GetTeamByIdQuery        (детали + участники)
   POST   /api/teams              → CreateTeamCommand       (только TeamLeader+)
   PUT    /api/teams/{id}         → UpdateTeamCommand       (только TeamLeader команды)
   DELETE /api/teams/{id}         → DeleteTeamCommand       (только TeamLeader команды)
   POST   /api/teams/{id}/members → AddTeamMemberCommand    (только TeamLeader команды)
   DELETE /api/teams/{id}/members/{airsofterId} → RemoveTeamMemberCommand
   PUT    /api/teams/{id}/leader  → ChangeTeamLeaderCommand
   ```

3. Правила авторизации:
   - Создать команду может любой Airsofter
   - Редактировать/удалять команду может TeamLeader этой команды
   - Администратор может редактировать/удалять любую команду
   - При создании команды создатель становится TeamLeader
   - Один игрок может состоять только в одной команде одновременно

4. Добавить валидацию FluentValidation:
   - Название команды: 2-100 символов
   - Аббревиатура: 2-10 символов, только буквы
   - LogoUrl: опционально, валидный URL

5. Написать тесты (модульные):
   - `CreateTeamCommandTests.cs` — успешное создание, создание с коротким именем, создание не-Airsofter
   - `GetTeamsListQueryTests.cs` — пагинация, фильтрация
   - `AddTeamMemberCommandTests.cs` — добавление, добавление в полную команду, добавление игрока уже в команде

### Frontend — что нужно сделать

1. Создать `src/app/features/teams/`:
   ```
   pages/
   ├── teams-list/           # Список команд (карточки, поиск, пагинация)
   │   ├── teams-list.component.ts
   │   ├── teams-list.component.html
   │   └── teams-list.component.scss
   ├── team-details/         # Детали команды + список участников
   │   ├── team-details.component.ts
   │   ├── team-details.component.html
   │   └── team-details.component.scss
   └── team-create/          # Создание команды (форма)
       ├── team-create.component.ts
       ├── team-create.component.html
       └── team-create.component.scss
   data/
   ├── teams.service.ts      # HTTP-запросы к /api/teams
   └── teams.models.ts       # Интерфейсы Team, TeamDetails, TeamMember
   ```

2. В `app.routes.ts` добавить маршруты:
   - `/teams` → `teams-list` (ленивая загрузка)
   - `/teams/:id` → `team-details`
   - `/teams/create` → `team-create` (защищён authGuard)

3. Компоненты (standalone):
   - **teams-list**: отображает карточки команд (название, аббревиатура, логотип, количество участников с ролью TeamLeader). Поиск по названию. Кнопка "Создать команду" (только для авторизованных)
   - **team-details**: полная информация о команде, список участников с ролями, возможность покинуть/вступить
   - **team-create**: форма с полями FullName, Abbreviation, LogoUrl

4. Учесть тёмную тему: все компоненты должны корректно отображаться в `.theme-dark`.

### Критерии завершения
- [ ] Все CQRS команды/запросы для Teams реализованы
- [ ] TeamsController с 8 эндпоинтами работает
- [ ] Валидация FluentValidation настроена
- [ ] Написаны тесты для ключевых сценариев
- [ ] Страницы teams-list, team-details, team-create работают на фронтенде
- [ ] Маршруты добавлены и защищены

---

## Задача 2b.2: API + Frontend для Fields (Полигоны)

### Контекст
Сущность `Field` содержит координаты (Latitude/Longitude). Полигоны — места проведения игр. Нужно реализовать управление полигонами с привязкой к карте.

### Backend — что нужно сделать

1. Создать CQRS в `Application/Features/Fields/`:
   ```
   Commands/
   ├── CreateFieldCommand.cs / Handler
   ├── UpdateFieldCommand.cs / Handler
   └── DeleteFieldCommand.cs / Handler
   Queries/
   ├── GetFieldsListQuery.cs / Handler         # Пагинированный список + фильтр по городу
   ├── GetFieldByIdQuery.cs / Handler          # Детали полигона
   └── GetFieldsNearbyQuery.cs / Handler       # Полигоны рядом (по координатам + радиусу)
   Validators/
   ├── CreateFieldValidator.cs
   └── UpdateFieldValidator.cs
   DTOs/
   ├── FieldDto.cs
   └── FieldDetailsDto.cs
   ```

2. Создать `Controllers/FieldsController.cs`:
   ```
   GET    /api/fields               → GetFieldsListQuery
   GET    /api/fields/nearby        → GetFieldsNearbyQuery (?lat=...&lng=...&radius=10)
   GET    /api/fields/{id}          → GetFieldByIdQuery
   POST   /api/fields               → CreateFieldCommand    (FieldOwner+)
   PUT    /api/fields/{id}          → UpdateFieldCommand    (FieldOwner+)
   DELETE /api/fields/{id}          → DeleteFieldCommand    (Administrator)
   ```

3. Правила авторизации:
   - Просматривать поля могут все (включая гостей)
   - Создавать/редактировать — FieldOwner и Administrator
   - Удалять — только Administrator

4. Валидация:
   - Название: 3-150 символов
   - Адрес: обязателен, 10-500 символов
   - Координаты: Latitude от -90 до 90, Longitude от -180 до 180
   - Rules: опционально, до 10000 символов

5. Тесты:
   - `CreateFieldCommandTests.cs` — создание с валидными/невалидными координатами
   - `GetFieldsNearbyQueryTests.cs` — поиск в радиусе, сортировка по расстоянию

### Frontend — что нужно сделать

1. Создать `src/app/features/fields/`:
   ```
   pages/
   ├── fields-list/            # Список полигонов (карточки, поиск по городу/названию, фильтр по региону)
   ├── field-details/          # Детали полигона + правила + карта с маркером
   ├── field-create/           # Создание/редактирование полигона (форма с картой для выбора координат)
   └── fields-map/             # Карта со всеми полигонами (во весь экран, кластеризация маркеров)
   data/
   ├── fields.service.ts
   └── fields.models.ts
   ```

2. Использовать Leaflet (уже есть в package.json):
   - На странице `field-details` — карта с одним маркером
   - На странице `fields-map` — карта со всеми полигонами, кластеризация маркеров
   - На странице `field-create` — карта с возможностью кликнуть для выбора координат

3. Маршруты:
   - `/fields` → `fields-list`
   - `/fields/map` → `fields-map`
   - `/fields/:id` → `field-details`
   - `/fields/create` → `field-create` (защищён authGuard, только FieldOwner+)

### Критерии завершения
- [ ] Все CQRS для Fields реализованы
- [ ] FieldsController с 6 эндпоинтами работает
- [ ] `GetFieldsNearbyQuery` корректно сортирует по расстоянию
- [ ] Leaflet карта работает на страницах fields-details, fields-map, field-create
- [ ] Клик по карте позволяет выбрать координаты при создании полигона

---

## Задача 2b.3: API + Frontend для Games (Игры)

### Контекст
`Game` — центральная сущность платформы. Игры проводятся на полях, организуются организаторами, в них участвуют игроки (индивидуально или командами).

### Backend — что нужно сделать

1. Создать CQRS в `Application/Features/Games/`:
   ```
   Commands/
   ├── CreateGameCommand.cs / Handler
   ├── UpdateGameCommand.cs / Handler
   ├── DeleteGameCommand.cs / Handler
   ├── ChangeGameStatusCommand.cs / Handler    # Изменить статус (Scheduled→InProgress→Completed)
   ├── RegisterForGameCommand.cs / Handler     # Игрок регистрируется на игру
   ├── UnregisterFromGameCommand.cs / Handler  # Игрок отменяет регистрацию
   ├── ConfirmParticipantCommand.cs / Handler  # Организатор подтверждает участника
   └── CheckInParticipantCommand.cs / Handler  # Отметить прибытие
   Queries/
   ├── GetGamesListQuery.cs / Handler           # Пагинированный список с фильтрами
   ├── GetGameByIdQuery.cs / Handler            # Детали игры + участники
   ├── GetMyGamesQuery.cs / Handler             # Игры, где я участвую
   └── GetUpcomingGamesQuery.cs / Handler       # Ближайшие игры
   DTOs/
   ├── GameDto.cs
   ├── GameDetailsDto.cs
   └── GameParticipantDto.cs
   ```

2. Создать `Controllers/GamesController.cs`:
   ```
   GET    /api/games                 → GetGamesListQuery
   GET    /api/games/upcoming        → GetUpcomingGamesQuery
   GET    /api/games/my              → GetMyGamesQuery         (только auth)
   GET    /api/games/{id}            → GetGameByIdQuery
   POST   /api/games                 → CreateGameCommand       (Organizer+)
   PUT    /api/games/{id}            → UpdateGameCommand       (организатор игры)
   DELETE /api/games/{id}            → DeleteGameCommand       (организатор/админ)
   PATCH  /api/games/{id}/status     → ChangeGameStatusCommand (организатор)
   POST   /api/games/{id}/register   → RegisterForGameCommand  (auth)
   DELETE /api/games/{id}/register   → UnregisterFromGameCommand
   POST   /api/games/{id}/participants/{participantId}/confirm → ConfirmParticipantCommand
   POST   /api/games/{id}/participants/{participantId}/checkin → CheckInParticipantCommand
   ```

3. Бизнес-правила:
   - Игра может быть создана только Organizer (или Administrator)
   - Статусы идут по цепочке: Scheduled → InProgress → Completed (нельзя перескочить)
   - Нельзя зарегистрироваться на игру, если она уже InProgress или Completed
   - MaxParticipants — если достигнут, возвращать 409 Conflict
   - Date игры должна быть в будущем (при создании)
   - OrganizerId в Game должен быть Guid действующего Organizer

4. Валидация:
   - Date: обязательна, в будущем
   - Cost: >= 0
   - MaxParticipants: >= 2
   - Status: только допустимые переходы

5. Тесты:
   - `CreateGameCommandTests.cs` — создание, создание с прошлой датой
   - `RegisterForGameCommandTests.cs` — успешная регистрация, повторная, полная игра
   - `ChangeGameStatusCommandTests.cs` — корректные и некорректные переходы статусов
   - `GetUpcomingGamesQueryTests.cs` — фильтрация по дате и статусу

### Frontend — что нужно сделать

1. Создать `src/app/features/games/`:
   ```
   pages/
   ├── games-list/            # Список игр (карточки с датой, полем, статусом, количеством участников)
   ├── game-details/          # Детали игры + список участников + кнопка регистрации
   ├── game-create/           # Создание игры (выбор поля из списка/карты, дата, стоимость)
   ├── my-games/              # Мои игры (защищённый роут)
   └── upcoming-games/        # Ближайшие игры на карте
   data/
   ├── games.service.ts
   └── games.models.ts
   ```

2. Маршруты:
   - `/games` → `games-list`
   - `/games/upcoming` → `upcoming-games`
   - `/games/my` → `my-games` (authGuard)
   - `/games/:id` → `game-details`
   - `/games/create` → `game-create` (authGuard, Organizer+)

3. Ключевые UX-сценарии:
   - На `game-details`: кнопка "Зарегистрироваться" / "Отменить регистрацию", прогресс-бар заполнения (сейчас 15/50 участников), статус игры цветовой индикацией
   - На `game-create`: выбор поля из выпадающего списка или с карты, выбор даты через date picker, поле стоимости
   - На `games-list`: фильтры по статусу, полю, дате (от-до), сортировка по дате

### Критерии завершения
- [ ] Все CQRS для Games реализованы
- [ ] GamesController с 12 эндпоинтами работает
- [ ] Бизнес-правила (статусы, лимит участников, дата в будущем) соблюдены
- [ ] Страницы games-list, game-details, game-create, my-games, upcoming-games работают
- [ ] Регистрация на игру через UI работает

---

## Задача 2b.4: API + Frontend для Violations (Нарушения)

### Контекст
`Violation` — система репутации. Организаторы и администраторы могут фиксировать нарушения на игрока/команду во время игр. Это создаёт историю, влияющую на доверие.

### Backend — что нужно сделать

1. Создать CQRS в `Application/Features/Violations/`:
   ```
   Commands/
   ├── RecordViolationCommand.cs / Handler       # Записать нарушение
   ├── UpdateViolationCommand.cs / Handler       # Изменить нарушение
   └── DeleteViolationCommand.cs / Handler       # Удалить нарушение
   Queries/
   ├── GetViolationsListQuery.cs / Handler       # Пагинированный список с фильтрами
   ├── GetViolationByIdQuery.cs / Handler
   └── GetPlayerViolationsQuery.cs / Handler     # Нарушения конкретного игрока
   Validators/
   └── RecordViolationValidator.cs
   DTOs/
   ├── ViolationDto.cs
   └── ViolationDetailsDto.cs
   ```

2. Создать `Controllers/ViolationsController.cs`:
   ```
   GET    /api/violations                 → GetViolationsListQuery
   GET    /api/violations/{id}            → GetViolationByIdQuery
   GET    /api/violations/player/{airsofterId} → GetPlayerViolationsQuery
   POST   /api/violations                → RecordViolationCommand (Organizer+)
   PUT    /api/violations/{id}           → UpdateViolationCommand (создатель/админ)
   DELETE /api/violations/{id}           → DeleteViolationCommand (админ)
   ```

3. Правила:
   - Нарушение привязано к игре (GameId optional) или к полигону (FieldId)
   - Severity: Minor, Moderate, Severe, Critical
   - Описание: обязательно, 20-2000 символов
   - Нарушение может быть на игрока (AirsofterId) или на команду (TeamId)
   - Записать нарушение может Organizer, FieldOwner, Administrator

4. Тесты:
   - `RecordViolationCommandTests.cs` — запись, запись без описания, запись не-организатором
   - `GetPlayerViolationsQueryTests.cs` — получение списка, агрегация по severity

### Frontend — что нужно сделать

1. Создать `src/app/features/violations/`:
   ```
   pages/
   ├── violation-record/       # Форма записи нарушения (выбор игрока/команды, severity, описание)
   └── violation-history/      # История нарушений игрока/команды
   data/
   ├── violations.service.ts
   └── violations.models.ts
   ```

2. Интегрировать с существующим `airsofter-details`: добавить вкладку "Нарушения" с историей.

3. Маршруты:
   - `/violations/record` → `violation-record` (authGuard, Organizer+)
   - `/violations/player/:id` → `violation-history`
   - Вкладка на странице игрока: `/airsofters/:id/violations`

### Критерии завершения
- [ ] Все CQRS для Violations реализованы
- [ ] ViolationsController с 6 эндпоинтами работает
- [ ] Форма записи нарушения работает на фронтенде
- [ ] История нарушений отображается на странице игрока

---

## Задача 2b.5: API + Frontend для Organizers

### Контекст
`Organizer` — это не отдельная регистрация, а расширение прав Airsofter. Любой игрок может стать организатором, если администратор подтвердит. Организаторы могут создавать игры и управлять ими.

### Backend — что нужно сделать

1. Создать CQRS в `Application/Features/Organizers/`:
   ```
   Commands/
   ├── RequestOrganizerRoleCommand.cs / Handler    # Подать заявку
   ├── ApproveOrganizerCommand.cs / Handler        # Админ подтверждает
   └── RevokeOrganizerCommand.cs / Handler         # Админ отзывает
   Queries/
   ├── GetOrganizersListQuery.cs / Handler
   └── GetPendingOrganizersQuery.cs / Handler      # Заявки на утверждение
   DTOs/
   ├── OrganizerDto.cs
   └── OrganizerRequestDto.cs
   ```

2. Создать `Controllers/OrganizersController.cs`:
   ```
   GET    /api/organizers              → GetOrganizersListQuery
   GET    /api/organizers/pending      → GetPendingOrganizersQuery (Admin)
   POST   /api/organizers/request      → RequestOrganizerRoleCommand (auth)
   POST   /api/organizers/{id}/approve → ApproveOrganizerCommand (Admin)
   POST   /api/organizers/{id}/revoke  → RevokeOrganizerCommand (Admin)
   ```

### Frontend — что нужно сделать

1. Страница в профиле: "Стать организатором" с формой заявки.

2. Админ-панель для управления заявками (можно в рамках существующего dashboard или отдельного `/admin/organizers`).

3. Интегрировать с `dashboard/`: если пользователь Organizer, показывать кнопку "Создать игру".

### Критерии завершения
- [ ] CQRS для Organizers реализованы
- [ ] Подача заявки и подтверждение работают через UI

---

## Задача 2b.6: Рефакторинг дублирующихся компонентов

### Контекст
В проекте есть дубли и дедвейт: `my-profile/` и `me/` делают одно и то же, `players/` не используется в маршрутах.

### Что нужно сделать

1. Сравнить `my-profile` и `me`:
   - Определить, какой компонент полнее и лучше написан
   - Оставить один (рекомендуется `my-profile/`), удалить другой
   - Обновить маршруты в `app.routes.ts` — все ссылки на `/me` должны вести на `/profile`
   - Обновить `header.component.ts` и `main-nav.component.ts` если там есть ссылки на `/me`

2. Удалить `features/players/` — он не используется и не подключён в маршрутах.

3. Проверить, что после удаления:
   - `ng build --configuration production` проходит без ошибок
   - Все маршруты работают

### Критерии завершения
- [ ] `features/profile/me/` удалён (или смержен)
- [ ] `features/players/` удалён
- [ ] Маршруты обновлены, нет битых ссылок
- [ ] `ng build` проходит

---

## Зависимости

- Все задачи фазы 2b зависят от Фазы 2a (нужен Application слой)
- Задачи 2b.1–2b.5 независимы друг от друга и могут выполняться параллельно
- Задача 2b.6 может выполняться параллельно с любыми другими задачами

## Критерии завершения фазы

- [ ] Все 5 сущностей (Teams, Fields, Games, Violations, Organizers) имеют полный CRUD API
- [ ] Фронтенд-страницы для всех сущностей работают
- [ ] `dotnet build` и `ng build` проходят
- [ ] `dotnet test` — все тесты зелёные
- [ ] Дубликаты и дедвейт удалены
