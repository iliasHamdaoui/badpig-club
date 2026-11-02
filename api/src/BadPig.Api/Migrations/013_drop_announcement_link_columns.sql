-- Videos and social links are now links written in the text of the announcement (the site turns a lone YouTube or Vimeo link
-- into a player): the two dedicated columns are no longer used.

ALTER TABLE announcements DROP COLUMN video_urls;
ALTER TABLE announcements DROP COLUMN social_urls;
