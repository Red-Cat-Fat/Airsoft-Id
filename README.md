# Strikeball Social Network

Комплексная веб-платформа социальной сети для любителей страйкбола, которая помогает участникам сообщества знакомиться, организовывать игры, отслеживать историю и управлять командной активностью.

## 🎯 Возможности

- **Профили игроков**: управление профилями страйкболистов, позывными, снаряжением и историей игр
- **Управление командами**: создание и администрирование команд с участниками, логотипами и статистикой
- **Организация игр**: планирование и проведение страйкбольных игр с регистрацией
- **Управление полигонами**: хранение информации о полигонах, картах, правилах и локациях
- **Система организаторов**: ролевая модель прав для управления играми и событиями
- **Учёт нарушений**: фиксация и управление нарушениями правил для игроков и команд
- **Аутентификация**: безопасная JWT-аутентификация с ролевым контролем доступа

## 🛠 Технологический стек

### Backend
- **Фреймворк**: ASP.NET Core 8.0 Web API
- **Язык**: C#
- **Архитектура**: Clean Architecture с паттерном CQRS
- **ORM**: Entity Framework Core
- **База данных**: PostgreSQL 15
- **Кеширование**: Redis
- **Аутентификация**: ASP.NET Core Identity + JWT
- **Документация API**: Swagger/OpenAPI

### Frontend
- **Фреймворк**: Angular (актуальная LTS-версия)
- **Язык**: TypeScript
- **UI-библиотека**: Angular Material / PrimeNG
- **Управление состоянием**: NgRx / Akita
- **Карты**: Leaflet с OpenStreetMap

### Mobile
- **Фреймворк**: Ionic Framework + Angular
- **Платформы**: iOS и Android
- **Переиспользование кода**: общие компоненты и сервисы с веб-приложением

### Infrastructure
- **Контейнеризация**: Docker и Docker Compose
- **Обратный прокси**: Nginx
- **Контроль версий**: Git

## ⚡ Быстрый Docker-деплой (одной командой)

1. Подготовьте секреты:
   ```bash
   cp .env.example .env
   ```
   Обновите в `.env` значения `POSTGRES_PASSWORD`, `REDIS_PASSWORD`, `JWT_SECRET_KEY`.

2. Запустите всё приложение одной командой:
   ```bash
   docker compose up --build -d
   ```

Подробная инструкция: [DOCKER_DEPLOYMENT.md](DOCKER_DEPLOYMENT.md).

