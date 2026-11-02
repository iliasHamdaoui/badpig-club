-- A comment can quote any earlier comment of the same announcement (chat style), not only a top-level one. Deleting the quoted
-- comment no longer deletes the messages that quote it: they show "message deleted". parent_id therefore stops being a foreign key.

ALTER TABLE announcement_comments DROP CONSTRAINT announcement_comments_parent_id_fkey;
