# Фаза 2a: Application слой + CQRS + Тесты + CI/CD

## Цель

Создать недостающий Application слой Clean Architecture, внедрить CQRS-паттерн через MediatR, вынести бизнес-логику из контроллеров, покрыть ключевые сценарии тестами и настроить CI/CD.

**Техдолг в этой фазе:** Удалить секреты из `appsettings.json`, добавить `.editorconfig`.

---

## Задача 2a.1: Создать проект StrikeballApi.Application

### Контекст
Сейчас Clean Architecture нарушена: бизнес-логика Auth и Airsofter находится в контроллерах WebApi. Отсутствует слой Application, который должен содержать CQRS-команды/запросы, DTO, маппинг, валидацию. В README проекта упоминается, что Application слой и CQRS будут добавлены — это тот самый момент.

### Что нужно сделать

1. Создать проект `StrikeballApi.Application` в `backend/src/`:
   - Target Framework: `net8.0`
   - Добавить nuget-пакет `MediatR` (последняя стабильная версия для .NET 8)
   - Добавить nuget-пакет `AutoMapper` или `Mapster` для маппинга Entity → DTO
   - Добавить nuget-пакет `FluentValidation` для валидации команд/запросов
   - Добавить project reference на `StrikeballApi.Domain`

2. Создать структуру папок:
   ```
   StrikeballApi.Application/
   ├── Common/
   │   ├── Interfaces/
   │   │   ├── IApplicationDbContext.cs
   │   │   ├── ITokenService.cs
   │   │   └── ICurrentUserService.cs
   │   ├── Behaviours/
   │   │   ├── ValidationBehaviour.cs       # FluentValidation pipeline
   │   │   └── LoggingBehaviour.cs          # Serilog/ILogger pipeline
   │   └── Exceptions/
   │       ├── NotFoundException.cs
   │       ├── ValidationException.cs
   │       └── UnauthorizedException.cs
   ├── DTOs/
   │   ├── Auth/
   │   │   ├── LoginRequest.cs, RegisterRequest.cs, RefreshTokenRequest.cs
   │   │   ├── AuthResponse.cs, UserInfo.cs
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
   │   │   │   ├── RegisterAirsofterCommand.cs
   │   │   │   ├── RegisterAirsofterCommandHandler.cs
   │   │   │   ├── LoginCommand.cs
   │   │   │   ├── LoginCommandHandler.cs
   │   │   │   ├── RefreshTokenCommand.cs
   │   │   │   ├── RefreshTokenCommandHandler.cs
   │   │   │   ├── LogoutCommand.cs
   │   │   │   └── LogoutCommandHandler.cs
   │   │   └── Queries/
   │   │       ├── GetCurrentUserQuery.cs
   │   │       └── GetCurrentUserQueryHandler.cs
   │   └── Airsofters/
   │       ├── Queries/
   │       │   ├── GetAirsoftersListQuery.cs
   │       │   ├── GetAirsoftersListQueryHandler.cs
   │       │   ├── GetAirsofterByIdQuery.cs
   │       │   ├── GetAirsofterByIdQueryHandler.cs
   │       │   ├── GetAirsofterGameHistoryQuery.cs
   │       │   ├── GetAirsofterGameHistoryQueryHandler.cs
   │       │   ├── GetAirsofterViolationsQuery.cs
   │       │   └── GetAirsofterViolationsQueryHandler.cs
   │       ├── Commands/
   │       │   ├── UpdateAirsofterCommand.cs
   │       │   ├── UpdateAirsofterCommandHandler.cs
   │       │   ├── DeleteAirsofterCommand.cs
   │       │   └── DeleteAirsofterCommandHandler.cs
   │       └── Validators/
   │           ├── RegisterAirsofterValidator.cs
   │           ├── UpdateAirsofterValidator.cs
   │           └── LoginValidator.cs
   └── DependencyInjection.cs  # Extension method для регистрации MediatR, FluentValidation, AutoMapper
   ```

3. Реализовать `DependencyInjection.cs`:
   - `services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()))`
   - `services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly())`
   - `services.AddAutoMapper(Assembly.GetExecutingAssembly())`
   - Зарегистрировать pipeline behaviours (Validation, Logging)

4. Создать маппинг-профиль (AutoMapper):
   ```csharp
   public class MappingProfile : Profile
   {
       public MappingProfile()
       {
           CreateMap<Airsofter, AirsofterDto>();
           CreateMap<Airsofter, AirsofterDetailsDto>();
           CreateMap<Game, GameHistoryDto>();
           CreateMap<Violation, ViolationDto>();
       }
   }
   ```

