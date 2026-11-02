-- School holidays of a season: the club closes, the site says there is no session. Owned by the Administration module.
CREATE TABLE school_holidays (
    id         uuid        PRIMARY KEY DEFAULT gen_random_uuid(),
    season_id  uuid        NOT NULL REFERENCES seasons (id) ON DELETE CASCADE,
    name       text        NOT NULL CHECK (char_length(name) BETWEEN 1 AND 60),
    starts_on  date        NOT NULL,
    ends_on    date        NOT NULL,
    created_at timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT school_holidays_dates_check CHECK (ends_on >= starts_on)
);

CREATE INDEX school_holidays_season_id_idx ON school_holidays (season_id, starts_on);
