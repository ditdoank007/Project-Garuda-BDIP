BEGIN;

CREATE TABLE IF NOT EXISTS public.finger_machine_runtime
(
    finger_machine_id UUID PRIMARY KEY,

    is_online BOOLEAN NOT NULL DEFAULT FALSE,

    serial_number VARCHAR(100),

    device_name VARCHAR(200),

    last_seen_at TIMESTAMPTZ,

    last_time_sync_at TIMESTAMPTZ,

    last_pull_at TIMESTAMPTZ,

    last_clear_at TIMESTAMPTZ,

    last_error_at TIMESTAMPTZ,

    last_error TEXT,

    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT fk_finger_runtime_machine
        FOREIGN KEY (finger_machine_id)
        REFERENCES public.finger_machines(id)
        ON UPDATE CASCADE
        ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_finger_runtime_online
ON public.finger_machine_runtime(is_online);

CREATE INDEX IF NOT EXISTS idx_finger_runtime_last_seen
ON public.finger_machine_runtime(last_seen_at);

CREATE INDEX IF NOT EXISTS idx_finger_runtime_last_pull
ON public.finger_machine_runtime(last_pull_at);

CREATE INDEX IF NOT EXISTS idx_finger_runtime_last_sync
ON public.finger_machine_runtime(last_time_sync_at);

CREATE INDEX IF NOT EXISTS idx_finger_runtime_last_clear
ON public.finger_machine_runtime(last_clear_at);

COMMIT;
