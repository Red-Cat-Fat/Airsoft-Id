# Фаза 5: Production-ready

## Цель

Подготовить платформу к реальной эксплуатации: обеспечить безопасность, настроить SSL, внедрить мониторинг, провести нагрузочное тестирование, финализировать юридическую документацию.

**Техдолг в этой фазе:** Финальная ревизия кода — удалить закомментированный код, проверить логирование, унифицировать error handling.

---

## Задача 5.1: Аудит безопасности

### Контекст
Платформа содержит пользовательские данные, аутентификацию, платёжную информацию. Нужно провести систематический аудит безопасности.

### Что нужно сделать

1. **OWASP Top 10 проверка:**
   - **A01: Broken Access Control:** проверить все эндпоинты на корректную авторизацию. Убедиться, что пользователь не может изменить данные другой команды, отменить чужую регистрацию, и т.д.
   - **A02: Cryptographic Failures:** проверить, что JWT подписываются достаточно длинным ключом (>= 256 бит для HMAC-SHA256), пароли хешируются (Identity уже делает bcrypt по умолчанию)
   - **A03: Injection:** EF Core параметризирует запросы, но проверить raw SQL в миграциях и любых `FromSqlRaw` вызовах
   - **A04: Insecure Design:** проверить rate limiting на эндпоинты аутентификации (защита от brute force)
   - **A05: Security Misconfiguration:** CORS настроен? HSTS? Content Security Policy?
   - **A06: Vulnerable Components:** `dotnet list package --vulnerable` — проверить уязвимости
   - **A07: Auth Failures:** проверить refresh token rotation, logout инвалидацию
   - **A08: Data Integrity:** проверить, что webhook от Stripe верифицируется через signature
   - **A09: Logging Failures:** проверить, что чувствительные данные не попадают в логи
   - **A10: SSRF:** если есть загрузка по URL (логотипы), ограничить разрешённые домены

2. **Rate limiting:**
   - Установить `AspNetCoreRateLimit` NuGet или использовать встроенный rate limiting в .NET 9
   - Ограничения:
     - `/api/auth/login`: 5 попыток в минуту с одного IP
     - `/api/auth/register`: 3 регистрации в час с одного IP
     - Общий лимит: 100 запросов в минуту для неавторизованных, 500 для авторизованных

3. **Security headers:**
   - Проверить и дополнить `nginx/conf.d/default.conf`:
     ```
     add_header Content-Security-Policy "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'";
     add_header Strict-Transport-Security "max-age=31536000; includeSubDomains" always;
     add_header X-Frame-Options "SAMEORIGIN" always;
     add_header X-Content-Type-Options "nosniff" always;
     add_header Referrer-Policy "strict-origin-when-cross-origin" always;
     add_header Permissions-Policy "camera=(), geolocation=(), microphone=()" always;
     ```

4. **Аудит зависимостей:**
   - Backend: `dotnet list package --vulnerable`
   - Frontend: `npm audit`
   - Docker: `docker scout` (если доступен) или Trivy

### Критерии завершения
- [ ] OWASP Top 10 проверка пройдена, найденные уязвимости исправлены
- [ ] Rate limiting работает на эндпоинтах аутентификации
- [ ] Security headers настроены в Nginx
- [ ] `dotnet list package --vulnerable` и `npm audit` не показывают уязвимостей
- [ ] CORS настроен корректно (только доверенные домены)

---

## Задача 5.2: SSL-сертификаты и HTTPS

### Контекст
В Nginx конфиге секция SSL закомментирована. Для production обязательно HTTPS.

### Что нужно сделать

1. Раскомментировать и настроить SSL в `nginx/conf.d/default.conf`:
   ```nginx
   server {
       listen 443 ssl http2;
       server_name strikeball.ru www.strikeball.ru;

       ssl_certificate /etc/nginx/ssl/cert.pem;
       ssl_certificate_key /etc/nginx/ssl/key.pem;

       ssl_protocols TLSv1.2 TLSv1.3;
       ssl_ciphers ECDHE-ECDSA-AES128-GCM-SHA256:ECDHE-RSA-AES128-GCM-SHA256:ECDHE-ECDSA-AES256-GCM-SHA384:ECDHE-RSA-AES256-GCM-SHA384;
       ssl_prefer_server_ciphers on;
       ssl_session_cache shared:SSL:10m;
       ssl_session_timeout 10m;

       # Остальные директивы...
   }

   server {
       listen 80;
       server_name strikeball.ru www.strikeball.ru;
       return 301 https://$host$request_uri;
   }
   ```

