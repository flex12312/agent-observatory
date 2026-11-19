-- Destructive: removes all stored telemetry. Local development only.
BEGIN;
DROP TABLE telemetry_events;
COMMIT;
