import { Routes } from '@angular/router';
import { memberGuard, signedInGuard } from '../../shared/infrastructure/guards';

export const accountsRoutes: Routes = [
  { path: 'profile', canActivate: [signedInGuard], loadComponent: () => import('./features/my-profile/my-profile.page').then((m) => m.MyProfilePage) },
  { path: 'club/directory', canActivate: [memberGuard], loadComponent: () => import('./features/club-directory/club-directory.page').then((m) => m.ClubDirectoryPage) },
];