2. Добавить Certbot / Let's Encrypt:
   - Создать `scripts/init-ssl.sh` для автоматического получения сертификата:
     ```bash
     #!/bin/bash
     docker compose run --rm certbot certonly --webroot \
       --webroot-path=/var/www/html \
       --email admin@strikeball.ru \
       --agree-tos \
       --no-eff-email \
       -d strikeball.ru -d www.strikeball.ru
     ```
   - Добавить сервис `certbot` в `docker-compose.yml`:
     ```yaml
     certbot:
       image: certbot/certbot
       volumes:
         - ./nginx/ssl:/etc/letsencrypt
         - ./nginx/html:/var/www/html
       entrypoint: "/bin/sh -c 'trap exit TERM; while :; do certbot renew; sleep 12h & wait $${!}; done;'"
     ```

3. Настроить автоматическое обновление сертификатов (certbot renew).

4. Обновить `launcher/start.sh`:
   - При первом запуске проверять наличие SSL-сертификатов
   - Если нет — получать через certbot (если указан домен) или генерировать self-signed

### Критерии завершения
- [ ] HTTPS работает на production-домене
- [ ] HTTP → HTTPS редирект настроен
- [ ] Сертификаты Let's Encrypt установлены
- [ ] Auto-renew настроен

---

## Задача 5.3: Мониторинг и логирование

### Контекст
Для production нужна observability: логи, метрики, алерты.

### Что нужно сделать

1. **Логирование (Serilog):**
   - Установить `Serilog.AspNetCore`, `Serilog.Sinks.Console`, `Serilog.Sinks.File`
   - Настроить structured logging в `Program.cs`:
     ```csharp
     Log.Logger = new LoggerConfiguration()
         .ReadFrom.Configuration(builder.Configuration)
         .Enrich.FromLogContext()
         .Enrich.WithMachineName()
         .Enrich.WithEnvironmentName()
         .WriteTo.Console()
         .WriteTo.File("logs/api-.log", rollingInterval: RollingInterval.Day)
         .CreateLogger();

     builder.Host.UseSerilog();
     ```
   - Настроить уровни логирования:
     - Все ошибки → файл + консоль
     - Warning → файл
     - Info → консоль (только важные события: регистрация, оплата, создание игры)
     - В логи не должны попадать пароли, токены, номера карт

2. **Метрики (Prometheus + Grafana):**
   - Установить `prometheus-net.AspNetCore`
   - Добавить эндпоинт `GET /metrics` (защищён внутренней сетью)
   - Добавить сервисы в `docker-compose.yml`:
     ```yaml
     prometheus:
       image: prom/prometheus:latest
       volumes:
         - ./monitoring/prometheus.yml:/etc/prometheus/prometheus.yml
       ports:
         - "9090:9090"

     grafana:
       image: grafana/grafana:latest
       volumes:
         - ./monitoring/grafana/dashboards:/var/lib/grafana/dashboards
         - ./monitoring/grafana/datasources:/etc/grafana/provisioning/datasources
       ports:
         - "3000:3000"
       environment:
         - GF_SECURITY_ADMIN_PASSWORD=${GRAFANA_PASSWORD}
     ```

3. **Health checks:**
   - Уже есть `GET /health` в API
   - Добавить проверки: БД (подключение), Redis (ping), платежный шлюз
   - Настроить Docker healthchecks (уже есть)

4. **Error tracking (Sentry):**
   - Установить `Sentry.AspNetCore`
   - Настроить в `Program.cs`:
     ```csharp
     builder.WebHost.UseSentry(options =>
     {
         options.Dsn = builder.Configuration["Sentry:Dsn"];
         options.TracesSampleRate = 1.0;
         options.ProfilesSampleRate = 1.0;
     });
     ```
   - Для фронтенда: `@sentry/angular-ivy`

5. **Алерты:**
   - Настроить базовые алерты в Prometheus/Grafana:
     - API недоступен > 5 минут
     - Error rate > 1%
     - P99 latency > 500ms
     - PostgreSQL connections > 80%

### Критерии завершения
- [ ] Serilog настроен с structured logging
- [ ] Prometheus собирает метрики с API
- [ ] Grafana дашборд с базовыми графиками работает
- [ ] Sentry ловит ошибки на backend и frontend
- [ ] Health checks проверяют все сервисы
- [ ] Алерты настроены для критических событий

---

## Задача 5.4: Нагрузочное тестирование

### Контекст
Перед запуском нужно понять, сколько пользователей выдерживает система, и где узкие места.

### Что нужно сделать

1. **Выбрать инструмент:** k6 (Grafana) — современный, скриптовый, интегрируется с Prometheus.

2. Создать `tests/load/scenarios/`:
   - `smoke-test.js` — 1 пользователь, проверить базовые эндпоинты
   - `auth-scenario.js` — 50 пользователей регистрируются и логинятся
   - `read-scenario.js` — 200 пользователей читают списки игроков/команд/игр
   - `mixed-scenario.js` — 100 пользователей (20% пишут, 80% читают)
   - `spike-scenario.js` — внезапный всплеск до 500 пользователей

