import { Routes } from '@angular/router';
import { memberGuard } from '../../shared/infrastructure/guards';

// Public pages and the members area.
export const siteRoutes: Routes = [
  { path: 'members', canActivate: [memberGuard], loadComponent: () => import('./features/members-area/members-area.page').then((m) => m.MembersAreaPage) },
];
