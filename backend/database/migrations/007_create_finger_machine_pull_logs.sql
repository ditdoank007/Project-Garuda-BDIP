BEGIN;

CREATE TABLE IF NOT EXISTS public.finger_machine_pull_logs
(
    id UUID PRIMARY KEY,

    finger_machine_id UUID NOT NULL,

    executed_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    read_count INTEGER NOT NULL DEFAULT 0,

    inserted_count INTEGER NOT NULL DEFAULT 0,

    skipped_count INTEGER NOT NULL DEFAULT 0,

    success BOOLEAN NOT NULL DEFAULT FALSE,

    error_message TEXT,

    CONSTRAINT fk_finger_pull_log_machine
        FOREIGN KEY (finger_machine_id)
        REFERENCES public.finger_machines(id)
        ON UPDATE CASCADE
        ON DELETE CASCADE,

    CONSTRAINT chk_finger_pull_log_read
        CHECK (read_count >= 0),

    CONSTRAINT chk_finger_pull_log_inserted
        CHECK (inserted_count >= 0),

    CONSTRAINT chk_finger_pull_log_skipped
        CHECK (skipped_count >= 0)
);

CREATE INDEX IF NOT EXISTS idx_finger_pull_logs_machine
ON public.finger_machine_pull_logs(finger_machine_id);

CREATE INDEX IF NOT EXISTS idx_finger_pull_logs_executed
ON public.finger_machine_pull_logs(executed_at);

CREATE INDEX IF NOT EXISTS idx_finger_pull_logs_machine_date
ON public.finger_machine_pull_logs(
    finger_machine_id,
    executed_at
);

CREATE INDEX IF NOT EXISTS idx_finger_pull_logs_success
ON public.finger_machine_pull_logs(success);

COMMIT;
