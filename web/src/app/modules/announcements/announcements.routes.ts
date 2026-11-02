import { Routes } from '@angular/router';
import { boardGuard } from '../../shared/infrastructure/guards';

// Reading is public (visitors see the public announcements); writing is for the board (the API decides, the guard only hides).
export const announcementsRoutes: Routes = [
  { path: 'announcements', loadComponent: () => import('./features/announcement-list/announcement-list.page').then((m) => m.AnnouncementListPage) },
  { path: 'announcements/:id', loadComponent: () => import('./features/announcement-detail/announcement-detail.page').then((m) => m.AnnouncementDetailPage) },
  { path: 'admin/announcements', canActivate: [boardGuard], loadComponent: () => import('./features/manage-announcements/manage-announcements.page').then((m) => m.ManageAnnouncementsPage) },
  { path: 'admin/announcements/new', canActivate: [boardGuard], loadComponent: () => import('./features/announcement-form/announcement-form.page').then((m) => m.AnnouncementFormPage) },
  { path: 'admin/announcements/new-poll', canActivate: [boardGuard], data: { form: 'poll' }, loadComponent: () => import('./features/announcement-form/announcement-form.page').then((m) => m.AnnouncementFormPage) },
  { path: 'admin/announcements/:id/edit', canActivate: [boardGuard], loadComponent: () => import('./features/announcement-form/announcement-form.page').then((m) => m.AnnouncementFormPage) },
];
