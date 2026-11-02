-- Audit log of the board's actions on accounts.
-- Only Authentik identifiers (UUID) are kept for the people concerned: no name or e-mail,
-- so that deleting an account (right to erasure) leaves no personal data here.
-- Only the name of the board member who acts is kept (proof of who did what).
CREATE TABLE journal (
    id         bigserial   PRIMARY KEY,
    date       timestamptz NOT NULL DEFAULT now(),
    acteur_id  uuid        NOT NULL,
    acteur_nom text        NOT NULL,
    action     text        NOT NULL,
    cible_id   uuid
);

CREATE INDEX journal_date_idx ON journal (date DESC);

-- The log cannot be modified or erased from the application.
CREATE FUNCTION journal_immuable() RETURNS trigger AS $$
BEGIN
    RAISE EXCEPTION 'Le journal ne peut être ni modifié ni effacé';
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER journal_immuable
    BEFORE UPDATE OR DELETE ON journal
    FOR EACH ROW EXECUTE FUNCTION journal_immuable();
