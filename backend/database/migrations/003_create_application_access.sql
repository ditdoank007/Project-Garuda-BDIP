CREATE TABLE IF NOT EXISTS public.application_access (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),

    user_id uuid NOT NULL,
    application_id uuid NOT NULL,

    is_active boolean NOT NULL DEFAULT true,

    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now(),

    CONSTRAINT fk_application_access_user
        FOREIGN KEY (user_id)
        REFERENCES public.users(id)
        ON UPDATE CASCADE
        ON DELETE CASCADE,

    CONSTRAINT fk_application_access_application
        FOREIGN KEY (application_id)
        REFERENCES public.applications(id)
        ON UPDATE CASCADE
        ON DELETE CASCADE,

    CONSTRAINT uq_application_access_user_application
        UNIQUE (user_id, application_id)
);

CREATE INDEX IF NOT EXISTS idx_application_access_user
    ON public.application_access(user_id);

CREATE INDEX IF NOT EXISTS idx_application_access_application
    ON public.application_access(application_id);

CREATE INDEX IF NOT EXISTS idx_application_access_active
    ON public.application_access(is_active);
