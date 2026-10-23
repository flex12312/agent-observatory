# Локальный запуск backend

## Что подготовлено

Основа задачи #5: solution, подключение EF Core к PostgreSQL, health checks, OpenAPI и стандартные ответы ProblemDetails. Предметных сущностей, миграций и CI пока нет. Задача #5 ещё не завершена.

## Требования

- .NET SDK 10.0, стабильная версия. global.json допускает установленный feature band внутри .NET 10.
- Docker с Linux-контейнерами и Docker Compose. Docker Desktop должен быть запущен.
- Свободные порты 5080 для API и 55432 для PostgreSQL.

Команды ниже выполняются из корня репозитория в PowerShell.

## 1. Поднять PostgreSQL

```powershell
Copy-Item .env.example .env
docker compose up -d --wait
```

Копировать .env нужно только при первом запуске. Пароль в примере предназначен только для локальной разработки. .env исключён из Git. База доступна только на loopback-интерфейсе; данные хранятся в Docker volume.

## 2. Задать строку подключения

Для пароля из .env.example:

```powershell
dotnet user-secrets set "ConnectionStrings:Database" "Host=localhost;Port=55432;Database=agent_observatory;Username=agent_observatory;Password=local-development-only;Timeout=3;Command Timeout=5" --project src/backend/AgentObservatory.Api
```

Если пароль в .env изменён, используйте тот же пароль в строке подключения. User Secrets хранятся вне репозитория и загружаются только в Development; это локальное хранилище, не production secret manager.

Для другого окружения задайте ConnectionStrings__Database через окружение процесса. Без строки подключения API завершится с понятной ошибкой конфигурации; с недоступной БД API запустится, но не будет готово принимать рабочие запросы.

## 3. Собрать и запустить

```powershell
dotnet restore AgentObservatory.sln
dotnet build AgentObservatory.sln --no-restore
dotnet run --project src/backend/AgentObservatory.Api --launch-profile http
```

## 4. Проверить в другом терминале

```powershell
curl.exe -i http://localhost:5080/health/live
curl.exe -i http://localhost:5080/health/ready
curl.exe -i http://localhost:5080/openapi/v1.json
```

| Адрес | Что проверяет | Ожидаемый ответ |
|---|---|---|
| /health/live | Процесс API отвечает; БД не проверяется | 200 Healthy |
| /health/ready | Есть подключение к PostgreSQL | 200 Healthy или 503 Unhealthy |
| /openapi/v1.json | OpenAPI-документ в Development | 200; пока без предметных endpoints |

Health checks намеренно возвращают краткий статус без строки подключения и деталей исключений. Readiness проверяет соединение, а не наличие будущих таблиц. Миграции автоматически не применяются, EnsureCreated не вызывается.

Проверка отказа и восстановления:

```powershell
docker compose stop postgres
curl.exe -i http://localhost:5080/health/live
curl.exe -i http://localhost:5080/health/ready
docker compose start postgres
# После готовности PostgreSQL readiness снова вернёт 200.
curl.exe -i http://localhost:5080/health/ready
```

Остановить API: Ctrl+C. Остановить контейнеры с сохранением данных: docker compose down. Не добавляйте -v, если хотите сохранить базу.

## Где писать следующий код

- Domain — предметные сущности и правила, без HTTP и EF Core.
- Application — сценарии работы приложения; зависит от Domain.
- Infrastructure — ObservatoryDbContext, EF Core и регистрация доступа к БД; зависит от Application.
- Api — контроллеры, HTTP и сборка зависимостей; подключает Application и Infrastructure.

Domain и Application пока не содержат предметного кода. Следующий шаг — согласовать формат событий и модель Projects/Runs, затем добавить первую миграцию и тесты реального сценария. Общую миграцию готовит один участник по договорённости команды.