5. Перенести существующую логику:
   - Из `AuthController` — в `Features/Auth/Commands/*`
   - Из `AirsoftersController` — в `Features/Airsofters/Commands/*` и `Queries/*`
   - Из `Infrastructure/Repositories/` — использовать `IApplicationDbContext` напрямую (через DI)
   - `TokenService.cs` оставить в WebApi или перенести интерфейс в Application, реализацию — в Infrastructure

6. Обновить `Program.cs` WebApi:
   - Добавить `builder.Services.AddApplication()` (из DependencyInjection)
   - Контроллеры должны только принимать команды/запросы через `IMediator`
   - Убрать прямые зависимости сервисов из контроллеров

### Критерии завершения
- [ ] Проект `StrikeballApi.Application` создан и скомпилирован
- [ ] Все команды/запросы для Auth и Airsofters реализованы
- [ ] Валидация работает через FluentValidation pipeline
- [ ] Маппинг Entity → DTO работает через AutoMapper
- [ ] Контроллеры используют только `IMediator`, без бизнес-логики
- [ ] `dotnet build` проходит без ошибок и предупреждений

---

## Задача 2a.2: Реализовать ICurrentUserService и IApplicationDbContext

### Контекст
Application слою нужен доступ к текущему пользователю (UserId, Roles) для авторизации команд, и к DbContext для выполнения запросов. Эти интерфейсы должны быть в Application, а реализации — в Infrastructure и WebApi соответственно.

### Что нужно сделать

1. В `Application/Common/Interfaces/ICurrentUserService.cs`:
   ```csharp
   public interface ICurrentUserService
   {
       Guid? UserId { get; }
       string? UserName { get; }
       string? IpAddress { get; }
       bool IsAuthenticated { get; }
       bool IsInRole(string role);
       bool IsAdministrator { get; }
   }
   ```

2. В `Infrastructure/Services/CurrentUserService.cs`:
   - Реализация через `IHttpContextAccessor`
   - Извлекать UserId из JWT claims
   - Проверять роли через `IsInRole`

3. В `Application/Common/Interfaces/IApplicationDbContext.cs`:
   ```csharp
   public interface IApplicationDbContext
   {
       DbSet<Airsofter> Airsofters { get; }
       DbSet<Team> Teams { get; }
       DbSet<Field> Fields { get; }
       DbSet<Game> Games { get; }
       DbSet<GameParticipant> GameParticipants { get; }
       DbSet<Organizer> Organizers { get; }
       DbSet<Violation> Violations { get; }
       DbSet<FieldBannedTeam> FieldBannedTeams { get; }
       DbSet<OrganizerFieldAuthorization> OrganizerFieldAuthorizations { get; }
       Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
   }
   ```

4. Реализовать интерфейс в `Infrastructure/Data/ApplicationDbContext.cs` (он уже частично это делает).

5. Зарегистрировать `ICurrentUserService` как Scoped.

### Критерии завершения
- [ ] `ICurrentUserService` добавлен в Application и реализован в Infrastructure
- [ ] `IApplicationDbContext` добавлен в Application и реализован в Infrastructure
- [ ] Все CQRS-хендлеры используют `IApplicationDbContext` вместо прямого `ApplicationDbContext`
- [ ] Проверка прав через `ICurrentUserService` работает в командах

---

## Задача 2a.3: Написать модульные тесты для Application слоя

### Контекст
В проекте всего 2 теста. Нужно заложить культуру тестирования с самого начала. Первыми покрываем Application слой — он содержит бизнес-логику и наиболее критичен.

### Что нужно сделать

1. В `tests/StrikeballApi.Application.Tests/`:
   - Target: `net8.0`
   - NuGet: `xunit`, `Moq`, `FluentAssertions`, `Shouldly`
   - Project reference на `StrikeballApi.Application`

2. Структура тестов (копирует структуру Features):
   ```
   Tests/
   └── Features/
       ├── Auth/
       │   ├── RegisterAirsofterCommandTests.cs
       │   ├── LoginCommandTests.cs
       │   └── RefreshTokenCommandTests.cs
       └── Airsofters/
           ├── GetAirsoftersListQueryTests.cs
           ├── GetAirsofterByIdQueryTests.cs
           └── UpdateAirsofterCommandTests.cs
   ```

3. Что тестировать в каждом хендлере:
   - **Успешный сценарий** — данные корректны, операция выполнена
   - **Несуществующая сущность** — `NotFoundException`
   - **Недостаточно прав** — `UnauthorizedException` / `ForbiddenException`
   - **Невалидные входные данные** — `ValidationException`
   - **Дубликаты** — попытка создать пользователя с существующим Callsign

