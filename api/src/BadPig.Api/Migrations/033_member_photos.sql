-- One small profile picture per person, kept in the database (saved with it): the lists never read the bytes.
-- No foreign key: the accounts live in the identity provider; the picture is erased with the account (IAccountErasure).
CREATE TABLE member_photos (
    account_id   uuid        NOT NULL,
    content_type text        NOT NULL,
    content      bytea       NOT NULL,
    updated_at   timestamptz NOT NULL,
    CONSTRAINT member_photos_pkey PRIMARY KEY (account_id)
);
