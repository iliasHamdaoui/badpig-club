import { Routes } from '@angular/router';
import { boardGuard } from '../../shared/infrastructure/guards';

export const membershipsRoutes: Routes = [
  { path: 'admin/seasons', canActivate: [boardGuard], loadComponent: () => import('./features/manage-seasons/manage-seasons.page').then((m) => m.ManageSeasonsPage) },
  { path: 'admin/seasons/:id/holidays', canActivate: [boardGuard], loadComponent: () => import('./features/manage-holidays/manage-holidays.page').then((m) => m.ManageHolidaysPage) },
  { path: 'admin/seasons/statistics', canActivate: [boardGuard], loadComponent: () => import('./features/season-statistics/season-statistics.page').then((m) => m.SeasonStatisticsPage) },
];
