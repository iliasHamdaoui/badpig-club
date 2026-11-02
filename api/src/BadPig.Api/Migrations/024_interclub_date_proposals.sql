-- Proposals of dates for the next fixture of a team: the captain offers two dates or more, the adherents answer, the captain chooses one
-- (which creates the fixture). Owned by the Interclubs module.
CREATE TABLE interclub_date_proposals (
    id         uuid        PRIMARY KEY,
    team_id    uuid        NOT NULL,
    opponent   text        NOT NULL,
    venue      text        NOT NULL,
    place      text,
    fixture_id uuid,
    created_at timestamptz NOT NULL,
    CONSTRAINT interclub_date_proposals_team_id_fkey FOREIGN KEY (team_id) REFERENCES interclub_teams (id) ON DELETE CASCADE
);

CREATE INDEX interclub_date_proposals_team_id_idx ON interclub_date_proposals (team_id);

CREATE TABLE interclub_proposed_dates (
    id          uuid PRIMARY KEY,
    proposal_id uuid NOT NULL,
    played_on   date NOT NULL,
    start_time  time,
    CONSTRAINT interclub_proposed_dates_proposal_id_fkey FOREIGN KEY (proposal_id) REFERENCES interclub_date_proposals (id) ON DELETE CASCADE
);

CREATE INDEX interclub_proposed_dates_proposal_id_idx ON interclub_proposed_dates (proposal_id);

-- Personal data (who answered what): erased with the account.
CREATE TABLE interclub_date_answers (
    proposed_date_id uuid        NOT NULL,
    account_id       uuid        NOT NULL,
    answer           text        NOT NULL,
    answered_at      timestamptz NOT NULL,
    CONSTRAINT interclub_date_answers_pkey PRIMARY KEY (proposed_date_id, account_id),
    CONSTRAINT interclub_date_answers_proposed_date_id_fkey FOREIGN KEY (proposed_date_id) REFERENCES interclub_proposed_dates (id) ON DELETE CASCADE
);

CREATE INDEX interclub_date_answers_account_id_idx ON interclub_date_answers (account_id);
