-- Renames tables, columns and constraints to English. Data is untouched.

-- Audit log
ALTER TABLE journal RENAME TO audit_log;
ALTER TABLE audit_log RENAME COLUMN date TO occurred_at;
ALTER TABLE audit_log RENAME COLUMN acteur_id TO actor_id;
ALTER TABLE audit_log RENAME COLUMN acteur_nom TO actor_name;
ALTER TABLE audit_log RENAME COLUMN cible_id TO target_id;
ALTER TABLE audit_log RENAME CONSTRAINT journal_pkey TO audit_log_pkey;
ALTER INDEX journal_date_idx RENAME TO audit_log_occurred_at_idx;
ALTER SEQUENCE journal_id_seq RENAME TO audit_log_id_seq;

DROP TRIGGER journal_immuable ON audit_log;
DROP FUNCTION journal_immuable();

CREATE FUNCTION audit_log_immutable() RETURNS trigger AS $$
BEGIN
    RAISE EXCEPTION 'The audit log cannot be modified or deleted';
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER audit_log_immutable
    BEFORE UPDATE OR DELETE ON audit_log
    FOR EACH ROW EXECUTE FUNCTION audit_log_immutable();

-- Invitations
ALTER TABLE invitations RENAME COLUMN cible_id TO target_id;
ALTER TABLE invitations RENAME COLUMN par_id TO invited_by_id;
ALTER TABLE invitations RENAME COLUMN par_nom TO invited_by_name;
ALTER TABLE invitations RENAME COLUMN date TO created_at;

-- Memberships
ALTER TABLE adhesions RENAME TO memberships;
ALTER TABLE memberships RENAME COLUMN membre_id TO member_id;
ALTER TABLE memberships RENAME COLUMN saison TO season;
ALTER TABLE memberships RENAME COLUMN validee_le TO validated_at;
ALTER TABLE memberships RENAME COLUMN validee_par_id TO validated_by_id;
ALTER TABLE memberships RENAME COLUMN validee_par_nom TO validated_by_name;
ALTER TABLE memberships RENAME CONSTRAINT adhesions_pkey TO memberships_pkey;

-- Board titles (the list)
ALTER TABLE titres RENAME TO titles;
ALTER TABLE titles RENAME COLUMN libelle TO label;
ALTER TABLE titles RENAME COLUMN cree_le TO created_at;
ALTER TABLE titles RENAME CONSTRAINT titres_pkey TO titles_pkey;
ALTER TABLE titles RENAME CONSTRAINT titres_libelle_check TO titles_label_check;
ALTER INDEX titres_libelle_uq RENAME TO titles_label_uq;

-- Board titles (who holds which)
ALTER TABLE titres_bureau RENAME TO board_titles;
ALTER TABLE board_titles RENAME COLUMN membre_id TO member_id;
ALTER TABLE board_titles RENAME COLUMN titre_id TO title_id;
ALTER TABLE board_titles RENAME CONSTRAINT titres_bureau_pkey TO board_titles_pkey;
ALTER TABLE board_titles RENAME CONSTRAINT titres_bureau_titre_uq TO board_titles_title_uq;
ALTER TABLE board_titles RENAME CONSTRAINT titres_bureau_titre_id_fkey TO board_titles_title_id_fkey;

-- Disabled accounts
ALTER TABLE desactivations RENAME TO disabled_accounts;
ALTER TABLE disabled_accounts RENAME COLUMN compte_id TO account_id;
ALTER TABLE disabled_accounts RENAME COLUMN depuis TO disabled_since;
ALTER TABLE disabled_accounts RENAME CONSTRAINT desactivations_pkey TO disabled_accounts_pkey;
