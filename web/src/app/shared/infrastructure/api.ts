import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { AuthService } from './auth.service';

/** API prefix: same site as the application, the BFF relays the calls to the API. */
export const API = '/api/v1';

/**
 * Calls to /api: adds the anti-CSRF header expected by the BFF and, if the session has expired (401),
 * sends the person back to sign-in.
 */
export const apiInterceptor: HttpInterceptorFn = (req, next) => {
  if (!req.url.startsWith('/api/')) {
    return next(req);
  }
  const auth = inject(AuthService);
  return next(req.clone({ setHeaders: { 'X-BadPig-CSRF': '1' } })).pipe(
    catchError((e: unknown) => {
      if (e instanceof HttpErrorResponse && e.status === 401) {
        auth.sessionExpired();
      }
      return throwError(() => e);
    }),
  );
};
