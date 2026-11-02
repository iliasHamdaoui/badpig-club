-- Federation licence number of a person (FFBaD), kept by the club. Owned by the Administration module.
-- Personal data: the row is erased with the account.
CREATE TABLE member_licences (
    account_id uuid        PRIMARY KEY,
    number     text        NOT NULL CHECK (char_length(number) BETWEEN 1 AND 20),
    updated_at timestamptz NOT NULL DEFAULT now()
);
