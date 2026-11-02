-- Interclub competition: the club's teams of the season (with their captain) and their fixtures. Owned by the Interclubs module.
CREATE TABLE interclub_teams (
    id         uuid        PRIMARY KEY,
    name       text        NOT NULL,
    season     text        NOT NULL,
    captain_id uuid,
    created_at timestamptz NOT NULL,
    CONSTRAINT interclub_teams_season_name_key UNIQUE (season, name)
);

CREATE TABLE interclub_fixtures (
    id         uuid        PRIMARY KEY,
    team_id    uuid        NOT NULL,
    played_on  date        NOT NULL,
    start_time time,
    opponent   text        NOT NULL,
    venue      text        NOT NULL,
    place      text,
    created_at timestamptz NOT NULL,
    CONSTRAINT interclub_fixtures_team_id_fkey FOREIGN KEY (team_id) REFERENCES interclub_teams (id) ON DELETE CASCADE
);

CREATE INDEX interclub_fixtures_team_id_played_on_idx ON interclub_fixtures (team_id, played_on);
