import { Routes } from '@angular/router';
import { memberGuard } from '../../shared/infrastructure/guards';

// The interclubs of the members: guards only hide the pages, the API decides who may see and change what.
export const interclubsRoutes: Routes = [
  { path: 'interclubs', canActivate: [memberGuard], loadComponent: () => import('./features/calendar/calendar.page').then((m) => m.CalendarPage) },
  { path: 'interclubs/teams', canActivate: [memberGuard], loadComponent: () => import('./features/teams/teams.page').then((m) => m.TeamsPage) },
  { path: 'interclubs/teams/:id', canActivate: [memberGuard], loadComponent: () => import('./features/team/team.page').then((m) => m.TeamPage) },
  { path: 'interclubs/teams/:id/members', canActivate: [memberGuard], loadComponent: () => import('./features/team-members/team-members.page').then((m) => m.TeamMembersPage) },
  // Hidden for now (the club creates its fixtures directly); the pages are kept for the day the feature comes back:
  // { path: 'interclubs/proposals', canActivate: [memberGuard], loadComponent: () => import('./features/proposals/proposals.page').then((m) => m.ProposalsPage) },
  // Hidden for now (the club creates its fixtures directly); the pages are kept for the day the feature comes back:
  // { path: 'interclubs/proposals/:id', canActivate: [memberGuard], loadComponent: () => import('./features/proposal-detail/proposal-detail.page').then((m) => m.ProposalDetailPage) },
  { path: 'interclubs/fixtures/:id', canActivate: [memberGuard], loadComponent: () => import('./features/fixture-detail/fixture-detail.page').then((m) => m.FixtureDetailPage) },
];
