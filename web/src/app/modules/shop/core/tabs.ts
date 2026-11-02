import { Tab } from '../../../shared/ui/tabs';

/** The pages of the shop, for the board. */
export const SHOP_ADMIN_TABS: Tab[] = [
  { path: '/admin/shop', label: 'Articles et stock', exact: true },
  { path: '/admin/shop/orders', label: 'Commandes' },
  { path: '/admin/shop/audit-log', label: 'Journal' },
];
