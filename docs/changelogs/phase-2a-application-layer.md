# Фаза 2a: Application слой + CQRS + Тесты + CI/CD

**Дата:** 19 июня 2026
**Ветка:** main

---

## Что сделано

### 1. Создан проект StrikeballApi.Application

`backend/src/StrikeballApi.Application/` — новый проект Clean Architecture слоя Application.

**NuGet-пакеты:** MediatR 12.2, AutoMapper 13.0.1, FluentValidation 11.9, EF Core 8.0

**Структура папок:**

```
StrikeballApi.Application/
├── Common/
│   ├── Interfaces/
│   │   ├── IApplicationDbContext.cs
│   │   ├── ICurrentUserService.cs
│   │   ├── IIdentityService.cs
│   │   └── ITokenService.cs
│   ├── Behaviours/
│   │   ├── ValidationBehaviour.cs
│   │   └── LoggingBehaviour.cs
│   ├── Exceptions/
│   │   ├── NotFoundException.cs
│   │   ├── ValidationException.cs
│   │   └── UnauthorizedException.cs
│   └── MappingProfile.cs
├── DTOs/
│   ├── Auth/
│   │   ├── LoginRequest.cs
│   │   ├── RegisterRequest.cs
│   │   ├── RefreshTokenRequest.cs
│   │   ├── AuthResponse.cs
│   │   └── UserInfo.cs
│   ├── Airsofters/
│   │   ├── AirsofterDto.cs
│   │   ├── AirsofterDetailsDto.cs
│   │   ├── GameHistoryDto.cs
│   │   └── ViolationDto.cs
│   └── Common/
│       └── PagedResult.cs
├── Features/
│   ├── Auth/
│   │   ├── Commands/
│   │   │   ├── RegisterAirsofterCommand.cs + Handler
│   │   │   ├── LoginCommand.cs + Handler
│   │   │   ├── RefreshTokenCommand.cs + Handler
│   │   │   └── LogoutCommand.cs + Handler
│   │   └── Queries/
│   │       └── GetCurrentUserQuery.cs + Handler
│   └── Airsofters/
│       ├── Queries/
│       │   ├── GetAirsoftersListQuery.cs + Handler
│       │   ├── GetAirsofterByIdQuery.cs + Handler
│       │   ├── GetAirsofterGameHistoryQuery.cs + Handler
│       │   └── GetAirsofterViolationsQuery.cs + Handler
│       ├── Commands/
│       │   ├── UpdateAirsofterCommand.cs + Handler
│       │   └── DeleteAirsofterCommand.cs + Handler
│       └── Validators/
│           ├── RegisterAirsofterValidator.cs
│           ├── UpdateAirsofterValidator.cs
│           └── LoginValidator.cs
└── DependencyInjection.cs
```

### 2. Реализованы интерфейсы и сервисы

- **IApplicationDbContext** — абстракция БД в Application слое
- **ICurrentUserService** — получение текущего пользователя из JWT
- **IIdentityService** — абстракция ASP.NET Identity (UserManager/SignInManager)
- **ITokenService** — генерация и валидация JWT токенов
- Реализация `CurrentUserService` в Infrastructure через `IHttpContextAccessor`
- Реализация `IdentityService` в Infrastructure (обёртка над UserManager/RoleManager/SignInManager)

### 3. Обновлены контроллеры

**AuthController** — теперь использует только `IMediator`:
- `POST /api/auth/register` → `RegisterAirsofterCommand`
- `POST /api/auth/login` → `LoginCommand`
- `POST /api/auth/refresh-token` → `RefreshTokenCommand`
- `POST /api/auth/logout` → `LogoutCommand`
- `GET /api/auth/me` → `GetCurrentUserQuery`

**AirsoftersController** — теперь использует только `IMediator`:
- `GET /api/airsofters` → `GetAirsoftersListQuery`
- `GET /api/airsofters/{id}` → `GetAirsofterByIdQuery`
- `GET /api/airsofters/{id}/game-history` → `GetAirsofterGameHistoryQuery`
- `GET /api/airsofters/{id}/violations` → `GetAirsofterViolationsQuery`
- `PUT /api/airsofters/{id}` → `UpdateAirsofterCommand`
- `DELETE /api/airsofters/{id}` → `DeleteAirsofterCommand`

### 4. Pipeline Behaviours

- **ValidationBehaviour** — автоматическая валидация FluentValidation перед каждым запросом
- **LoggingBehaviour** — логирование входящих/исходящих запросов и ошибок

### 5. Маппинг (AutoMapper)

Создан `MappingProfile` для:
- `Airsofter` → `AirsofterDto` / `AirsofterDetailsDto`
- `GameParticipant` → `GameHistoryDto`
- `Violation` → `ViolationDto`

### 6. Техдолг

- Создан `.editorconfig` в корне проекта
- `appsettings.json` очищен от секретов (заменены плейсхолдерами)
- Удалены старые WebApi Models (AuthResponse, LoginRequest, RegisterRequest, AirsofterDtos) — заменены DTO в Application слое
- Удалён пустой каталог Models/

### 7. Тесты

**Модульные тесты Application слоя** — 12 тестов:
| Файл | Тесты |
|------|-------|
| `RegisterAirsofterCommandTests` | Успешная регистрация |
| `LoginCommandTests` | Успешный вход + неверные данные |
| `RefreshTokenCommandTests` | Успешный refresh + неверный токен |
| `GetAirsoftersListQueryTests` | Все записи + фильтр по Callsign |
| `GetAirsofterByIdQueryTests` | Существующий + несуществующий |
| `UpdateAirsofterCommandTests` | Свой профиль, чужой профиль, несуществующий |

