# Архитектура Agent Observatory

## Общая схема

```text
Observed Application
        |
        | events
        v
Integration Layer
 SDK / HTTP / OTLP
        |
        v
Collector
        |
        +--> Validation
        +--> Normalization
        +--> Correlation
        |
        v
PostgreSQL
        |
        v
Backend API
        |
        +--> REST
        +--> SignalR
        |
        v
Web UI
```

## Integration Layer

Первая версия:
- .NET SDK;
- HTTP API.

Следующий этап:
- OpenTelemetry/OTLP;
- адаптеры к внешним агентным средам.

## Collector

Collector принимает события, проверяет формат, приводит их к единой модели и связывает по RunId, AgentId и ParentId.

Требования:
- пакетная обработка;
- защита от повторной записи одного события;
- корректная работа с событиями, пришедшими с небольшой задержкой;
- минимальная задержка между событием и отображением в интерфейсе.

## Storage

PostgreSQL хранит:
- Projects;
- Runs;
- Agents;
- Events;
- ModelCalls;
- ToolCalls;
- Errors/Retry;
- агрегированную статистику.

## Backend API

ASP.NET Core предоставляет:
- управление проектами;
- чтение Run;
- поиск и фильтрацию;
- статистику;
- realtime-канал через SignalR.

## Frontend

React + TypeScript.

Основные экраны:
- Projects;
- Runs;
- Run Details;
- Execution Graph;
- Timeline;
- Model Calls;
- Tool Calls;
- Errors;
- Analytics.

## Надёжность

Agent Observatory не должен быть критической зависимостью наблюдаемого приложения.

```text
Observed app
   |
   +--> local non-blocking queue
            |
            +--> background batch exporter
                       |
                       +--> Collector
```

Если Collector недоступен, основное приложение продолжает выполнение.

## Корреляция

Основные идентификаторы:
- ProjectId;
- RunId;
- AgentId;
- EventId;
- ParentId.

ParentId позволяет восстановить дерево выполнения, Timestamp и Duration — временную шкалу.

## Конфиденциальность

Полные входные и выходные данные вызовов должны быть опциональными. Система должна уметь работать на метаданных без хранения чувствительного содержимого.
