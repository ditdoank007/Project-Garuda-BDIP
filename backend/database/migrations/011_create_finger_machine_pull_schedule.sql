CREATE TABLE IF NOT EXISTS public.finger_machine_pull_schedule
(
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),

    pull_time TIME NOT NULL,

    is_active BOOLEAN NOT NULL DEFAULT TRUE,

    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT uq_finger_pull_schedule_time
        UNIQUE (pull_time)
);


CREATE INDEX IF NOT EXISTS idx_finger_pull_schedule_active
ON public.finger_machine_pull_schedule(is_active);


INSERT INTO public.finger_machine_pull_schedule
(
    pull_time
)
VALUES
('03:00'),
('09:00'),
('15:00'),
('21:00')
ON CONFLICT (pull_time)
DO NOTHING;
