CREATE TABLE IF NOT EXISTS public.attendance_synchronization_schedule
(
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),

    is_enabled BOOLEAN NOT NULL DEFAULT FALSE,

    frequency VARCHAR(20) NOT NULL DEFAULT 'DAILY',

    sync_time TIME NOT NULL DEFAULT '02:00',

    weekday SMALLINT NULL,

    day_of_month SMALLINT NULL,

    last_started_at TIMESTAMPTZ NULL,

    last_finished_at TIMESTAMPTZ NULL,

    last_success BOOLEAN NULL,

    last_message TEXT NULL,

    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT chk_attendance_sync_schedule_frequency
        CHECK (frequency IN ('DAILY', 'WEEKLY', 'MONTHLY')),

    CONSTRAINT chk_attendance_sync_schedule_weekday
        CHECK (weekday IS NULL OR weekday BETWEEN 1 AND 7),

    CONSTRAINT chk_attendance_sync_schedule_day_of_month
        CHECK (day_of_month IS NULL OR day_of_month BETWEEN 1 AND 31)
);

CREATE UNIQUE INDEX IF NOT EXISTS uq_attendance_sync_schedule_single
ON public.attendance_synchronization_schedule ((TRUE));

INSERT INTO public.attendance_synchronization_schedule
(
    is_enabled,
    frequency,
    sync_time
)
VALUES
(
    FALSE,
    'DAILY',
    '02:00'
)
ON CONFLICT DO NOTHING;
