# Распределение работ

Проект разбит на три потока, чтобы команда могла работать параллельно.

## Поток A — Backend / Collector

**Текущий исполнитель:** @IKSXXX

Зона ответственности:
- ASP.NET Core backend;
- PostgreSQL и EF Core;
- Projects / Runs API;
- HTTP ingestion;
- единая event-модель;
- нормализация и хранение событий;
- дедупликация;
- SignalR / realtime;
- API поиска, фильтрации и агрегатов;
- локальная инфраструктура.

Основные Issues: #7, #13, #15, #17, #19, #21, #23, #24, #25.

## Поток B — Frontend / Visualization

**Текущий исполнитель:** @flex12312

Зона ответственности:
- React + TypeScript;
- Projects / Runs screens;
- Live Run Details;
- Execution Graph;
- Timeline;
- панели деталей событий;
- поиск и фильтры;
- Analytics dashboard;
- базовый CI frontend/backend build.

Основные Issues: #26, #27, #28, #29, #30, #31, #32, #33, #49.

## Поток C — SDK / Telemetry

**Исполнитель:** третий участник будет добавлен позже.

Зона ответственности:
- публичные contracts телеметрии;
- .NET SDK;
- локальная очередь;
- background exporter;
- batch delivery;
- отказоустойчивость SDK;
- graceful flush;
- оценка overhead.

Основные Issues: #12, #34, #36, #38, #40, #54.

До добавления третьего GitHub-аккаунта задачи этого потока остаются без assignee.

## Общие задачи

Все участники участвуют в:
- demo integration;
- end-to-end tests;
- documentation;
- code review;
- подготовке защиты;
- финальной стабилизации MVP.

Основные общие Issues: #22, #46, #47, #48, #56.

## Приоритеты

- **P0** — обязательный базовый контур;
- **P1** — обязательный полноценный MVP;
- **P2** — стабилизация и улучшения;
- **Post-MVP** — перспективы, не блокирующие первую версию.
