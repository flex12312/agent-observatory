# GitHub Project configuration

Для проекта рекомендуется одна доска: **Agent Observatory — MVP**.

## Status

Поля статуса:

1. **Backlog** — задача существует, но ещё не готова к началу;
2. **Ready** — все зависимости закрыты, задачу можно брать;
3. **In Progress** — идёт разработка;
4. **Review** — открыт Pull Request;
5. **Done** — изменение принято и Issue закрыт.

## Priority

Приоритет уже зафиксирован в заголовках Issues:

- **P0** — критический базовый контур;
- **P1** — обязательный MVP;
- **P2** — стабилизация / улучшения;
- **Post-MVP** — развитие после первой версии.

## Рекомендуемые views

### 1. MVP Board
Board grouped by Status.

Filter:

```text
is:open -title:"Post-MVP"
```

### 2. By Assignee
Table grouped by Assignee.

### 3. Post-MVP

Filter:

```text
is:open title:"Post-MVP"
```

## Поля

Минимально достаточно:

- Status;
- Assignee;
- Repository;
- Labels.

Отдельное поле Priority не обязательно: приоритет уже входит в название Issue и остаётся виден везде.

## Основной контроль прогресса

Даже без Projects-доски главным контрольным Issue остаётся:

- #4 — **[MVP] Backlog и прогресс проекта**.

Он содержит полный список обязательных задач и позволяет преподавателю быстро увидеть scope проекта.
