CREATE TABLE IF NOT EXISTS public.attendance_finger_templates
(
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),

    user_id UUID NOT NULL,

    finger_id VARCHAR(50) NOT NULL,

    fid SMALLINT NOT NULL,

    valid BOOLEAN NOT NULL DEFAULT TRUE,

    template_data BYTEA NOT NULL,

    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT fk_attendance_finger_template_user
        FOREIGN KEY (user_id)
        REFERENCES public.users(id)
        ON UPDATE CASCADE
        ON DELETE CASCADE,

    CONSTRAINT chk_attendance_finger_template_fid
        CHECK (fid >= 0),

    CONSTRAINT uq_attendance_finger_template_user_fid
        UNIQUE (user_id, fid)
);

CREATE INDEX IF NOT EXISTS idx_attendance_finger_templates_finger_id
ON public.attendance_finger_templates(finger_id);

CREATE INDEX IF NOT EXISTS idx_attendance_finger_templates_user
ON public.attendance_finger_templates(user_id);

CREATE INDEX IF NOT EXISTS idx_attendance_finger_templates_valid
ON public.attendance_finger_templates(valid);
