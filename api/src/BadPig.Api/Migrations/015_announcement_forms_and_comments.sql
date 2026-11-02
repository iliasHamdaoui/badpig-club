-- An announcement is either a text or a poll (a standalone form of announcement, with its own question and choices).
-- Announcements that already had a poll become polls. Members comment announcements; a comment is deleted with its announcement.

ALTER TABLE announcements ADD COLUMN form text NOT NULL DEFAULT 'text' CHECK (form IN ('text', 'poll'));
UPDATE announcements SET form = 'poll' WHERE id IN (SELECT announcement_id FROM announcement_polls);

CREATE TABLE announcement_comments (
    id              uuid        PRIMARY KEY,
    announcement_id uuid        NOT NULL REFERENCES announcements (id) ON DELETE CASCADE,
    author_id       uuid        NOT NULL,
    author_name     text        NOT NULL,
    text            text        NOT NULL CHECK (char_length(text) BETWEEN 1 AND 1000),
    created_at      timestamptz NOT NULL
);

CREATE INDEX announcement_comments_announcement_ix ON announcement_comments (announcement_id, created_at);
CREATE INDEX announcement_comments_author_ix ON announcement_comments (author_id);
