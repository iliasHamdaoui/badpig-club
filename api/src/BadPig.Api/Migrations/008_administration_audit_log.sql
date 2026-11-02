-- The audit table belongs to the Administration module: its name says so.
ALTER TABLE audit_log RENAME TO administration_audit_log;
ALTER TABLE administration_audit_log RENAME CONSTRAINT audit_log_pkey TO administration_audit_log_pkey;
ALTER INDEX audit_log_occurred_at_idx RENAME TO administration_audit_log_occurred_at_idx;
ALTER SEQUENCE audit_log_id_seq RENAME TO administration_audit_log_id_seq;

DROP TRIGGER audit_log_immutable ON administration_audit_log;
DROP FUNCTION audit_log_immutable();

CREATE FUNCTION administration_audit_log_immutable() RETURNS trigger AS $$
BEGIN
    RAISE EXCEPTION 'The audit log cannot be modified or deleted';
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER administration_audit_log_immutable
    BEFORE UPDATE OR DELETE ON administration_audit_log
    FOR EACH ROW EXECUTE FUNCTION administration_audit_log_immutable();
