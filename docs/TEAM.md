# Распределение работ

Проект разбит на три относительно независимых потока, чтобы команда могла работать параллельно.

## Поток A — Backend / Collector

**Текущий исполнитель:** @flex12312

Зона ответственности:
- ASP.NET Core backend;
- PostgreSQL и EF Core;
- HTTP ingestion;
- Run lifecycle;
- нормализация и хранение событий;
- SignalR/realtime;
- API чтения и поиска.

## Поток B — SDK / Telemetry

**Текущий исполнитель:** @IKSXXX

Зона ответственности:
- единая event-модель;
- .NET SDK;
- локальная очередь;
- background exporter;
- batch delivery;
- отказоустойчивость SDK;
- demo integration;
- подготовка OpenTelemetry-совместимости.

## Поток C — Frontend / Visualization

**Исполнитель:** третий участник будет добавлен позже.

Зона ответственности:
- React + TypeScript;
- список проектов и Run;
- Run Details;
- execution graph;
- timeline;
- details panel;
- filters/search UI;
- dashboard.

До добавления третьего GitHub-аккаунта задачи этого потока остаются без assignee.

## Общие задачи

Все участники:
- интеграционные тесты;
- review;
- документация;
- подготовка демонстрации;
- финальная стабилизация MVP.
