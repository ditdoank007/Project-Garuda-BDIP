BEGIN;

CREATE TABLE IF NOT EXISTS public.finger_machine_clear_logs
(
    id UUID PRIMARY KEY,

    finger_machine_id UUID NOT NULL,

    executed_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    cleared_count INTEGER,

    success BOOLEAN NOT NULL DEFAULT FALSE,

    error_message TEXT,

    CONSTRAINT fk_finger_clear_log_machine
        FOREIGN KEY (finger_machine_id)
        REFERENCES public.finger_machines(id)
        ON UPDATE CASCADE
        ON DELETE CASCADE,

    CONSTRAINT chk_finger_clear_log_count
        CHECK (
            cleared_count IS NULL
            OR cleared_count >= 0
        )
);

CREATE INDEX IF NOT EXISTS idx_finger_clear_logs_machine
ON public.finger_machine_clear_logs(finger_machine_id);

CREATE INDEX IF NOT EXISTS idx_finger_clear_logs_executed
ON public.finger_machine_clear_logs(executed_at);

CREATE INDEX IF NOT EXISTS idx_finger_clear_logs_machine_date
ON public.finger_machine_clear_logs(
    finger_machine_id,
    executed_at
);

CREATE INDEX IF NOT EXISTS idx_finger_clear_logs_success
ON public.finger_machine_clear_logs(success);

COMMIT;
