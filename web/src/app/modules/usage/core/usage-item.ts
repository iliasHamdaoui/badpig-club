/** One page of the website and how it was used (GET /api/v1/admin/usage). */
export interface UsageItem {
  /** Route template, for example `/announcements/:id`. */
  path: string;
  views: number;
  totalSeconds: number;
  averageSeconds: number;
}

/** One day of the chart (GET /api/v1/admin/usage/trend), all pages together. */
export interface UsageDay {
  /** yyyy-MM-dd */
  date: string;
  views: number;
  seconds: number;
}

/** Periods the board can look at, in days. */
export const PERIODS = [7, 30, 90, 365] as const;

const NAMES: Record<string, string> = {
  '/': 'Accueil',
  '/club': 'Le club',
  '/club/directory': 'Trombinoscope',
  '/profile': 'Mon profil',
  '/announcements': 'Annonces',
  '/announcements/:id': 'Détail d\'une annonce',
  '/interclubs': 'Interclubs : calendrier',
  '/interclubs/fixtures/:id': 'Interclubs : détail d\'une rencontre',
  '/interclubs/teams': 'Interclubs : équipes',
  '/interclubs/teams/:id': 'Interclubs : une équipe',
  '/interclubs/teams/:id/members': 'Interclubs : joueurs d\'une équipe',
  '/interclubs/proposals': 'Interclubs : propositions de dates',
  '/interclubs/proposals/:id': 'Interclubs : une proposition de dates',
  '/shop': 'Boutique',
  '/shop/orders': 'Boutique : mes commandes',
  '/shop/orders/:id': 'Boutique : détail d\'une commande',
  '/admin': 'Administration',
  '/admin/users': 'Administration : adhérents',
  '/admin/seasons': 'Administration : saisons',
  '/admin/seasons/statistics': 'Administration : statistiques des saisons',
  '/admin/seasons/:id/holidays': 'Administration : vacances scolaires',
  '/admin/titles': 'Administration : titres du bureau',
  '/admin/announcements': 'Administration : annonces',
  '/admin/announcements/new': 'Administration : nouvelle annonce',
  '/admin/announcements/new-poll': 'Administration : nouveau sondage',
  '/admin/announcements/:id/edit': 'Administration : modifier une annonce',
  '/admin/shop': 'Administration : articles',
  '/admin/shop/orders': 'Administration : commandes reçues',
  '/admin/shop/variants/:id/history': 'Administration : historique du stock',
  '/admin/shop/audit-log': 'Administration : journal de la boutique',
  '/admin/audit-log': 'Administration : journal',
  '/admin/usage': 'Administration : usage du site',
};

/** The name of a page for the board; an unknown one (a page added since) is shown as its route. */
export function pageName(path: string): string {
  return NAMES[path] ?? path;
}
