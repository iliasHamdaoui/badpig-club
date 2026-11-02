-- How a match ended: played to the end (the default when sets are present), forfeit (not played, awarded to the other side) or retirement
-- (stopped during the play, awarded to the other side; the sets completed so far may be kept). "withdrawn" is the side that gave up.
ALTER TABLE interclub_fixture_matches
    ADD COLUMN ending    text,
    ADD COLUMN withdrawn text;
