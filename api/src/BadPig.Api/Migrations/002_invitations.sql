-- Who invited each person. The row is deleted with the account (right to erasure);
-- the lasting trace of the invitation stays in the audit log (identifiers only).
CREATE TABLE invitations (
    cible_id  uuid        PRIMARY KEY,
    par_id    uuid        NOT NULL,
    par_nom   text        NOT NULL,
    date      timestamptz NOT NULL DEFAULT now()
);
