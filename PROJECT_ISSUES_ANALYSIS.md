# Анализ потенциальных проблем проекта Airsoft-Id

## Краткий вывод
Проект имеет хорошую заготовку (backend + frontend + docker), но в текущем состоянии есть блокирующие проблемы для запуска/сборки и несколько архитектурных/безопасностных рисков.

## 1) Критичные проблемы (блокируют запуск/CI)

### 1.1 Backend не собирается в данном окружении
- В контейнере отсутствует `dotnet`, поэтому полноценная проверка backend-сборки не выполнена.
- Это повышает риск того, что ошибки компиляции backend обнаружатся только на CI/проде.

### 1.2 Frontend production-сборка падает из-за external fonts
- `npm run build` завершается ошибкой: Angular не может встроить Google Fonts (HTTP 403).
- Причина подтверждается импортом внешних шрифтов в `frontend/src/index.html`.
- Риск: сборка нестабильна в окружениях с ограниченным доступом к `fonts.googleapis.com`.

### 1.3 Тестовый контур frontend фактически пустой
- `npm run test` падает с `TS18003` — нет ни одного `*.spec.ts` файла, который указан в `tsconfig.spec.json`.
- Риск: отсутствие unit-тестов и нерабочая команда тестирования в CI.

### 1.4 Docker Compose содержит ссылки на отсутствующие файлы
- В `docker-compose.yml` указан mount `./nginx/nginx.conf`, но файла `nginx/nginx.conf` в репозитории нет.
- В `docker-compose.yml` указан mount `./scripts/backup.sh`, но файла `scripts/backup.sh` также нет.
- Риск: `docker compose up` с высокой вероятностью упадёт на старте или сервисы будут работать некорректно.

## 2) Высокий приоритет (функциональные/архитектурные риски)

### 2.1 Нет явного сидирования ролей Identity
- В коде есть назначение роли `Airsofter` при регистрации пользователя, но не найдена логика создания ролей в БД.
- Риск: регистрация может падать при первой попытке назначения роли, если роль заранее не создана.

### 2.2 Подозрение на потенциальную компиляционную проблему в TokenService
- В `TokenService` используется `FirstOrDefaultAsync`, но отсутствует явный `using Microsoft.EntityFrameworkCore;`.
- При определённых настройках/версиях это может приводить к ошибке компиляции.

### 2.3 Неполная реализация production-hardening CORS
- CORS разрешён только для `http://localhost:4200`.
- Для production это обычно недостаточно: при деплое за nginx/домен нужно параметризовать origin через конфиг.

## 3) Безопасность и эксплуатация

### 3.1 Секреты/пароли по умолчанию в конфиге
- В `appsettings.json` зашиты дефолтные строки подключения (`postgres/postgres`) и шаблонный JWT secret.
- Риск: небезопасные значения могут утечь в non-dev окружение.

### 3.2 Refresh token хранится в user-record как одно значение
- Текущая модель refresh token делает ротацию, но хранит только один токен на пользователя.
- Риск: неудобства/конфликты многосессионного входа (несколько устройств), ограниченные возможности revoke-аudit.

## 4) Документация и реальность проекта расходятся

### 4.1 README описывает более зрелую структуру, чем есть фактически
- Упоминаются каталоги/части (например, `tests`, `scripts`, mobile roadmap), которые не полностью представлены или нерабочие.
- Риск: неправильные ожидания у новых разработчиков, потеря времени при онбординге.

## 5) Рекомендованный план исправлений

1. **Стабилизировать сборку frontend**: убрать external font inlining зависимость (локальные шрифты, отключение inlining в CI, либо зеркала).  
2. **Починить тестовый контур frontend**: добавить минимальные `*.spec.ts` или скорректировать `tsconfig.spec.json`/скрипт test.  
3. **Исправить docker-compose mounts**: добавить `nginx/nginx.conf` и `scripts/backup.sh` (или убрать их из compose).  
4. **Добавить role seeding** (`Airsofter`, `TeamLeader`, `Organizer`, `FieldOwner`, `Administrator`) на старте приложения/миграциях.  
5. **Провести backend compile check в CI** с `dotnet build` + `dotnet test`.  
6. **Секреты вынести в env/secret manager**, убрать небезопасные дефолты из runtime-конфига.  
7. **Обновить README** под фактическое состояние, добавить точные шаги запуска и troubleshooting.

## 6) Команды, использованные для анализа
- `dotnet build backend/StrikeballApi.sln` (не выполнено: `dotnet` отсутствует в окружении)
- `cd frontend && npm install` (успешно)
- `cd frontend && npm run build` (ошибка inlining Google Fonts 403)
- `cd frontend && npm run test -- --watch=false --browsers=ChromeHeadless` (ошибка TS18003)
- Просмотр ключевых конфигов и исходников: `docker-compose.yml`, `frontend/src/index.html`, `frontend/tsconfig.spec.json`, `backend/src/StrikeballApi.WebApi/Program.cs`, `backend/src/StrikeballApi.WebApi/Controllers/AuthController.cs`, `backend/src/StrikeballApi.WebApi/Services/TokenService.cs`, `backend/src/StrikeballApi.WebApi/appsettings.json`.
