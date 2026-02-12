# Инструкция: запуск и развёртывание в Docker

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

## 3. Проверка состояния
Проверьте контейнеры:
```bash
docker compose ps
```

Проверьте здоровье API через nginx:
```bash
curl -f http://localhost/health
```

## 4. Доступ к приложению
- Сайт: `http://localhost`
- API: `http://localhost/api`
- Swagger: `http://localhost/swagger`

## 5. Обновление приложения
После изменений в коде:
```bash
docker compose up --build -d
```

## 6. Остановка
```bash
docker compose down
```

Чтобы удалить тома БД/кеша (осторожно, удалятся данные):
```bash
docker compose down -v
```

## 7. Что исправлено для стабильного one-command deploy
- Добавлен `nginx/nginx.conf`.
- Добавлен `scripts/backup.sh`.
- Nginx теперь собирает frontend внутри Docker-образа и не требует локального `frontend/dist`.
- Убраны внешние Google Fonts из `frontend/src/index.html`, чтобы production build не падал в ограниченных сетях.
