-- Players of an interclub team (chosen by its captain). Only they can answer the proposals of dates of the team. Owned by the Interclubs module.
-- Personal data (who plays in which team): erased with the account.
CREATE TABLE interclub_team_members (
    team_id    uuid        NOT NULL,
    account_id uuid        NOT NULL,
    added_at   timestamptz NOT NULL,
    CONSTRAINT interclub_team_members_pkey PRIMARY KEY (team_id, account_id),
    CONSTRAINT interclub_team_members_team_id_fkey FOREIGN KEY (team_id) REFERENCES interclub_teams (id) ON DELETE CASCADE
);

CREATE INDEX interclub_team_members_account_id_idx ON interclub_team_members (account_id);
