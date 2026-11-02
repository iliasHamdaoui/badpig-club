import { Tab } from '../../../shared/ui/tabs';

/**
 * The pages of the interclubs section, shown at the top of each of them.
 * The date proposals are hidden for now: their line is kept here, commented out, to bring the feature back.
 */
export const INTERCLUBS_TABS: Tab[] = [
  // { path: '/interclubs/proposals', label: 'Disponibilités' },
  { path: '/interclubs', label: 'Calendrier', exact: true },
  { path: '/interclubs/teams', label: 'Équipes' },
];
