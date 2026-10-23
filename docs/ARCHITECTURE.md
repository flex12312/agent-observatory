# Архитектура Agent Observatory

## Решение

Модульный монолит: один ASP.NET Core процесс содержит REST API, Collector и SignalR. SDK — отдельная библиотека в наблюдаемом приложении. PostgreSQL — основное хранилище.

```text
Observed application
  -> .NET SDK: bounded Channel -> background batch exporter
  -> HTTP ingestion in ASP.NET Core
  -> validation + transaction + PostgreSQL
  -> REST queries / SignalR notifications
  -> demo UI
```

HTTP доступен и без SDK. Успешный ответ ingestion выдаётся после фиксации транзакции. Очередь SDK находится в памяти: доставка при аварийном завершении процесса не гарантируется.

## Планируемый стек

| Область | Выбор |
|---|---|
| Backend | C#, .NET 10 LTS, ASP.NET Core Web API, controllers |
| Данные | PostgreSQL, EF Core 10, Npgsql совместимой версии |
| SDK | HttpClientFactory, System.Threading.Channels, BackgroundService |
| Realtime | ASP.NET Core SignalR |
| Доступ | ASP.NET Core Identity; cookie для UI; ключи проектов для ingestion |
| API | OpenAPI, DTO, ProblemDetails, валидация и ограничение размера запросов |
| Тесты | xUnit, WebApplicationFactory, Testcontainers с PostgreSQL |
| Окружение | Docker Compose, GitHub Actions |
| Demo UI | React, TypeScript |
| Диагностика | ILogger со структурированными полями, health checks |

Фиксируем SDK через global.json и конкретные версии пакетов при создании solution. Основание выбора .NET 10: [официальная политика поддержки](https://dotnet.microsoft.com/en-us/platform/support/policy).

## Структура solution

```text
src/backend/AgentObservatory.Api
src/backend/AgentObservatory.Application
src/backend/AgentObservatory.Domain
src/backend/AgentObservatory.Infrastructure
src/contracts/AgentObservatory.Contracts
src/sdk/AgentObservatory.Sdk
src/frontend
tests
samples
```

Domain не зависит от EF Core и HTTP. Application реализует сценарии и необходимые интерфейсы; Infrastructure — доступ к БД и внешним ресурсам. Api является composition root. Contracts содержит транспортные DTO с версией схемы; сущности EF не выдаются наружу. Внутри проектов группируем код по Projects, Runs, Telemetry и Access.

EF Core используется для записи и чтения; универсальный Generic Repository поверх DbContext не требуется. MediatR, отдельные микросервисы, брокер и Redis не входят в базовый MVP.

## Хранение и корреляция

- Projects, Runs, Events и данные доступа.
- EventId уникален в пределах проекта; уникальность обеспечивается БД.
- RunId, AgentId и ParentId связывают события; точную семантику ParentId и пар started/finished фиксируем в контракте до реализации.
- Timestamp — время источника в UTC; ReceivedAt — время приёма сервером. Длительность передаётся источником, чтобы не вычислять её по часам разных машин.
- Типизированные поля используются для фильтров; дополнительные Metadata — JSONB.
- Индексы проектируем по запросам: список Run по проекту и времени, события по Run и времени. Проверяем выбор через EXPLAIN ANALYZE на тестовом наборе.
- Материализованные представления ModelCall/ToolCall и агрегаты добавляем только при необходимости; события — исходные данные.

## Доставка и надёжность

SDK использует ограниченную очередь без ожидания сети в основном потоке. При переполнении новое событие отклоняется с диагностическим счётчиком; публичный API сообщает результат постановки. Размеры очереди, пакета, интервалы и timeout настраиваются.

Exporter повторяет transient failures с ограниченным backoff и jitter. Ошибки валидации и доступа не повторяются бесконечно. При shutdown выполняется flush с timeout. Потери при переполнении и crash документируются.

Collector валидирует весь пакет до записи. Валидный пакет сохраняется атомарно; повторные EventId игнорируются с отчётом о количестве дубликатов. Конкурентные запросы защищены уникальным индексом и conflict-safe записью, а не предварительной проверкой существования.

События могут приходить не по порядку. До появления родителя UI показывает незавершённую связь; порядок показа детерминирован по Timestamp и EventId. Завершённый Run допускает позднюю телеметрию в рамках документированной политики.

## Realtime

Уведомления отправляются после commit. SignalR не является хранилищем и не гарантирует доставку каждого уведомления. При подключении и reconnect клиент перечитывает состояние через REST; уведомления служат сигналом обновления. Подписка на группу Run требует проверки доступа к его проекту.

## Безопасность

Identity управляет пользователями; cookie UI требует защиты от CSRF для изменяющих запросов. По умолчанию UI и API публикуются с одного origin. Ключ проекта выдаётся однократно, хранится как хеш, может быть отозван и даёт только ingestion в свой проект.

Проверяем доступ к проекту во всех REST-запросах и SignalR-подписках. Ограничиваем batch size, payload size и частоту запросов. Секреты не коммитятся. Prompt/output по умолчанию не сохраняются; включение явно настраивается, чувствительные данные не попадают в логи.

## Проверка

Интеграционные тесты используют настоящий PostgreSQL в контейнере. Покрываем транзакции, конкурентные дубликаты, фильтры, изоляцию проектов и восстановление SDK после сбоя Collector. In-memory EF provider не заменяет эти проверки.
