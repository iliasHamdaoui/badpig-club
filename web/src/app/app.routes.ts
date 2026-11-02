import { Routes } from '@angular/router';
import { boardGuard } from './shared/infrastructure/guards';
import { accountsRoutes } from './modules/accounts';
import { announcementsRoutes } from './modules/announcements';
import { auditRoutes } from './modules/audit';
import { boardRoutes } from './modules/board';
import { membershipsRoutes } from './modules/memberships';
import { interclubsRoutes } from './modules/interclubs';
import { ordersRoutes } from './modules/orders';
import { shopRoutes } from './modules/shop';

// The host assembles the routes of the modules.
// Guards are only a display convenience: the real protection is in the API.
export const routes: Routes = [
  // The home page is assembled by the host: it shows blocks that belong to several modules.
  { path: '', loadComponent: () => import('./home/home.page').then((m) => m.HomePage) },
  { path: 'club', loadComponent: () => import('./club/club.page').then((m) => m.ClubPage) },
  ...accountsRoutes,
  ...boardRoutes,
  ...membershipsRoutes,
  ...auditRoutes,
  ...announcementsRoutes,
  ...interclubsRoutes,
  ...ordersRoutes,
  ...shopRoutes,
  { path: 'admin', canActivate: [boardGuard], loadComponent: () => import('./admin/admin-home.page').then((m) => m.AdminHomePage) },
  { path: 'admin/users', canActivate: [boardGuard], loadComponent: () => import('./admin/admin-users.page').then((m) => m.AdminUsersPage) },
  { path: '**', redirectTo: '' },
];
