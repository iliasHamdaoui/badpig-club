import { Routes } from '@angular/router';
import { signedInGuard } from '../../shared/infrastructure/guards';

export const accountsRoutes: Routes = [
  { path: 'profile', canActivate: [signedInGuard], loadComponent: () => import('./features/my-profile/my-profile.page').then((m) => m.MyProfilePage) },
];
