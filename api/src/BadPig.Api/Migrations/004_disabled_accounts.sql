-- Disabling date of each disabled account: an account disabled for more than a month is deleted automatically.
-- The row disappears when the account is re-enabled or deleted.
CREATE TABLE desactivations (
    compte_id uuid        PRIMARY KEY,
    depuis    timestamptz NOT NULL DEFAULT now()
);
