BEGIN;

-- ==========================================================
-- TABLE : applications
-- ==========================================================

CREATE TABLE IF NOT EXISTS public.applications
(
    id UUID PRIMARY KEY,
    code VARCHAR(50) NOT NULL UNIQUE,
    name VARCHAR(200) NOT NULL,
    description TEXT,
    base_url VARCHAR(255) NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- ==========================================================
-- INDEXES
-- ==========================================================

CREATE INDEX IF NOT EXISTS idx_applications_name
ON public.applications(name);

-- ==========================================================
-- SEED DATA
-- ==========================================================

INSERT INTO public.applications
(
    id,
    code,
    name,
    description,
    base_url,
    is_active
)
VALUES
(
    '00000000-0000-0000-0000-000000000001',
    'GEOSPASIAL',
    'Geospasial',
    'Geospatial application',
    'https://geospasial.sarsurabaya.id',
    TRUE
),
(
    '00000000-0000-0000-0000-000000000002',
    'URBAN_SAR',
    'Urban SAR',
    'Urban SAR operational application',
    'https://usar.sarsurabaya.id',
    TRUE
),
(
    '00000000-0000-0000-0000-000000000003',
    'HRIS',
    'HRIS',
    'Human Resources Information System',
    'https://hris.sarsurabaya.id',
    TRUE
)
ON CONFLICT (code) DO NOTHING;

COMMIT;
