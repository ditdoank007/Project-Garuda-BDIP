BEGIN;

CREATE TABLE IF NOT EXISTS public.finger_machine_sync_logs
(
    id UUID PRIMARY KEY,

    finger_machine_id UUID NOT NULL,

    executed_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    device_time_before TIMESTAMPTZ,

    server_time TIMESTAMPTZ,

    device_time_after TIMESTAMPTZ,

    success BOOLEAN NOT NULL DEFAULT FALSE,

    error_message TEXT,

    CONSTRAINT fk_finger_sync_log_machine
        FOREIGN KEY (finger_machine_id)
        REFERENCES public.finger_machines(id)
        ON UPDATE CASCADE
        ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_finger_sync_logs_machine
ON public.finger_machine_sync_logs(finger_machine_id);

CREATE INDEX IF NOT EXISTS idx_finger_sync_logs_executed
ON public.finger_machine_sync_logs(executed_at);

CREATE INDEX IF NOT EXISTS idx_finger_sync_logs_machine_date
ON public.finger_machine_sync_logs(
    finger_machine_id,
    executed_at
);

CREATE INDEX IF NOT EXISTS idx_finger_sync_logs_success
ON public.finger_machine_sync_logs(success);

COMMIT;
