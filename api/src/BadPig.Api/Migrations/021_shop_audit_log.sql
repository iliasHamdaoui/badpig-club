-- Audit log of the board's actions on the shop (articles, variants, stock, handing over of orders). Owned by the Shop module.
-- No personal data about members: the subject is the name of an article or variant, the target an article/variant/order identifier.
-- Only the name of the board member who acts is kept (proof of who did what).
CREATE TABLE shop_audit_log (
    id          bigserial   PRIMARY KEY,
    occurred_at timestamptz NOT NULL,
    actor_id    uuid        NOT NULL,
    actor_name  text        NOT NULL,
    action      text        NOT NULL,
    target_id   uuid,
    subject     text,
    details     text
);

CREATE INDEX shop_audit_log_occurred_at_idx ON shop_audit_log (occurred_at DESC);

-- The log cannot be modified or erased from the application.
CREATE FUNCTION shop_audit_log_immutable() RETURNS trigger AS $$
BEGIN
    RAISE EXCEPTION 'The shop audit log can be neither modified nor erased';
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER shop_audit_log_immutable
    BEFORE UPDATE OR DELETE ON shop_audit_log
    FOR EACH ROW EXECUTE FUNCTION shop_audit_log_immutable();