**Интеграционные тесты WebApi** — 5 тестов:
| Файл | Тесты |
|------|-------|
| `RegisterTests` | Успешная регистрация + несовпадение паролей |
| `LoginTests` | Неверные учётные данные → 401 |
| `GetAirsoftersListTests` | Пустой список |

### 8. CI/CD

- Создан `.github/workflows/ci.yml` — сборка, тесты, линтинг для backend (.NET) и frontend (Angular)
- Создан `.github/dependabot.yml` — автообновления NuGet, npm, Docker

---

## Изменённые файлы

### Новые файлы

```
.editorconfig
.github/workflows/ci.yml
.github/dependabot.yml
backend/src/StrikeballApi.Application/StrikeballApi.Application.csproj
backend/src/StrikeballApi.Application/DependencyInjection.cs
backend/src/StrikeballApi.Application/Common/MappingProfile.cs
backend/src/StrikeballApi.Application/Common/Interfaces/IApplicationDbContext.cs
backend/src/StrikeballApi.Application/Common/Interfaces/ICurrentUserService.cs
backend/src/StrikeballApi.Application/Common/Interfaces/IIdentityService.cs
backend/src/StrikeballApi.Application/Common/Interfaces/ITokenService.cs
backend/src/StrikeballApi.Application/Common/Behaviours/ValidationBehaviour.cs
backend/src/StrikeballApi.Application/Common/Behaviours/LoggingBehaviour.cs
backend/src/StrikeballApi.Application/Common/Exceptions/NotFoundException.cs
backend/src/StrikeballApi.Application/Common/Exceptions/ValidationException.cs
backend/src/StrikeballApi.Application/Common/Exceptions/UnauthorizedException.cs
backend/src/StrikeballApi.Application/DTOs/Auth/*.cs           (5 файлов)
backend/src/StrikeballApi.Application/DTOs/Airsofters/*.cs     (4 файла)
backend/src/StrikeballApi.Application/DTOs/Common/PagedResult.cs
backend/src/StrikeballApi.Application/Features/Auth/Commands/*.cs    (8 файлов)
backend/src/StrikeballApi.Application/Features/Auth/Queries/*.cs     (2 файла)
backend/src/StrikeballApi.Application/Features/Airsofters/Queries/*.cs (8 файлов)
backend/src/StrikeballApi.Application/Features/Airsofters/Commands/*.cs (4 файла)
backend/src/StrikeballApi.Application/Features/Airsofters/Validators/*.cs (3 файла)
backend/src/StrikeballApi.Infrastructure/Services/CurrentUserService.cs
backend/src/StrikeballApi.Infrastructure/Services/IdentityService.cs
backend/tests/StrikeballApi.Application.Tests/StrikeballApi.Application.Tests.csproj
backend/tests/StrikeballApi.Application.Tests/Common/TestDbContext.cs
backend/tests/StrikeballApi.Application.Tests/Features/Auth/*.cs     (3 файла)
backend/tests/StrikeballApi.Application.Tests/Features/Airsofters/*.cs (3 файла)
backend/tests/StrikeballApi.WebApi.Tests/IntegrationTests/IntegrationTestBase.cs
backend/tests/StrikeballApi.WebApi.Tests/IntegrationTests/Auth/*.cs   (2 файла)
backend/tests/StrikeballApi.WebApi.Tests/IntegrationTests/Airsofters/*.cs
```

### Изменённые файлы

```
backend/StrikeballApi.sln                          (+ проект Application, + тесты)
backend/src/StrikeballApi.WebApi/Program.cs        (+ AddApplication, IApplicationDbContext, IIdentityService)
backend/src/StrikeballApi.WebApi/StrikeballApi.WebApi.csproj  (+ reference на Application)
backend/src/StrikeballApi.WebApi/appsettings.json             (секреты → плейсхолдеры)
backend/src/StrikeballApi.WebApi/Controllers/AuthController.cs         (MediatR вместо сервисов)
backend/src/StrikeballApi.WebApi/Controllers/AirsoftersController.cs   (MediatR вместо репозитория)
backend/src/StrikeballApi.WebApi/Services/TokenService.cs              (новый интерфейс ITokenService)
backend/src/StrikeballApi.Infrastructure/StrikeballApi.Infrastructure.csproj (+ FrameworkReference)
backend/src/StrikeballApi.Infrastructure/Data/ApplicationDbContext.cs  (+ IApplicationDbContext)
backend/tests/StrikeballApi.WebApi.Tests/*.csproj     (+ references, + пакеты)
backend/tests/StrikeballApi.WebApi.Tests/AuthControllerRegistrationTests.cs (переписан под MediatR)
```

### Удалённые файлы

```
backend/src/StrikeballApi.WebApi/Models/          (весь каталог — заменён DTO из Application)
```

---

## Итоги

- **`dotnet build`** — 0 ошибок
- **`dotnet test`** — 17/17 тестов зеленые
- Бизнес-логика вынесена из контроллеров в Application слой
- CQRS-паттерн через MediatR внедрён
- Валидация работает через FluentValidation pipeline
- CI/CD pipeline настроен для GitHub Actions
