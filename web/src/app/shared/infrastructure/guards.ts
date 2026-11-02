import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

// Guards are only a display convenience: the real protection is in the API.

/** Reserves a page for signed-in people; otherwise sends them to the sign-in page. */
export const signedInGuard: CanActivateFn = (_route, state) => {
  const auth = inject(AuthService);
  if (auth.signedIn()) {
    return true;
  }
  void auth.signIn(state.url);
  return false;
};

export const memberGuard: CanActivateFn = (_route, state) => {
  const auth = inject(AuthService);
  if (!auth.signedIn()) {
    void auth.signIn(state.url);
    return false;
  }
  if (auth.isMember() && auth.membershipUpToDate() === false) {
    return inject(Router).parseUrl('/profile'); // membership to renew: the profile page explains it
  }
  return auth.isMember() || inject(Router).parseUrl('/');
};

export const boardGuard: CanActivateFn = (_route, state) => {
  const auth = inject(AuthService);
  if (!auth.signedIn()) {
    void auth.signIn(state.url);
    return false;
  }
  return auth.isBoard() || inject(Router).parseUrl('/');
};
