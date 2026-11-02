-- Memberships: one row per person and per season (season N runs from 1 September N to 31 August N+1).
-- No amount or payment method is kept: only the validated season, the date and the board member.
-- Rows are deleted with the account (right to erasure); the lasting trace is in the audit log.
CREATE TABLE adhesions (
    membre_id     uuid        NOT NULL,
    saison        integer     NOT NULL,
    validee_le    timestamptz NOT NULL DEFAULT now(),
    validee_par_id  uuid      NOT NULL,
    validee_par_nom text      NOT NULL,
    PRIMARY KEY (membre_id, saison)
);

-- Board titles: a person has at most one title, and a title has only one holder.
CREATE TABLE titres_bureau (
    membre_id uuid PRIMARY KEY,
    titre     text NOT NULL UNIQUE CHECK (titre IN ('president', 'secretaire', 'tresorier'))
);
