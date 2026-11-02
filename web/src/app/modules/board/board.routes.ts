import { Routes } from '@angular/router';
import { boardGuard } from '../../shared/infrastructure/guards';

export const boardRoutes: Routes = [
  { path: 'admin/titles', canActivate: [boardGuard], loadComponent: () => import('./features/manage-titles/manage-titles.page').then((m) => m.ManageTitlesPage) },
];