4. Использовать `Mock<IApplicationDbContext>` и `Mock<ICurrentUserService>` для изоляции.

5. Пример теста:
   ```csharp
   public class RegisterAirsofterCommandTests
   {
       private readonly Mock<IApplicationDbContext> _context;
       private readonly Mock<IMapper> _mapper;
       private readonly RegisterAirsofterCommandHandler _handler;

       public RegisterAirsofterCommandTests()
       {
           _context = new Mock<IApplicationDbContext>();
           _mapper = new Mock<IMapper>();
           _handler = new RegisterAirsofterCommandHandler(_context.Object, _mapper.Object);
       }

       [Fact]
       public async Task Handle_ValidRequest_ReturnsAuthResponse()
       {
           // Arrange
           var command = new RegisterAirsofterCommand
           {
               Email = "test@example.com",
               Password = "Password123!",
               FirstName = "John",
               LastName = "Doe",
               Callsign = "JDoe"
           };

           // Act
           var result = await _handler.Handle(command, CancellationToken.None);

           // Assert
           result.Should().NotBeNull();
           result.Callsign.Should().Be("JDoe");
       }
   }
   ```

### Критерии завершения
- [ ] Проект `StrikeballApi.Application.Tests` создан
- [ ] Написано минимум 10 тестов (по 2 на каждый из 5 ключевых хендлеров)
- [ ] `dotnet test` проходит успешно
- [ ] Покрыты success и failure сценарии

---

## Задача 2a.4: Написать интеграционные тесты для WebApi

### Контекст
Модульные тесты изолированы от БД. Нужны интеграционные тесты, которые проверяют реальную работу API с PostgreSQL (через Testcontainers).

### Что нужно сделать

1. В `tests/StrikeballApi.WebApi.Tests/` (существующий проект) обновить:
   - Добавить `Testcontainers.PostgreSql` (nuget)
   - Добавить `Microsoft.AspNetCore.Mvc.Testing` (nuget)

2. Написать базовый класс `IntegrationTestBase`:
   - Запускает PostgreSQL в Testcontainer
   - Накатывает миграции EF Core
   - Создаёт `HttpClient` через `WebApplicationFactory`
   - Очищает БД между тестами

3. Написать тесты:
   ```
   IntegrationTests/
   ├── Auth/
   │   ├── RegisterTests.cs
   │   ├── LoginTests.cs
   │   └── RefreshTokenTests.cs
   └── Airsofters/
       ├── GetAirsoftersListTests.cs
       ├── GetAirsofterByIdTests.cs
       └── UpdateAirsofterTests.cs
   ```

4. Каждый тест:
   - Делает реальный HTTP-запрос к API
   - Проверяет HTTP-статус код
   - Проверяет тело ответа (JSON)

5. Пример:
   ```csharp
   [Fact]
   public async Task Register_ValidData_ReturnsOkWithTokens()
   {
       // Arrange
       var request = new RegisterRequest
       {
           Email = "test@test.com",
           Password = "Pass123!",
           FirstName = "Test",
           LastName = "User",
           Callsign = "Tester"
       };

       // Act
       var response = await _client.PostAsJsonAsync("/api/auth/register", request);

       // Assert
       response.StatusCode.Should().Be(HttpStatusCode.OK);
       var content = await response.Content.ReadFromJsonAsync<AuthResponse>();
       content.Should().NotBeNull();
       content.AccessToken.Should().NotBeNullOrEmpty();
   }
   ```

### Критерии завершения
- [ ] Testcontainer для PostgreSQL настроен
- [ ] Написано минимум 6 интеграционных тестов
- [ ] `dotnet test` проходит локально (без внешней БД)
- [ ] Тесты изолированы, не зависят друг от друга

---

## Задача 2a.5: Настроить CI/CD (GitHub Actions)

### Контекст
Проект не имеет CI/CD. Каждый коммит должен проходить сборку, линтинг, тесты. Это базовый quality gate перед мержем.

### Что нужно сделать

