# Проверки backend

AgentObservatory.Api.Tests запускает API через WebApplicationFactory.

- ApiAvailabilityTests проверяет, что API продолжает отвечать при недоступной БД, а также OpenAPI и формат ошибок.
- DatabaseAvailabilityTests поднимает отдельный PostgreSQL через Testcontainers и проверяет готовность при запуске, отказе и восстановлении БД. Требуется Docker.

Все проверки: `dotnet test AgentObservatory.sln`.

Без Docker: `dotnet test AgentObservatory.sln --filter "Category!=Database"`.

Тесты не используют базу из Docker Compose или личные строки подключения. Связанные задачи: #5 и #46. Подробности: [локальный запуск](../docs/LOCAL_DEVELOPMENT.md).
