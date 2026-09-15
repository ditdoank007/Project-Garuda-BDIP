CREATE TABLE IF NOT EXISTS public.audit_logs
(
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),

    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    username VARCHAR(100) NULL,

    full_name VARCHAR(200) NULL,

    role VARCHAR(100) NULL,

    action VARCHAR(100) NOT NULL,

    module VARCHAR(100) NOT NULL,

    target VARCHAR(255) NULL,

    result VARCHAR(20) NOT NULL DEFAULT 'SUCCESS',

    ip_address VARCHAR(64) NULL,

    user_agent TEXT NULL,

    details TEXT NULL,

    CONSTRAINT chk_audit_logs_result
        CHECK (result IN ('SUCCESS', 'DENIED', 'FAILED'))
);

CREATE INDEX IF NOT EXISTS ix_audit_logs_created_at
ON public.audit_logs (created_at DESC);

CREATE INDEX IF NOT EXISTS ix_audit_logs_username
ON public.audit_logs (username);

CREATE INDEX IF NOT EXISTS ix_audit_logs_module
ON public.audit_logs (module);

CREATE INDEX IF NOT EXISTS ix_audit_logs_action
ON public.audit_logs (action);

CREATE INDEX IF NOT EXISTS ix_audit_logs_result
ON public.audit_logs (result);
