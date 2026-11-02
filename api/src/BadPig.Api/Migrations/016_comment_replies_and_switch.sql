-- Comments can be switched off per announcement (on by default, also for the existing ones), and a comment can answer another one
-- (one level). Deleting a comment deletes its answers.

ALTER TABLE announcements ADD COLUMN comments_enabled boolean NOT NULL DEFAULT true;

ALTER TABLE announcement_comments ADD COLUMN parent_id uuid NULL REFERENCES announcement_comments (id) ON DELETE CASCADE;
CREATE INDEX announcement_comments_parent_ix ON announcement_comments (parent_id);
