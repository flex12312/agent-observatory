# Agent Observatory

Проект для просмотра работы агентов: какие вызовы выполнялись, сколько времени заняли и где возникли ошибки.

## Что есть сейчас

- Основа ASP.NET Core API на .NET 10.
- Подключение PostgreSQL через EF Core и запуск БД в Docker Compose.
- Проверки состояния приложения, OpenAPI и ответы об ошибках в формате ProblemDetails.
- Тесты API и доступности БД, сборка и тесты в GitHub Actions.
- Предложение формата событий: DTO и JSON-пример для обсуждения командой.

Приём событий, API проектов и запусков, SDK и интерфейс ещё не сделаны.

## Запуск

Нужны .NET SDK 10 и работающий Docker. При первом запуске настройте пароль и строку подключения по [инструкции](docs/LOCAL_DEVELOPMENT.md).

После настройки, из корня репозитория:

```powershell
docker compose up -d --wait
dotnet run --project src/backend/AgentObservatory.Api --launch-profile http
```

Проверки: `http://localhost:5080/health/live` и `/health/ready`. Описание API в Development: `/openapi/v1.json`.

Все тесты: `dotnet test AgentObservatory.sln`. Для теста PostgreSQL нужен Docker.

## Задачи и документация

- [План проекта](https://github.com/flex12312/agent-observatory/issues/4)
- [Формат событий](docs/EVENT_CONTRACT.md)
- [Архитектура](docs/ARCHITECTURE.md)
- [Работа с ветками и PR](CONTRIBUTING.md)