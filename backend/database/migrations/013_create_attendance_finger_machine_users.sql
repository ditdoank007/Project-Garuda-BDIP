CREATE TABLE IF NOT EXISTS public.attendance_finger_machine_users
(
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),

    finger_machine_id UUID NOT NULL,

    user_id UUID NOT NULL,

    finger_id VARCHAR(50) NOT NULL,

    device_uid INTEGER NOT NULL,

    device_user_id VARCHAR(50) NOT NULL,

    device_name VARCHAR(200),

    privilege SMALLINT NOT NULL DEFAULT 0,

    enabled BOOLEAN NOT NULL DEFAULT TRUE,

    last_seen_at TIMESTAMPTZ,

    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT fk_attendance_machine_user_machine
        FOREIGN KEY (finger_machine_id)
        REFERENCES public.finger_machines(id)
        ON UPDATE CASCADE
        ON DELETE CASCADE,

    CONSTRAINT fk_attendance_machine_user_user
        FOREIGN KEY (user_id)
        REFERENCES public.users(id)
        ON UPDATE CASCADE
        ON DELETE CASCADE,

    CONSTRAINT chk_attendance_machine_user_device_uid
        CHECK (device_uid >= 0),

    CONSTRAINT uq_attendance_machine_user_machine_user
        UNIQUE (finger_machine_id, user_id),

    CONSTRAINT uq_attendance_machine_user_machine_device_uid
        UNIQUE (finger_machine_id, device_uid)
);

CREATE INDEX IF NOT EXISTS idx_attendance_machine_users_user
ON public.attendance_finger_machine_users(user_id);

CREATE INDEX IF NOT EXISTS idx_attendance_machine_users_machine
ON public.attendance_finger_machine_users(finger_machine_id);

CREATE INDEX IF NOT EXISTS idx_attendance_machine_users_finger_id
ON public.attendance_finger_machine_users(finger_id);

CREATE INDEX IF NOT EXISTS idx_attendance_machine_users_enabled
ON public.attendance_finger_machine_users(enabled);
