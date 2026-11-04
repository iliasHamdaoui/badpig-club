-- Anonymous usage statistics of the website: for each day and each page (route template such as /announcements/:id),
-- how many times it was opened and how many seconds it stayed visible. No account, no address, no identifier is kept.
CREATE TABLE page_usage (
    day     date   NOT NULL,
    path    text   NOT NULL CHECK (char_length(path) BETWEEN 1 AND 100),
    views   int    NOT NULL DEFAULT 0 CHECK (views >= 0),
    seconds bigint NOT NULL DEFAULT 0 CHECK (seconds >= 0),
    CONSTRAINT page_usage_pkey PRIMARY KEY (day, path)
);