3. Пример k6-скрипта:
   ```javascript
   import http from 'k6/http';
   import { check, sleep } from 'k6';

   export const options = {
     stages: [
       { duration: '2m', target: 50 },   // ramp up
       { duration: '5m', target: 50 },   // stay
       { duration: '2m', target: 200 },  // ramp up
       { duration: '5m', target: 200 },  // stay
       { duration: '2m', target: 0 },    // ramp down
     ],
     thresholds: {
       http_req_duration: ['p(95)<500'], // 95% запросов быстрее 500ms
       http_req_failed: ['rate<0.01'],   // менее 1% ошибок
     },
   };

   export default function () {
     const res = http.get('http://localhost:5000/api/airsofters');
     check(res, { 'status is 200': (r) => r.status === 200 });
     sleep(1);
   }
   ```

4. Запускать тесты против Docker-окружения:
   ```bash
   k6 run tests/load/scenarios/mixed-scenario.js
   ```

5. Анализировать результаты:
   - Какие эндпоинты самые медленные?
   - Где узкое место: БД, Redis, API, Nginx?
   - Оптимизировать, перезапустить тест

### Критерии завершения
- [ ] k6 скрипты созданы для всех сценариев
- [ ] Smoke-тест проходит без ошибок
- [ ] Mixed-сценарий: 100 пользователей, 95% запросов < 500ms
- [ ] Узкие места выявлены и оптимизированы

---

## Задача 5.5: Юридическая документация и политики

### Контекст
Платформа собирает пользовательские данные, обрабатывает платежи. Нужна юридическая защита.

### Что нужно сделать

1. **Проверить и дополнить существующие страницы:**
   - `Privacy Policy` — убедиться, что соответствует GDPR и 152-ФЗ (РФ)
   - `Terms of Service` — пользовательское соглашение
   - `Cookie Policy` — если используются сторонние cookie

2. **Добавить:**
   - Согласие на обработку персональных данных при регистрации (чекбокс)
   - Возможность удалить аккаунт и все данные (GDPR "right to erasure")
   - Возрастное ограничение 18+ (страйкбол — оружие)

3. **Лицензия:**
   - Выбрать и добавить `LICENSE` файл в корень проекта
   - MIT (если open source) или проприетарная

### Критерии завершения
- [ ] Privacy Policy, Terms of Service, Cookie Policy обновлены
- [ ] Чекбокс согласия при регистрации работает
- [ ] "Удалить аккаунт" функция реализована
- [ ] Возрастное ограничение 18+ при регистрации
- [ ] LICENSE файл добавлен

---

## Задача 5.6: Финальная ревизия кода

### Контекст
Финальная чистка перед production.

### Что нужно сделать

1. **Удалить закомментированный код:**
   - Пройтись по всему решению, удалить блоки `/* ... */` и `// ...` которые не несут смысла
   - Оставить только комментарии XML-doc для публичных API

2. **Проверить логирование:**
   - Убедиться, что `Log.Information` не содержит чувствительных данных
   - Убедиться, что эксепшены логируются на `Log.Error`

3. **Error handling:**
   - Добавить глобальный middleware для обработки ошибок в `Program.cs`:
     ```csharp
     app.UseMiddleware<ExceptionHandlingMiddleware>();
     ```
   - Middleware должен возвращать ProblemDetails (RFC 7807)

4. **Удалить неиспользуемые зависимости:**
   - `dotnet` и `npm` — проверить и удалить неиспользуемые пакеты
   - Удалить неиспользуемые import'ы

5. **Проверить конфигурацию:**
   - Убедиться, что все строки подключения, API-ключи вынесены в переменные окружения
   - Убедиться, что docker-compose.yml не содержит hardcoded значений

6. **Финальная сборка:**
   - `bash launcher/start.sh` — полный цикл сборки и запуска
   - Проверить все эндпоинты
   - Проверить frontend страницы

### Критерии завершения
- [ ] Весь закомментированный код удалён
- [ ] ExceptionHandlingMiddleware добавлен
- [ ] Неиспользуемые зависимости удалены
- [ ] Конфигурация чиста (нет секретов, нет hardcoded значений)
- [ ] `bash launcher/start.sh` успешно запускает всё приложение

---

## Зависимости

- Задача 5.1 (аудит безопасности) — желательно перед production
- Задача 5.2 (SSL) — обязательна перед выходом в production
- Задача 5.3 (мониторинг) — может быть после запуска
- Задача 5.4 (load testing) — перед запуском, после оптимизации
- Задача 5.5 (юридическая) — перед запуском
- Задача 5.6 (ревизия) — финальный шаг

## Критерии завершения фазы (и всего проекта)

- [ ] Аудит безопасности пройден, уязвимости устранены
- [ ] HTTPS работает с валидным SSL-сертификатом
- [ ] Мониторинг и алерты настроены
- [ ] Load-тесты пройдены (100+ пользователей, p95 < 500ms)
- [ ] Юридическая документация готова
- [ ] Код чист, без закомментированных блоков и секретов
- [ ] `bash launcher/start.sh` запускает production-ready приложение
