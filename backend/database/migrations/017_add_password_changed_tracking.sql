BEGIN;

ALTER TABLE public.users
    ADD COLUMN IF NOT EXISTS password_changed_at TIMESTAMPTZ NULL;

CREATE INDEX IF NOT EXISTS idx_users_password_changed_at
    ON public.users(password_changed_at);

COMMIT;
