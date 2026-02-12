# Инструкция: запуск и развёртывание в Docker

> Используется Docker Compose V2 (`docker compose ...`).

## 1. Подготовка секретов
1. Скопируйте шаблон переменных окружения:
   ```bash
   cp .env.example .env
   ```
2. Обязательно замените значения:
   - `POSTGRES_PASSWORD`
   - `REDIS_PASSWORD`
   - `JWT_SECRET_KEY` (минимум 32 символа)

## 2. Развёртывание одной командой
После подготовки `.env` запустите весь стек (backend + frontend + postgres + redis + backup + nginx):

```bash
docker compose up --build -d
```

## 3. Запуск в WSL

### Вариант A (рекомендуется): Docker Desktop + WSL Integration
1. Установите Docker Desktop в Windows.
2. Включите интеграцию с нужным дистрибутивом WSL в:
   **Settings → Resources → WSL Integration**.
3. Перезапустите WSL:
   ```bash
   wsl --shutdown
   ```
4. Проверьте доступность Compose:
   ```bash
   docker compose version
   ```

### Вариант B: Docker Engine внутри WSL
1. Установите Docker Engine из официального Docker-репозитория.
2. Установите Compose V2 plugin (`docker-compose-plugin`).
3. Проверьте:
   ```bash
   docker compose version
   ```

Если команда возвращает `docker: unknown command: docker compose`, значит Compose V2 не установлен или не подключён к текущему WSL-дистрибутиву.

## 4. Проверка состояния
Проверьте контейнеры:
```bash
docker compose ps
```

Проверьте здоровье API через nginx:
```bash
curl -f http://localhost/health
```

## 5. Доступ к приложению
- Сайт: `http://localhost`
- API: `http://localhost/api`
- Swagger: `http://localhost/swagger`

## 6. Обновление приложения
После изменений в коде:
```bash
docker compose up --build -d
```

## 7. Остановка
```bash
docker compose down
```

Чтобы удалить тома БД/кеша (осторожно, удалятся данные):
```bash
docker compose down -v
```

## 8. Что исправлено для стабильного one-command deploy
- Добавлен `nginx/nginx.conf`.
- Добавлен `scripts/backup.sh`.
- Nginx теперь собирает frontend внутри Docker-образа и не требует локального `frontend/dist`.
- Убраны внешние Google Fonts из `frontend/src/index.html`, чтобы production build не падал в ограниченных сетях.
