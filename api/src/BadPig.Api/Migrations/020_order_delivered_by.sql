-- Who handed an order over to the member (name copied, like the author of a stock movement; erased with the account of that board member).

ALTER TABLE orders ADD COLUMN delivered_by_id uuid NULL;
ALTER TABLE orders ADD COLUMN delivered_by_name text NULL;
