-- The 7 matches of a fixture (SD, MX1, MX2, DD, DH, SH1, SH2): who plays them (chosen by the captain) and how they ended.
-- One row per fixture and slot. Personal data (who plays): erased with the account. Owned by the Interclubs module.
CREATE TABLE interclub_fixture_matches (
    fixture_id uuid NOT NULL,
    slot       text NOT NULL,
    player1_id uuid,
    player2_id uuid,
    outcome    text,
    score      text,
    CONSTRAINT interclub_fixture_matches_pkey PRIMARY KEY (fixture_id, slot),
    CONSTRAINT interclub_fixture_matches_fixture_id_fkey FOREIGN KEY (fixture_id) REFERENCES interclub_fixtures (id) ON DELETE CASCADE
);

CREATE INDEX interclub_fixture_matches_player1_id_idx ON interclub_fixture_matches (player1_id);
CREATE INDEX interclub_fixture_matches_player2_id_idx ON interclub_fixture_matches (player2_id);
