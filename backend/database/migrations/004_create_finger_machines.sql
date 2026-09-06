BEGIN;

CREATE TABLE IF NOT EXISTS public.finger_machines
(
    id UUID PRIMARY KEY,

    code VARCHAR(50) NOT NULL UNIQUE,

    name VARCHAR(200) NOT NULL,

    ip_address VARCHAR(45) NOT NULL,

    port INTEGER NOT NULL DEFAULT 4370,

    location_id UUID,

    serial_number VARCHAR(100),

    device_name VARCHAR(200),

    is_active BOOLEAN NOT NULL DEFAULT TRUE,

    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT fk_finger_machines_location
        FOREIGN KEY (location_id)
        REFERENCES public.locations(id)
        ON UPDATE CASCADE
        ON DELETE SET NULL,

    CONSTRAINT chk_finger_machines_port
        CHECK (port > 0 AND port <= 65535)
);

CREATE INDEX IF NOT EXISTS idx_finger_machines_name
ON public.finger_machines(name);

CREATE INDEX IF NOT EXISTS idx_finger_machines_ip
ON public.finger_machines(ip_address);

CREATE INDEX IF NOT EXISTS idx_finger_machines_location
ON public.finger_machines(location_id);

CREATE INDEX IF NOT EXISTS idx_finger_machines_active
ON public.finger_machines(is_active);

COMMIT;