1. Создать `.github/workflows/ci.yml`:
   ```yaml
   name: CI Pipeline

   on:
     push:
       branches: [main, develop]
     pull_request:
       branches: [main]

   jobs:
     backend:
       runs-on: ubuntu-latest
       services:
         postgres:
           image: postgres:15-alpine
           env:
             POSTGRES_DB: strikeball_test
             POSTGRES_USER: postgres
             POSTGRES_PASSWORD: postgres
           ports:
             - 5432:5432
         redis:
           image: redis:7-alpine
           ports:
             - 6379:6379

       steps:
         - uses: actions/checkout@v4
         - uses: actions/setup-dotnet@v4
           with:
             dotnet-version: '8.0.x'
         - name: Restore
           run: dotnet restore backend/StrikeballApi.sln
         - name: Build
           run: dotnet build backend/StrikeballApi.sln --no-restore --configuration Release
         - name: Test
           run: dotnet test backend/StrikeballApi.sln --no-build --configuration Release
         - name: Lint
           run: dotnet format backend/StrikeballApi.sln --verify-no-changes

     frontend:
       runs-on: ubuntu-latest
       steps:
         - uses: actions/checkout@v4
         - uses: actions/setup-node@v4
           with:
             node-version: '22'
         - name: Install
           run: npm ci
           working-directory: ./frontend
         - name: Lint
           run: npx ng lint
           working-directory: ./frontend
         - name: Build
           run: npx ng build --configuration production
           working-directory: ./frontend
         - name: Test
           run: npx ng test --no-watch --browsers ChromeHeadless
           working-directory: ./frontend
   ```

2. Добавить правило `pull_request` в GitHub: требуется прохождение CI перед мержем.

3. Настроить бейдж в `README.md`:
   ```markdown
   [![CI Pipeline](https://github.com/USER/Airsoft-Id/actions/workflows/ci.yml/badge.svg)](https://github.com/USER/Airsoft-Id/actions/workflows/ci.yml)
   ```

4. Создать `.github/dependabot.yml` для автоматических обновлений зависимостей:
   ```yaml
   version: 2
   updates:
     - package-ecosystem: "nuget"
       directory: "/backend"
       schedule:
         interval: "weekly"
     - package-ecosystem: "npm"
       directory: "/frontend"
       schedule:
         interval: "weekly"
     - package-ecosystem: "docker"
       directory: "/"
       schedule:
         interval: "weekly"
   ```

### Критерии завершения
- [ ] `.github/workflows/ci.yml` создан и работает
- [ ] `.github/dependabot.yml` создан
- [ ] Бейдж CI добавлен в README
- [ ] pipeline зелёный на тестовом коммите

---

## Задача 2a.6: Техдолг — Editorconfig + секреты

### Контекст
В проекте нет `.editorconfig`, что приводит к разнобою в стиле кода. `appsettings.json` содержит реальные пароли, что недопустимо для репозитория.

### Что нужно сделать

1. Создать `.editorconfig` в корне проекта:
   ```ini
   root = true

   [*]
   indent_style = space
   indent_size = 4
   charset = utf-8
   trim_trailing_whitespace = true
   insert_final_newline = true

   [*.{cs,csproj,sln}]
   indent_size = 4

   [*.{ts,html,scss,json,yml,yaml}]
   indent_size = 2

   [*.md]
   trim_trailing_whitespace = false
   ```

2. В `backend/src/StrikeballApi.WebApi/appsettings.json`:
   - Заменить `"DefaultConnection": "Host=localhost;Database=strikeballdb;Username=postgres;Password=postgres"` на `"DefaultConnection": "__DB_CONNECTION_STRING__"`
   - Заменить `"SecretKey": "YourSuperSecretKeyAtLeast32CharactersLong"` на `"SecretKey": "__JWT_SECRET_KEY__"`
   - Все секреты должны передаваться через переменные окружения (Docker Compose это уже делает через `${POSTGRES_PASSWORD}` и `${JWT_SECRET_KEY}`)

3. Проверить, что `appsettings.Development.json` аналогично очищен от секретов.

4. Убедиться, что в `Program.cs` корректно читаются переменные окружения:
   ```csharp
   builder.Configuration
       .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
       .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
       .AddEnvironmentVariables();
   ```

### Критерии завершения
- [ ] `.editorconfig` создан в корне проекта
- [ ] `appsettings.json` не содержит реальных паролей и ключей
- [ ] Все секреты передаются через переменные окружения
- [ ] `dotnet build` проходит после изменений

---

## Зависимости

- Задача 2a.1 (Application слой) — база для всех последующих
- Задача 2a.2 (IApplicationDbContext, ICurrentUserService) — необходима для 2a.1
- Задача 2a.3 и 2a.4 (тесты) — после 2a.1 и 2a.2
- Задача 2a.5 (CI/CD) — может выполняться параллельно после 2a.1
- Задача 2a.6 (Editorconfig) — независима, выполняется в начале

## Критерии завершения фазы

- [ ] Все 6 задач выполнены
- [ ] `dotnet build backend/StrikeballApi.sln` проходит без ошибок
- [ ] `dotnet test backend/StrikeballApi.sln` — все тесты зелёные
- [ ] CI pipeline в GitHub Actions проходит успешно
- [ ] Business logic вынесена из контроллеров в Application слой
