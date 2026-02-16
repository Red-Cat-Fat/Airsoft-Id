# Руководство по работе с сайтом Strikeball Social Network

Это руководство описывает базовые сценарии использования сайта на текущем этапе разработки: регистрация, вход, работа с профилем страйкболиста и использование API.

## 1. Доступ к приложению

После запуска проекта вы можете использовать:

- **Frontend**: `http://localhost:80` (через Nginx)
- **API**: `http://localhost:5000`
- **Swagger UI**: `http://localhost:5000/swagger`

> Если вы используете локальный запуск frontend через Angular CLI, адрес обычно `http://localhost:4200`.

## 2. Регистрация пользователя

Для создания аккаунта используйте endpoint:

- `POST /api/auth/register`

Пример запроса:

```json
{
  "email": "player@example.com",
  "password": "StrongPass123",
  "confirmPassword": "StrongPass123",
  "firstName": "Иван",
  "lastName": "Петров",
  "middleName": "Сергеевич",
  "callsign": "Ghost"
}
```

Что важно:

- `password` и `confirmPassword` должны совпадать.
- `callsign` (позывной) должен быть уникальным.
- После успешной регистрации автоматически создаётся профиль страйкболиста и назначается роль `Airsofter`.

## 3. Вход в систему

Для входа используйте endpoint:

- `POST /api/auth/login`

Пример запроса:

```json
{
  "email": "player@example.com",
  "password": "StrongPass123"
}
```

В ответе вы получите:

- `accessToken` — передаётся в заголовке `Authorization: Bearer <token>`
- `refreshToken` — для обновления access-токена
- `expiresAt` — время истечения access-токена

## 4. Обновление токена и выход

- Обновление токена: `POST /api/auth/refresh-token`
- Выход: `POST /api/auth/logout` (требуется авторизация)

Пример запроса на обновление:

```json
{
  "refreshToken": "<your_refresh_token>"
}
```

## 5. Получение информации о текущем пользователе

Endpoint:

- `GET /api/auth/me` (требуется авторизация)

Этот метод возвращает:

- `id`
- `email`
- `callsign`
- `roles`

## 6. Работа с профилями страйкболистов

### Публичные методы

Доступны без авторизации:

- `GET /api/airsofters` — список профилей
- `GET /api/airsofters/{id}` — профиль по ID
- `GET /api/airsofters/callsign/{callsign}` — профиль по позывному
- `GET /api/airsofters/{id}/game-history` — история игр
- `GET /api/airsofters/{id}/violations` — нарушения

### Методы с авторизацией

- `POST /api/airsofters` — создать профиль
- `PUT /api/airsofters/{id}` — обновить профиль
- `DELETE /api/airsofters/{id}` — удалить профиль (только роль `Administrator`)

## 7. Пример использования через cURL

### Логин

```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email":"player@example.com",
    "password":"StrongPass123"
  }'
```

### Получение списка страйкболистов

```bash
curl http://localhost:5000/api/airsofters
```

### Получение текущего пользователя (с токеном)

```bash
curl http://localhost:5000/api/auth/me \
  -H "Authorization: Bearer <access_token>"
```

## 8. Рекомендации по безопасности

- Не передавайте JWT-токены третьим лицам.
- Используйте сложные пароли (минимум 8 символов, верхний и нижний регистр, цифры).
- При подозрении на компрометацию выполните `logout` и повторный вход.

## 9. Типовые проблемы

- **401 Unauthorized**: отсутствует/истёк `accessToken`.
- **400 Bad Request при регистрации**: позывной уже занят или пароли не совпадают.
- **404 Not Found**: профиль по указанному `id` или `callsign` не найден.

---

Если API запущено в режиме Development, удобнее всего тестировать и изучать методы через Swagger UI: `http://localhost:5000/swagger`.
