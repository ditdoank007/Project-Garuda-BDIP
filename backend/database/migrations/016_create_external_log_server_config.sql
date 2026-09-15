CREATE TABLE IF NOT EXISTS public.external_log_server_config
(
    id SMALLINT PRIMARY KEY DEFAULT 1,

    enabled BOOLEAN NOT NULL DEFAULT FALSE,

    server_address VARCHAR(255) NULL,

    port INTEGER NOT NULL DEFAULT 514,

    protocol VARCHAR(20) NOT NULL DEFAULT 'UDP',

    facility INTEGER NOT NULL DEFAULT 3,

    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT chk_external_log_server_config_singleton
        CHECK (id = 1),

    CONSTRAINT chk_external_log_server_config_port
        CHECK (port BETWEEN 1 AND 65535),

    CONSTRAINT chk_external_log_server_config_protocol
        CHECK (protocol IN ('UDP', 'TCP')),

    CONSTRAINT chk_external_log_server_config_facility
        CHECK (facility BETWEEN 0 AND 23)
);

INSERT INTO public.external_log_server_config
(
    id,
    enabled,
    server_address,
    port,
    protocol,
    facility
)
VALUES
(
    1,
    FALSE,
    NULL,
    514,
    'UDP',
    3
)
ON CONFLICT (id) DO NOTHING;
