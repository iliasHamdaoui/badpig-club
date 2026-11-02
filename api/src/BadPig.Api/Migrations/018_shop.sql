-- Shop module: articles (products) sold by the club, their variants (price + stock) and the stock history.
-- Orders module: the orders of the members and their lines.
-- Orders keep a copy of names and prices; order lines have no foreign key to the variant (they stay if the variant is deleted).
-- quantity = units in stock (reserved ones included); reserved_quantity = units held for orders waiting for their payment.

CREATE TABLE shop_products (
    id          uuid        PRIMARY KEY,
    name        text        NOT NULL CHECK (char_length(name) BETWEEN 1 AND 100),
    description text        NOT NULL DEFAULT '',
    is_active   boolean     NOT NULL DEFAULT true,
    created_at  timestamptz NOT NULL
);

CREATE TABLE shop_product_variants (
    id                  uuid        PRIMARY KEY,
    product_id          uuid        NOT NULL REFERENCES shop_products (id) ON DELETE CASCADE,
    label               text        NOT NULL CHECK (char_length(label) BETWEEN 1 AND 100),
    price_cents         integer     NOT NULL CHECK (price_cents BETWEEN 1 AND 100000),
    quantity            integer     NOT NULL CHECK (quantity >= 0),
    reserved_quantity   integer     NOT NULL DEFAULT 0 CHECK (reserved_quantity >= 0),
    low_stock_threshold integer     NOT NULL DEFAULT 0 CHECK (low_stock_threshold >= 0),
    is_active           boolean     NOT NULL DEFAULT true,
    created_at          timestamptz NOT NULL,
    CONSTRAINT shop_variants_reserved_within_stock CHECK (reserved_quantity <= quantity)
);

CREATE INDEX shop_product_variants_product_ix ON shop_product_variants (product_id);

CREATE TABLE shop_stock_movements (
    id          uuid        PRIMARY KEY,
    variant_id  uuid        NOT NULL REFERENCES shop_product_variants (id) ON DELETE CASCADE,
    change      integer     NOT NULL CHECK (change <> 0),
    reason      text        NOT NULL CHECK (reason IN ('restock', 'correction', 'sale')),
    note        text        NOT NULL DEFAULT '',
    order_id    uuid        NULL,
    author_id   uuid        NULL,
    author_name text        NOT NULL DEFAULT '',
    created_at  timestamptz NOT NULL
);

CREATE INDEX shop_stock_movements_variant_ix ON shop_stock_movements (variant_id, created_at DESC);
CREATE INDEX shop_stock_movements_author_ix ON shop_stock_movements (author_id);

CREATE TABLE orders (
    id                 uuid        PRIMARY KEY,
    buyer_id           uuid        NULL,
    buyer_name         text        NOT NULL,
    status             text        NOT NULL CHECK (status IN ('pending', 'paid', 'delivered', 'cancelled', 'expired')),
    total_cents        integer     NOT NULL CHECK (total_cents >= 0),
    created_at         timestamptz NOT NULL,
    expires_at         timestamptz NOT NULL,
    paid_at            timestamptz NULL,
    delivered_at       timestamptz NULL,
    payment_session_id text        NULL UNIQUE,
    checkout_url       text        NULL
);

CREATE INDEX orders_buyer_ix ON orders (buyer_id, created_at DESC);
CREATE INDEX orders_status_ix ON orders (status, expires_at);

CREATE TABLE order_lines (
    id               uuid    PRIMARY KEY,
    order_id         uuid    NOT NULL REFERENCES orders (id) ON DELETE CASCADE,
    variant_id       uuid    NULL,
    product_name     text    NOT NULL,
    variant_label    text    NOT NULL,
    unit_price_cents integer NOT NULL CHECK (unit_price_cents >= 0),
    quantity         integer NOT NULL CHECK (quantity > 0)
);

CREATE INDEX order_lines_order_ix ON order_lines (order_id);
CREATE INDEX order_lines_variant_ix ON order_lines (variant_id);
