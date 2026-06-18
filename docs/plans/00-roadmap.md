# Roadmap развития проекта Strikeball Social Network / Airsoft ID

## Общая карта

```
Фаза 2a ──► Фаза 2b ──► Фаза 3 ──► Фаза 4 ──► Фаза 5
(Application   (API         (Фичи:       (Масштаби-   (Production-
 слой,         Completion)   карта,       рование:     ready:
 CQRS,                       поиск,       мобилка,     security,
 тесты,                       i18n,       админка,     SSL,
 CI/CD)                       обновление   платежи,     мониторинг)
                              версий)      чат)
```

## Содержание файлов

| Файл | Описание |
|---|---|
| `02a-phase-application-layer.md` | Application слой с CQRS (MediatR), модульные и интеграционные тесты, CI/CD pipeline |
| `02b-phase-api-completion.md` | Backend API + Frontend для Teams, Games, Fields, Violations, Organizers |
| `03-phase-functional-leap.md` | Интерактивная карта (Leaflet), уведомления (SignalR), поиск, i18n, обновление Angular 19 + .NET 9 |
| `04-phase-scaling.md` | Мобильное приложение (Ionic, Android + iOS), админ-панель, платёжная система, чат/мессенджер |
| `05-phase-production-ready.md` | Аудит безопасности, SSL-сертификаты, мониторинг (Prometheus/Grafana/Sentry), load-тестирование |

## Принципы и техдолг

Техдолг **не выносится в отдельную фазу**, а распределяется равномерно:
- Каждая фаза начинается с небольшого рефакторинга/чистки затрагиваемого участка
- Editorconfig, секреты в appsettings.json, дубли компонентов, дедвейт — исправляются по мере касания

## Общие конвенции (действуют для всех фаз)

### Backend (C# / .NET)
- **Архитектура:** Clean Architecture (Domain → Application → Infrastructure → WebApi)
- **CQRS:** Команды и запросы через MediatR
- **ORM:** Entity Framework Core 8.0 (Code-First, Fluent API конфигурации)
- **Auth:** JWT Bearer + ASP.NET Core Identity (6 ролей: Guest, Airsofter, TeamLeader, Organizer, FieldOwner, Administrator)
- **БД:** PostgreSQL (Npgsql)
- **Кеш:** Redis (StackExchange.Redis)
- **Тесты:** xUnit + Moq + InMemory Database или TestContainer для PostgreSQL
- **Стиль:** Следовать существующим конвенциям в коде (нейминг, namespace, folder structure)

### Frontend (Angular / TypeScript)
- **Архитектура:** Standalone components, без NgModules
- **Стейт-менеджмент:** Angular Signals (не NgRx)
- **UI Design:** Desert Tan `#C2B280`, Sand-Tactical `#A68A64`, Amber Accent `#FF8C00`
- **Темизация:** CSS-переменные в `styles.scss` (:root / .theme-dark)
- **HTTP:** Функциональные интерцепторы (provideHttpClient)
- **Навигация:** Ленивая загрузка через loadComponent
- **Генерация:** `ng generate` в `src/features/`

### DevOps
- **Контейнеризация:** Docker Compose (5 сервисов: postgres, redis, api, nginx, backup)
- **Сеть:** strikeball-network (bridge)
- **Reverse Proxy:** Nginx (прокси /api/, /health, /swagger → api:5000, статика → Angular)
- **Deploy:** bash launcher/start.sh для локального запуска
