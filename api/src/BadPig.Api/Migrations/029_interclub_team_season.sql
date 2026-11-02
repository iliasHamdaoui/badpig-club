-- A team now belongs to a season of the club (table seasons) instead of carrying its label as free text.
ALTER TABLE interclub_teams ADD COLUMN season_id uuid REFERENCES seasons (id);

-- The teams whose label is the name of a season join that season.
UPDATE interclub_teams t SET season_id = s.id FROM seasons s WHERE s.name = t.season;

-- The others (a label typed differently) join the latest season that has started.
UPDATE interclub_teams SET season_id = (SELECT id FROM seasons ORDER BY starts_on DESC LIMIT 1) WHERE season_id IS NULL;

-- A team without a season cannot exist: the board creates the seasons before the teams.
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM interclub_teams WHERE season_id IS NULL) THEN
        RAISE EXCEPTION 'Des équipes ne peuvent être rattachées à aucune saison : créez les saisons du club, puis relancez.';
    END IF;
END $$;

ALTER TABLE interclub_teams ALTER COLUMN season_id SET NOT NULL;
ALTER TABLE interclub_teams DROP CONSTRAINT interclub_teams_season_name_key;
ALTER TABLE interclub_teams DROP COLUMN season;
ALTER TABLE interclub_teams ADD CONSTRAINT interclub_teams_season_id_name_key UNIQUE (season_id, name);
