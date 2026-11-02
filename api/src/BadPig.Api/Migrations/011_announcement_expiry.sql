-- End date of an announcement: once it has passed (Europe/Paris date), the announcement is hidden from visitors and
-- members, then deleted automatically with its files and likes. Existing announcements get 90 days after their creation.

ALTER TABLE announcements ADD COLUMN expires_on date NULL;

UPDATE announcements SET expires_on = ((created_at AT TIME ZONE 'Europe/Paris')::date + 90);

ALTER TABLE announcements ALTER COLUMN expires_on SET NOT NULL;

CREATE INDEX announcements_expires_ix ON announcements (expires_on);
