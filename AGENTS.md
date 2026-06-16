All responses must be in Russian (Русский язык).
- **Backend Structure (C#):** Follow the Clean Architecture pattern. Logic resides in `*Api.Application/`, Domain models in `*Api.Domain/`, and Data Access in `*Api.Infrastructure/`.

## 🚀 Запуск и остановка проекта
Для запуска и остановки всего проекта (Docker + API + Frontend) используй скрипты из папки `launcher/`:

| Действие | MacOS / Linux | Windows |
|---|---|---|
| **Запуск** | `bash launcher/start.sh` | `.\launcher\start.ps1` |
| **Остановка** | `bash launcher/stop.sh` | `.\launcher\stop.ps1` |

**Что делают скрипты запуска:**
- Проверяют наличие Docker
- Если нет `.env` — интерактивно запрашивают параметры (порты, БД) со значениями по умолчанию и генерируют безопасные пароли
- Собирают и запускают контейнеры (`docker compose up --build -d`)
- Ожидают готовности PostgreSQL
- База данных создаётся автоматически при первом запросе к API (`EnsureCreatedAsync`)

**Что делают скрипты остановки:**
- Останавливают frontend dev server (если запущен на порту 4200)
- Останавливают и удаляют Docker-контейнеры (`docker compose down`)

## 🖥 Frontend Conventions (Angular)
- **Architecture:** Uses modern Angular standards: standalone components (no explicit NgModules).
- **State Management:** Prefer using Angular Signals for state management over older mechanisms like NgRx, unless complex global store logic is required.
- **UI Design Quirk:** New UI development *must* account for both light and dark themes. The core design system uses specific colors: Desert Tan (`#C2B280`), Sand-Tactical (`#A68A64`), and Amber Accent (`#FF8C00`).
- **Module Boundary:** Use `ng generate` commands to keep modules contained within the `src/features/` directory.

## 🧪 Testing Instructions
- **Backend Tests (API):** Navigate to the service root before testing: `cd backend && dotnet test`.
- **Frontend Unit Tests (Angular):** Run in the frontend directory: `cd frontend && ng test`.

## 🚢 Key Docker Commands
- **Build/Run:** `bash launcher/start.sh` (рекомендуется) или `docker compose up --build -d`
- **Migrate:** `docker compose exec api dotnet ef database update`
- **Debugging Access:** `docker compose exec api bash`
- **Backup DB Dump (Manual):** `docker compose exec postgres pg_dump -U postgres strikeballdb > backup.sql`

## 📚 Core API Endpoints (StrikeballSocialNetwork)
- **Authentication Flow:** Uses Bearer JWT tokens across all protected endpoints (`/api/*`).
- **List Components:** Check `/api/airsofters`, `/api/teams`, and `/api/games` for list retrieval.