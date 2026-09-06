BEGIN;

CREATE TABLE IF NOT EXISTS public.finger_machine_collection_policies
(
    id UUID PRIMARY KEY,

    finger_machine_id UUID NOT NULL UNIQUE,

    interval_minutes INTEGER NOT NULL DEFAULT 360,

    is_enabled BOOLEAN NOT NULL DEFAULT TRUE,

    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT fk_finger_collection_policy_machine
        FOREIGN KEY (finger_machine_id)
        REFERENCES public.finger_machines(id)
        ON UPDATE CASCADE
        ON DELETE CASCADE,

    CONSTRAINT chk_finger_collection_interval
        CHECK (interval_minutes > 0)
);

CREATE TABLE IF NOT EXISTS public.finger_machine_time_sync_policies
(
    id UUID PRIMARY KEY,

    finger_machine_id UUID NOT NULL UNIQUE,

    interval_minutes INTEGER NOT NULL DEFAULT 5,

    is_enabled BOOLEAN NOT NULL DEFAULT TRUE,

    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT fk_finger_time_sync_policy_machine
        FOREIGN KEY (finger_machine_id)
        REFERENCES public.finger_machines(id)
        ON UPDATE CASCADE
        ON DELETE CASCADE,

    CONSTRAINT chk_finger_time_sync_interval
        CHECK (interval_minutes > 0)
);

CREATE INDEX IF NOT EXISTS idx_finger_collection_policy_enabled
ON public.finger_machine_collection_policies(is_enabled);

CREATE INDEX IF NOT EXISTS idx_finger_time_sync_policy_enabled
ON public.finger_machine_time_sync_policies(is_enabled);

COMMIT;
