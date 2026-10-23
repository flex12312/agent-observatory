# Agent Observatory

Agent Observatory — система мониторинга, визуализации и анализа выполнения программных агентных систем.

Проект нужен разработчикам, которым важно понимать не только итоговый результат работы агента, но и весь процесс выполнения: какие агенты и подагенты участвовали, какие модели и инструменты вызывались, где возникали ошибки и повторные попытки, сколько времени занимали отдельные этапы и как различаются разные запуски.

## Основные возможности MVP

- проекты и запуски (Runs);
- структурированная телеметрия;
- HTTP API для приёма событий;
- .NET SDK для подключения приложений;
- агенты и подагенты;
- вызовы моделей и инструментов;
- ошибки и повторные попытки;
- длительность операций и статистика использования моделей;
- realtime-обновления;
- execution graph;
- timeline;
- поиск и фильтрация событий.

## Архитектура

```text
Agent / Agent Framework
        |
        | telemetry
        v
Agent Observatory Collector
        |
        v
PostgreSQL
        |
        v
ASP.NET Core API
        |
        +--> SignalR --> Web UI
```

Телеметрия передаётся асинхронно. Недоступность Agent Observatory не должна останавливать наблюдаемое приложение и не должна менять его основную логику.

## Статус и стек

Проект находится на этапе проектирования: текущая структура содержит документацию и заготовки каталогов. Перечисленные возможности — цели MVP.

Планируемый стек: C# / .NET 10 LTS, ASP.NET Core Web API, PostgreSQL, EF Core / Npgsql, SignalR; .NET SDK на Channels и BackgroundService. Тесты: xUnit, WebApplicationFactory, Testcontainers. Запуск: Docker Compose; CI: GitHub Actions. Небольшой demo UI: React / TypeScript.

Подробности и причины выбора: [архитектура](docs/ARCHITECTURE.md).

## Команда

**@IKSXXX и @flex12312 — два backend-разработчика.** Делим задачи по функциональности, меняемся направлениями и проверяем код друг друга. SDK и demo UI — часть общей работы.

- [Команда и правила разделения задач](docs/TEAM.md)
- [Актуальный план реализации с критериями готовности](docs/IMPLEMENTATION_PLAN.md)

## Управление проектом

- [Техническое задание — Issue #3](../../issues/3)
- [Исторический backlog — Issue #4](../../issues/4)
- [Техническое задание](docs/SPECIFICATION.md)
- [Архитектура](docs/ARCHITECTURE.md)
- [Roadmap](docs/ROADMAP.md)
- [Распределение работ](docs/TEAM.md)
- [Правила работы](CONTRIBUTING.md)
- [Конфигурация GitHub Project](docs/GITHUB_PROJECT.md)

## Workflow

```text
Backlog -> Ready -> In Progress -> Review -> Done
```

Рабочий прогресс и задачи ведутся через GitHub Issues.
