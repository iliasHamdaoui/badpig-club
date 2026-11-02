import { Routes } from '@angular/router';
import { boardGuard } from '../../shared/infrastructure/guards';

export const auditRoutes: Routes = [
  { path: 'admin/audit-log', canActivate: [boardGuard], loadComponent: () => import('./features/audit-log/audit-log.page').then((m) => m.AuditLogPage) },
];
