-- Board titles become a list that the board manages itself (create, rename, delete).
-- The three existing titles are carried over; assignments already made are kept.
CREATE TABLE titres (
    id      uuid        PRIMARY KEY DEFAULT gen_random_uuid(),
    libelle text        NOT NULL CHECK (char_length(libelle) BETWEEN 1 AND 60),
    cree_le timestamptz NOT NULL DEFAULT now()
);

-- Two titles cannot have the same name (case ignored).
CREATE UNIQUE INDEX titres_libelle_uq ON titres (lower(libelle));

INSERT INTO titres (libelle) VALUES ('Président(e)'), ('Secrétaire'), ('Trésorier(e)');

ALTER TABLE titres_bureau ADD COLUMN titre_id uuid REFERENCES titres (id) ON DELETE CASCADE;

UPDATE titres_bureau tb SET titre_id = t.id FROM titres t
WHERE (tb.titre = 'president'  AND t.libelle = 'Président(e)')
   OR (tb.titre = 'secretaire' AND t.libelle = 'Secrétaire')
   OR (tb.titre = 'tresorier'  AND t.libelle = 'Trésorier(e)');

ALTER TABLE titres_bureau ALTER COLUMN titre_id SET NOT NULL;
ALTER TABLE titres_bureau DROP COLUMN titre;
ALTER TABLE titres_bureau ADD CONSTRAINT titres_bureau_titre_uq UNIQUE (titre_id);