> ℹ️ В проекте используется синтаксис Docker Compose V2: `docker compose ...`.
> Если команда `docker compose` недоступна, см. раздел [Запуск в WSL](#-запуск-в-wsl).

## 📋 Требования

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/)
- [Angular CLI](https://angular.io/cli)
- [Docker](https://www.docker.com/get-started) и [Docker Compose](https://docs.docker.com/compose/install/)
- [PostgreSQL 15](https://www.postgresql.org/download/) (для локальной разработки без Docker)

## 🚀 Быстрый старт

### 1. Клонируйте репозиторий

```bash
git clone https://github.com/yourusername/strikeball-social-network.git
cd strikeball-social-network
```

### 2. Настройте переменные окружения

Скопируйте пример файла окружения и обновите значения:

```bash
cp .env.example .env
```

Отредактируйте `.env` и задайте безопасные пароли/секреты:

```env
POSTGRES_PASSWORD=YourSecurePassword
REDIS_PASSWORD=YourSecureRedisPassword
JWT_SECRET_KEY=YourSuperSecretJWTKey
```

### 3. Запуск через Docker Compose

Самый простой способ старта — Docker Compose, который поднимет все сервисы:

```bash
docker compose up --build -d
```

Будут запущены:
- PostgreSQL (порт 5432)
- Redis (порт 6379)
- ASP.NET Core API (порт 5000)
- Nginx reverse proxy (порты 80, 443)
- сервис автоматических бэкапов

### 4. Примените миграции базы данных

После запуска контейнеров примените миграции:

```bash
docker compose exec api dotnet ef database update
```

### 5. Откройте приложение

- **API**: http://localhost:5000
- **Swagger-документация**: http://localhost:5000/swagger
- **Frontend** (после сборки): http://localhost:80

## 🔧 Настройка среды разработки

### Разработка Backend (без Docker)

1. **Установите .NET 8.0 SDK**

2. **Поднимите PostgreSQL** и создайте БД:
   ```sql
   CREATE DATABASE strikeballdb;
   ```

3. **Обновите строку подключения** в `backend/src/StrikeballApi.WebApi/appsettings.Development.json`

4. **Выполните миграции**:
   ```bash
   cd backend/src/StrikeballApi.WebApi
   dotnet ef database update
   ```

5. **Запустите API**:
   ```bash
   dotnet run
   ```

API будет доступно по адресу `https://localhost:5001` или `http://localhost:5000`.

### Разработка Frontend

1. **Установите Node.js и Angular CLI**:
   ```bash
   npm install -g @angular/cli
   ```

2. **Перейдите в директорию frontend**:
   ```bash
   cd frontend
   ```

3. **Установите зависимости**:
   ```bash
   npm install
   ```

4. **Запустите dev-сервер**:
   ```bash
   ng serve
   ```

Angular-приложение будет доступно по адресу `http://localhost:4200`.

## 🐧 Запуск в WSL

Если вы запускаете проект в WSL, используйте один из двух поддерживаемых вариантов.

### Вариант A (рекомендуется): Docker Desktop + интеграция с WSL

1. Установите Docker Desktop в Windows.
2. В Docker Desktop откройте **Settings → Resources → WSL Integration**.
3. Включите интеграцию для вашего дистрибутива (Ubuntu и т.д.).
4. Перезапустите WSL:
   ```bash
   wsl --shutdown
   ```
5. Проверьте доступность Compose V2:
   ```bash
   docker compose version
   ```
6. Запустите проект:
   ```bash
   cp .env.example .env
   docker compose up --build -d
   ```

### Вариант B: Docker Engine внутри WSL (без Docker Desktop)

1. Установите Docker Engine из официального Docker-репозитория.
2. Убедитесь, что установлен плагин `docker-compose-plugin`.
3. Проверьте команду:
   ```bash
   docker compose version
   ```
4. Запустите проект:
   ```bash
   cp .env.example .env
   docker compose up --build -d
   ```

### Проверка и типовые проблемы в WSL

- Проверка запущенных сервисов:
  ```bash
  docker compose ps
  ```
- Если видите `docker: unknown command: docker compose`, значит Compose V2 не установлен или не подключён к WSL.
- Если Docker запускается в Windows, а не в WSL, убедитесь, что WSL Integration включена в Docker Desktop.

## 📁 Структура проекта

```
strikeball-social-network/
├── backend/
│   ├── src/
│   │   ├── StrikeballApi.Domain/         # Сущности домена и enum-ы
│   │   ├── StrikeballApi.Application/    # Бизнес-логика (будет добавлено)
│   │   ├── StrikeballApi.Infrastructure/ # Доступ к данным и внешние сервисы
│   │   └── StrikeballApi.WebApi/         # API-контроллеры и конфигурация
│   ├── tests/                            # Unit и интеграционные тесты
│   └── Dockerfile                        # Docker-конфигурация API
├── frontend/                             # Angular веб-приложение
├── mobile/                               # Ionic мобильное приложение (будет добавлено)
├── nginx/                                # Конфигурация Nginx
│   ├── conf.d/
│   │   └── default.conf                  # Конфигурация сервера
│   └── certs/                            # SSL-сертификаты
├── scripts/                              # Служебные скрипты
├── docker-compose.yml                    # Конфигурация Docker Compose
├── .env.example                          # Шаблон переменных окружения
├── .gitignore                            # Правила игнорирования Git
└── README.md                             # Этот файл
```

## 🗄 Схема базы данных

Приложение использует следующие основные сущности:

- **Airsofters**: профили игроков с личной информацией и снаряжением
- **Teams**: данные команд, участники и статистика
- **Fields**: полигоны с картами и правилами
- **Games**: запланированные и завершённые игры
- **Organizers**: пользователи с расширенными правами
- **Violations**: записи о нарушениях
- **GameParticipants**: связь многие-ко-многим между играми и игроками
- **OrganizerFieldAuthorizations**: права организаторов на конкретные полигоны
- **FieldBannedTeams**: команды, заблокированные на конкретных полигонах

## 🔐 Аутентификация и авторизация

Приложение использует JWT-аутентификацию и следующие роли:

- **Guest**: неавторизованные пользователи (просмотр публичного контента)
- **Airsofter**: зарегистрированные игроки
- **Team Leader**: администраторы команд
- **Organizer**: менеджеры событий
- **Field Owner**: администраторы полигонов
- **Administrator**: администраторы платформы

## 🐳 Docker-команды

### Запуск всех сервисов
```bash
docker compose up -d
```

### Остановка всех сервисов
```bash
docker compose down
```

### Просмотр логов
```bash
docker compose logs -f [service-name]
# Пример: docker compose logs -f api
```

### Пересобрать и перезапустить сервис
```bash
docker compose up -d --build api
```

### Подключиться к контейнеру
```bash
docker compose exec api bash
```

### Резервная копия БД (вручную)
```bash
docker compose exec postgres pg_dump -U postgres strikeballdb > backup.sql
```

### Восстановление БД
```bash
docker compose exec -T postgres psql -U postgres strikeballdb < backup.sql
```

## 📊 API-эндпоинты

### Аутентификация
- `POST /api/auth/register` — регистрация нового пользователя
- `POST /api/auth/login` — вход пользователя
- `POST /api/auth/refresh-token` — обновление access-токена
- `POST /api/auth/logout` — выход пользователя

### Airsofters
- `GET /api/airsofters` — получить список игроков
- `GET /api/airsofters/{id}` — получить игрока по ID
- `POST /api/airsofters` — создать игрока
- `PUT /api/airsofters/{id}` — обновить игрока
- `DELETE /api/airsofters/{id}` — удалить игрока

### Teams
- `GET /api/teams` — получить список команд
- `GET /api/teams/{id}` — получить команду по ID
- `POST /api/teams` — создать команду
- `PUT /api/teams/{id}` — обновить команду
- `DELETE /api/teams/{id}` — удалить команду

### Games
- `GET /api/games` — получить список игр
- `GET /api/games/{id}` — получить игру по ID
- `POST /api/games` — создать игру
- `PUT /api/games/{id}` — обновить игру
- `DELETE /api/games/{id}` — удалить игру
- `POST /api/games/{id}/register` — зарегистрироваться на игру
- `DELETE /api/games/{id}/unregister` — отменить регистрацию на игру

### Fields
- `GET /api/fields` — получить список полигонов
- `GET /api/fields/{id}` — получить полигон по ID
- `POST /api/fields` — создать полигон
- `PUT /api/fields/{id}` — обновить полигон
- `DELETE /api/fields/{id}` — удалить полигон

Полная документация API доступна по адресу `/swagger` при запущенном API.

## 🧪 Тестирование

### Backend
```bash
cd backend
dotnet test
```

### Frontend
```bash
cd frontend
ng test
```

## 🚢 Развёртывание

### Продакшен-деплой

1. Обновите переменные окружения в `.env` значениями для production
2. Настройте SSL-сертификаты в `nginx/certs/`
3. Обновите конфигурацию Nginx под ваш домен
4. Соберите и запустите сервисы:
   ```bash
   docker compose -f docker-compose.yml up -d
   ```
5. Выполните миграции БД:
   ```bash
   docker compose exec api dotnet ef database update
   ```
6. Настройте firewall, открыв порты 80 и 443

### Автоматический скрипт деплоя

Для Ubuntu-серверов можно использовать скрипт `deploy.sh`, который автоматизирует полный процесс деплоя:

1. Сделайте скрипт исполняемым:
   ```bash
   chmod +x deploy.sh
   ```

2. Установите зависимости:
   ```bash
   ./deploy.sh --install-prerequisites
   ```

3. Выполните первичный деплой:
   ```bash
   ./deploy.sh --initial-deploy
   ```

Подробности: `DEPLOY_SCRIPT_README.md`.

### Мониторинг

Просмотр логов:
```bash
docker compose logs -f
```

Проверка здоровья сервисов:
```bash
curl http://localhost/health
```

## 📝 Лицензия

Проект распространяется по лицензии MIT. Подробности — в файле LICENSE.

## 👥 Вклад в проект

Мы рады pull request-ам и предложениям по улучшению проекта.

## 📧 Поддержка

Для вопросов и поддержки создайте issue в репозитории GitHub.

## 🗺 Дорожная карта

- [x] Структура проекта и архитектура
- [x] Доменные сущности и схема БД
- [x] Конфигурация Docker Compose
- [ ] API-эндпоинты аутентификации
- [ ] CRUD-операции для всех сущностей
- [ ] Angular frontend
- [ ] Ionic мобильное приложение
- [ ] Интеграция карт на Leaflet
- [ ] Уведомления в реальном времени
- [ ] Интеграция платежей
- [ ] Расширенная аналитика и статистика

## 🙏 Благодарности

- Создано с использованием ASP.NET Core и Angular
- PostgreSQL для надёжного хранения данных
- Redis для высокопроизводительного кеширования
- Docker для контейнеризации
- Nginx для reverse proxy и балансировки нагрузки
