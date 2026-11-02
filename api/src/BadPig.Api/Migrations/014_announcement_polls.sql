-- A poll attached to an announcement: only club members answer, one answer or several, with a closing date (Europe/Paris date,
-- the poll is open through that day). A member can change the answer until then; afterwards nothing changes and the results are shown.
-- Deleting the announcement (or the poll) deletes the options and the answers. Answers keep the member identifier even when the
-- poll is anonymous: anonymity means that the application never shows who answered what.

CREATE TABLE announcement_polls (
    announcement_id uuid    PRIMARY KEY REFERENCES announcements (id) ON DELETE CASCADE,
    allow_multiple  boolean NOT NULL,
    is_anonymous    boolean NOT NULL,
    closes_on       date    NOT NULL
);

CREATE TABLE announcement_poll_options (
    id              uuid PRIMARY KEY,
    announcement_id uuid NOT NULL REFERENCES announcement_polls (announcement_id) ON DELETE CASCADE,
    label           text NOT NULL CHECK (char_length(label) BETWEEN 1 AND 100),
    position        int  NOT NULL
);

CREATE INDEX announcement_poll_options_poll_ix ON announcement_poll_options (announcement_id);

CREATE TABLE announcement_poll_votes (
    option_id       uuid        NOT NULL REFERENCES announcement_poll_options (id) ON DELETE CASCADE,
    announcement_id uuid        NOT NULL REFERENCES announcement_polls (announcement_id) ON DELETE CASCADE,
    member_id       uuid        NOT NULL,
    voted_at        timestamptz NOT NULL,
    CONSTRAINT announcement_poll_votes_pkey PRIMARY KEY (option_id, member_id)
);

CREATE INDEX announcement_poll_votes_poll_ix ON announcement_poll_votes (announcement_id);
CREATE INDEX announcement_poll_votes_member_ix ON announcement_poll_votes (member_id);
