-- Seasons become real data managed by the board: dates, grace period and the two yearly fees.
-- Memberships now point to a season, record the kind (internal = licensed by the club, external = licensed elsewhere)
-- and the amount of the fee at the time of the validation (so the statistics stay right if a fee changes later).

CREATE TABLE seasons (
    id           uuid         PRIMARY KEY DEFAULT gen_random_uuid(),
    name         text         NOT NULL CHECK (char_length(name) BETWEEN 1 AND 60),
    starts_on    date         NOT NULL,
    ends_on      date         NOT NULL,
    grace_days   integer      NOT NULL DEFAULT 30 CHECK (grace_days BETWEEN 0 AND 120),
    internal_fee numeric(8,2) NOT NULL DEFAULT 0 CHECK (internal_fee >= 0),
    external_fee numeric(8,2) NOT NULL DEFAULT 0 CHECK (external_fee >= 0),
    created_at   timestamptz  NOT NULL DEFAULT now(),
    CONSTRAINT seasons_dates_check CHECK (ends_on > starts_on)
);

CREATE UNIQUE INDEX seasons_name_uq ON seasons (lower(name));

-- The seasons already used by memberships (1 September N to 31 August N+1), plus the current one (Europe/Paris time).
-- Fees start at 0: the board fills them in.
INSERT INTO seasons (name, starts_on, ends_on)
SELECT y || '-' || (y + 1), make_date(y, 9, 1), make_date(y + 1, 8, 31)
FROM (
    SELECT DISTINCT season AS y FROM memberships
    UNION
    SELECT (CASE WHEN EXTRACT(MONTH FROM (now() AT TIME ZONE 'Europe/Paris')) >= 9
                 THEN EXTRACT(YEAR FROM (now() AT TIME ZONE 'Europe/Paris'))
                 ELSE EXTRACT(YEAR FROM (now() AT TIME ZONE 'Europe/Paris')) - 1 END)::integer
) s;

ALTER TABLE memberships ADD COLUMN season_id uuid;
ALTER TABLE memberships ADD COLUMN kind text NOT NULL DEFAULT 'internal' CHECK (kind IN ('internal', 'external'));
ALTER TABLE memberships ADD COLUMN amount numeric(8,2) NOT NULL DEFAULT 0 CHECK (amount >= 0);

UPDATE memberships m SET season_id = s.id FROM seasons s WHERE s.starts_on = make_date(m.season, 9, 1);

ALTER TABLE memberships ALTER COLUMN season_id SET NOT NULL;
ALTER TABLE memberships DROP CONSTRAINT memberships_pkey;
ALTER TABLE memberships DROP COLUMN season;
ALTER TABLE memberships ADD CONSTRAINT memberships_pkey PRIMARY KEY (member_id, season_id);
ALTER TABLE memberships ADD CONSTRAINT memberships_season_id_fkey FOREIGN KEY (season_id) REFERENCES seasons (id);
CREATE INDEX memberships_season_id_idx ON memberships (season_id);

-- Kind of membership chosen when the person is invited (the board can change it when validating the membership).
ALTER TABLE invitations ADD COLUMN membership_kind text NOT NULL DEFAULT 'internal' CHECK (membership_kind IN ('internal', 'external'));
