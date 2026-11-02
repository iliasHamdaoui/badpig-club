-- The membership operations are written to the administration audit log (audit_log): a separate table is not needed.
-- (Harmless if the table was never created.)
DROP TABLE IF EXISTS membership_audit_log;
DROP FUNCTION IF EXISTS membership_audit_log_immutable();
