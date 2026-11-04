import { Routes } from '@angular/router';
import { boardGuard } from '../../shared/infrastructure/guards';

export const usageRoutes: Routes = [
  { path: 'admin/usage', canActivate: [boardGuard], loadComponent: () => import('./features/usage-statistics/usage-statistics.page').then((m) => m.UsageStatisticsPage) },
];
