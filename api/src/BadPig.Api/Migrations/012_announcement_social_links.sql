-- Links to Instagram or Facebook posts of the club, shown by the site as link cards.

ALTER TABLE announcements ADD COLUMN social_urls text[] NOT NULL DEFAULT '{}';
