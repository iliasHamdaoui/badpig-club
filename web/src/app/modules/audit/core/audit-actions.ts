export interface AuditEntry {
  id: number;
  occurredAt: string;
  actor: string;
  action: string;
  targetId: string | null;
  targetName: string | null;
}

// Descriptions shown to the board (French: this is user-facing text), by action code.
const LABELS: Record<string, string> = {
  'account.disabled': 'a désactivé le compte de',
  'account.reenabled': 'a réactivé le compte de',
  'account.deleted': 'a supprimé le compte de',
  'account.deleted_by_owner': 'a supprimé son propre compte',
  'account.deleted_automatically': "a supprimé automatiquement (désactivé depuis plus d'un mois) le compte de",
  'invitation.sent': 'a invité',
  'invitation.resent': "a renvoyé l'invitation à",
  'board.joined': 'a ajouté au bureau',
  'licence.updated': 'a modifié la licence de',
  'board.left': 'a retiré du bureau',
};

// Rows written before the English renaming of the API keep their French action codes in the (immutable) audit log.
const LEGACY_CODES: Record<string, string> = {
  'compte.desactive': 'account.disabled',
  'compte.reactive': 'account.reenabled',
  'compte.supprime': 'account.deleted',
  'compte.supprime_par_titulaire': 'account.deleted_by_owner',
  'compte.supprime_auto': 'account.deleted_automatically',
  'invitation.envoyee': 'invitation.sent',
  'invitation.relancee': 'invitation.resent',
  'bureau.ajoute': 'board.joined',
  'bureau.retire': 'board.left',
};
const LEGACY_FAMILIES: Record<string, string> = { adhesion: 'membership', titre: 'title' };
const LEGACY_TYPES: Record<string, string> = {
  validee: 'validated', annulee: 'cancelled', attribue: 'assigned', retire: 'removed', cree: 'created', renomme: 'renamed', supprime: 'deleted',
};
// Even older rows (before free-form titles): the title was written as a code.
const LEGACY_TITLES: Record<string, string> = { president: 'Président(e)', secretaire: 'Secrétaire', tresorier: 'Trésorier(e)' };

/** Converts an action code written with the old French names to the current English one. */
export function normalize(action: string): string {
  if (LEGACY_CODES[action]) {
    return LEGACY_CODES[action];
  }
  const [family, type, ...rest] = action.split('.');
  if (LEGACY_FAMILIES[family] && LEGACY_TYPES[type]) {
    const value = rest.join('.');
    return `${LEGACY_FAMILIES[family]}.${LEGACY_TYPES[type]}.${family === 'titre' ? (LEGACY_TITLES[value] ?? value) : value}`;
  }
  return action;
}


/** Sentence describing an action code (French, shown to the board). */
export function describeAction(rawAction: string): string {
  const action = normalize(rawAction);
  const [family, type, ...rest] = action.split('.');
  const value = rest.join('.');
  if (family === 'title') {
    switch (type) {
      case 'assigned': return `a attribué le titre de ${value} à`;
      case 'removed': return `a retiré le titre de ${value} à`;
      case 'created': return `a créé le titre « ${value} »`;
      case 'renamed': return `a renommé le titre « ${value} »`;
      case 'deleted': return `a supprimé le titre « ${value} »`;
    }
  }
  if (family === 'season') {
    switch (type) {
      case 'created': return `a créé la saison « ${value} »`;
      case 'updated': return `a modifié la saison « ${value} »`;
      case 'deleted': return `a supprimé la saison « ${value} »`;
    }
  }
  if (family === 'holiday') {
    switch (type) {
      case 'created': return `a ajouté les vacances « ${value} »`;
      case 'updated': return `a modifié les vacances « ${value} »`;
      case 'deleted': return `a supprimé les vacances « ${value} »`;
    }
  }
  if (family === 'membership') {
    // Older rows hold the year ("2026"), newer ones the name of the season ("2026-2027").
    const season = /^\d+$/.test(value) ? `${value}-${Number(value) + 1}` : value;
    return type === 'validated' ? `a validé l'adhésion ${season} de` : `a annulé l'adhésion ${season} de`;
  }
  return LABELS[action] ?? action;
}

/** Name of the person concerned (empty when there is none or when it is the actor). */
export function targetLabel(entry: AuditEntry): string {
  if (normalize(entry.action) === 'account.deleted_by_owner') {
    return '';
  }
  return entry.targetName ?? (entry.targetId ? 'compte supprimé' : '');
}
