-- A licence number belongs to one person: two accounts may not carry the same one.
-- Upper and lower case are the same number, so the index is built on the upper-case form.
-- Before the index can exist, any duplicate already recorded has to go: the oldest row keeps the number,
-- the others lose it (the board enters them again from the member list).
DELETE FROM member_licences a
USING member_licences b
WHERE upper(a.number) = upper(b.number)
  AND (a.updated_at > b.updated_at OR (a.updated_at = b.updated_at AND a.account_id > b.account_id));

CREATE UNIQUE INDEX ux_member_licences_number ON member_licences (upper(number));
