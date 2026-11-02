-- One small picture per article, kept in the database (saved with it): the catalogue lists never read the bytes.
CREATE TABLE shop_product_images (
    product_id   uuid        NOT NULL,
    content_type text        NOT NULL,
    content      bytea       NOT NULL,
    updated_at   timestamptz NOT NULL,
    CONSTRAINT shop_product_images_pkey PRIMARY KEY (product_id),
    CONSTRAINT shop_product_images_product_id_fkey FOREIGN KEY (product_id) REFERENCES shop_products (id) ON DELETE CASCADE
);
