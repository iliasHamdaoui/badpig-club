-- The result of a match is now the score of its sets (a set is won at 15 points, 3 sets at most, 2 sets win the match):
-- the outcome is computed from them, so the stored outcome and the free-text score are replaced by the points of each set.
-- (Nothing was entered with the previous columns: the result entry did not exist before this script.)
ALTER TABLE interclub_fixture_matches
    DROP COLUMN outcome,
    DROP COLUMN score,
    ADD COLUMN set1_club     smallint,
    ADD COLUMN set1_opponent smallint,
    ADD COLUMN set2_club     smallint,
    ADD COLUMN set2_opponent smallint,
    ADD COLUMN set3_club     smallint,
    ADD COLUMN set3_opponent smallint;
