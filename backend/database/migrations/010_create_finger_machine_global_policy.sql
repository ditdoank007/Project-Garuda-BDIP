BEGIN;

CREATE TABLE IF NOT EXISTS public.finger_machine_global_policy
(
    id UUID PRIMARY KEY,

    pull_enabled BOOLEAN NOT NULL DEFAULT TRUE,

    pull_03_enabled BOOLEAN NOT NULL DEFAULT TRUE,
    pull_09_enabled BOOLEAN NOT NULL DEFAULT TRUE,
    pull_15_enabled BOOLEAN NOT NULL DEFAULT TRUE,
    pull_21_enabled BOOLEAN NOT NULL DEFAULT TRUE,

    clear_enabled BOOLEAN NOT NULL DEFAULT TRUE,

    clear_time TIME NOT NULL DEFAULT '04:00:00',

    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE UNIQUE INDEX IF NOT EXISTS uq_finger_machine_global_policy_singleton
ON public.finger_machine_global_policy ((TRUE));

COMMIT;
