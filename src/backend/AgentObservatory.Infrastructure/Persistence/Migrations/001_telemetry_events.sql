-- Local development migration 001. Apply explicitly; do not run on an unreviewed database.
BEGIN;
CREATE TABLE telemetry_events (
    project_id uuid NOT NULL,
    event_id uuid NOT NULL,
    run_id uuid NOT NULL,
    occurred_at timestamptz NOT NULL,
    received_at timestamptz NOT NULL DEFAULT now(),
    payload jsonb NOT NULL,
    CONSTRAINT pk_telemetry_events PRIMARY KEY (project_id, event_id)
);
CREATE INDEX ix_telemetry_events_run ON telemetry_events (project_id, run_id, occurred_at, event_id);
COMMIT;
