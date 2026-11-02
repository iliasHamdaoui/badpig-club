-- The board is e-mailed once when a variant falls to its alert threshold. low_stock_alerted remembers that it was done,
-- until the stock is back above the threshold (restock, correction, released order, new threshold).

ALTER TABLE shop_product_variants ADD COLUMN low_stock_alerted boolean NOT NULL DEFAULT false;
