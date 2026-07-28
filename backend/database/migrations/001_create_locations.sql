BEGIN;

-- ==========================================================
-- TABLE : location_types
-- ==========================================================

CREATE TABLE IF NOT EXISTS public.location_types
(
    code VARCHAR(50) PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    description TEXT,
    is_active BOOLEAN NOT NULL DEFAULT TRUE
);

-- ==========================================================
-- TABLE : locations
-- ==========================================================

CREATE TABLE IF NOT EXISTS public.locations
(
    id UUID PRIMARY KEY,

    code VARCHAR(50) NOT NULL UNIQUE,

    name VARCHAR(200) NOT NULL,

    location_type_code VARCHAR(50) NOT NULL,

    description TEXT,

    is_active BOOLEAN NOT NULL DEFAULT TRUE,

    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT fk_locations_location_type
        FOREIGN KEY (location_type_code)
        REFERENCES public.location_types(code)
);

-- ==========================================================
-- INDEXES
-- ==========================================================

CREATE INDEX IF NOT EXISTS idx_locations_code
ON public.locations(code);

CREATE INDEX IF NOT EXISTS idx_locations_name
ON public.locations(name);

CREATE INDEX IF NOT EXISTS idx_locations_type
ON public.locations(location_type_code);

-- ==========================================================
-- SEED DATA
-- ==========================================================

INSERT INTO public.location_types
(code, name, description)
VALUES
('HQ',  'Kantor Pusat',          'Basarnas Headquarters'),
('BD',  'Balai Diklat',          'Training Center'),
('UPT', 'Unit Pelaksana Teknis', 'Regional Office')
ON CONFLICT (code) DO NOTHING;

COMMIT;