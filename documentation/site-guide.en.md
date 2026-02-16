# Strikeball Social Network Website Guide

This guide describes the main user flows currently available in the project: registration, sign-in, airsofter profile operations, and API usage.

## 1. Accessing the application

After starting the project, use:

- **Frontend**: `http://localhost:80` (via Nginx)
- **API**: `http://localhost:5000`
- **Swagger UI**: `http://localhost:5000/swagger`

> If you run the frontend locally with Angular CLI, it is typically available at `http://localhost:4200`.

## 2. User registration

Use endpoint:

- `POST /api/auth/register`

Request example:

```json
{
  "email": "player@example.com",
  "password": "StrongPass123",
  "confirmPassword": "StrongPass123",
  "firstName": "John",
  "lastName": "Doe",
  "middleName": "A",
  "callsign": "Ghost"
}
```

Important notes:

- `password` and `confirmPassword` must match.
- `callsign` must be unique.
- On successful registration, an airsofter profile is created and the `Airsofter` role is assigned automatically.

## 3. Sign in

Use endpoint:

- `POST /api/auth/login`

Request example:

```json
{
  "email": "player@example.com",
  "password": "StrongPass123"
}
```

Response includes:

- `accessToken` — send it as `Authorization: Bearer <token>`
- `refreshToken` — used to renew the access token
- `expiresAt` — access token expiration timestamp

## 4. Refresh token and logout

- Refresh token: `POST /api/auth/refresh-token`
- Logout: `POST /api/auth/logout` (requires authorization)

Refresh request example:

```json
{
  "refreshToken": "<your_refresh_token>"
}
```

## 5. Get current user information

Endpoint:

- `GET /api/auth/me` (requires authorization)

Returns:

- `id`
- `email`
- `callsign`
- `roles`

## 6. Working with airsofter profiles

### Public endpoints

Available without authentication:

- `GET /api/airsofters` — list all profiles
- `GET /api/airsofters/{id}` — profile by ID
- `GET /api/airsofters/callsign/{callsign}` — profile by callsign
- `GET /api/airsofters/{id}/game-history` — game history
- `GET /api/airsofters/{id}/violations` — violations

### Authorized endpoints

- `POST /api/airsofters` — create profile
- `PUT /api/airsofters/{id}` — update profile
- `DELETE /api/airsofters/{id}` — delete profile (requires `Administrator` role)

## 7. cURL usage examples

### Sign in

```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email":"player@example.com",
    "password":"StrongPass123"
  }'
```

### List airsofters

```bash
curl http://localhost:5000/api/airsofters
```

### Get current user (with token)

```bash
curl http://localhost:5000/api/auth/me \
  -H "Authorization: Bearer <access_token>"
```

## 8. Security recommendations

- Never share JWT tokens.
- Use strong passwords (minimum 8 chars, uppercase + lowercase + numbers).
- If account compromise is suspected, call `logout` and sign in again.

## 9. Common issues

- **401 Unauthorized**: missing or expired `accessToken`.
- **400 Bad Request during registration**: callsign already used or passwords mismatch.
- **404 Not Found**: no profile exists for the provided `id` or `callsign`.

---

When the API runs in Development mode, Swagger UI is the fastest way to explore and test endpoints: `http://localhost:5000/swagger`.
