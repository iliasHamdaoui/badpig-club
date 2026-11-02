-- Announcements published by the board: internal (club members only) or public (any visitor of the site).
-- A new announcement is not published. The author is kept for the board only; when the author's account is deleted
-- the announcement stays, without author (author_id becomes NULL and the name is replaced).

CREATE TABLE announcements (
    id           uuid         PRIMARY KEY DEFAULT gen_random_uuid(),
    title        text         NOT NULL CHECK (char_length(title) BETWEEN 1 AND 150),
    body_html    text         NOT NULL,
    summary      text         NOT NULL DEFAULT '',
    kind         text         NOT NULL CHECK (kind IN ('internal', 'public')),
    is_published boolean      NOT NULL DEFAULT false,
    is_pinned    boolean      NOT NULL DEFAULT false,
    video_urls   text[]       NOT NULL DEFAULT '{}',
    author_id    uuid         NULL,
    author_name  text         NOT NULL,
    created_at   timestamptz  NOT NULL,
    updated_at   timestamptz  NOT NULL,
    published_at timestamptz  NULL
);

CREATE INDEX announcements_listing_ix ON announcements (is_published, is_pinned DESC, published_at DESC);
CREATE INDEX announcements_author_ix ON announcements (author_id);

-- Poster (at most one per announcement) and attachments; the content lives in the storage folder (volume), under storage_key.
CREATE TABLE announcement_files (
    id              uuid        PRIMARY KEY DEFAULT gen_random_uuid(),
    announcement_id uuid        NOT NULL REFERENCES announcements (id) ON DELETE CASCADE,
    role            text        NOT NULL CHECK (role IN ('poster', 'attachment')),
    file_name       text        NOT NULL CHECK (char_length(file_name) BETWEEN 1 AND 255),
    content_type    text        NOT NULL,
    size_bytes      bigint      NOT NULL CHECK (size_bytes >= 0),
    storage_key     text        NOT NULL UNIQUE,
    created_at      timestamptz NOT NULL
);

CREATE INDEX announcement_files_announcement_ix ON announcement_files (announcement_id);
CREATE UNIQUE INDEX announcement_files_poster_uq ON announcement_files (announcement_id) WHERE role = 'poster';

CREATE TABLE announcement_likes (
    announcement_id uuid        NOT NULL REFERENCES announcements (id) ON DELETE CASCADE,
    member_id       uuid        NOT NULL,
    liked_at        timestamptz NOT NULL,
    CONSTRAINT announcement_likes_pkey PRIMARY KEY (announcement_id, member_id)
);

CREATE INDEX announcement_likes_member_ix ON announcement_likes (member_id);
