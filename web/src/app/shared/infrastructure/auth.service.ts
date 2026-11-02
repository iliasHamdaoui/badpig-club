import { Injectable, computed, signal } from '@angular/core';

import { Groups } from '../core/groups';

interface BffUser {
  sub: string | null;
  name: string | null;
  groups: string[];
}

/**
 * Sign-in through the BFF (Backend For Frontend): the browser only has an HttpOnly session cookie,
 * the OIDC tokens stay on the server. This service only asks the BFF "who am I?"
 * and redirects to /bff/login or /bff/logout.
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly user = signal<BffUser | null>(null);

  readonly signedIn = computed(() => this.user() !== null);
  readonly displayName = computed(() => this.user()?.name ?? '');
  readonly groups = computed<string[]>(() => this.user()?.groups ?? []);
  readonly isAdmin = computed(() => this.groups().includes(Groups.admin));
  readonly isBoard = computed(() => [Groups.board, Groups.admin].some((g) => this.groups().includes(g)));
  readonly isMember = computed(() =>
    [Groups.members, Groups.board, Groups.admin].some((g) => this.groups().includes(g)),
  );

  /** Membership up to date? (null until the profile is loaded). Only used for display: the API decides. */
  readonly membershipUpToDate = signal<boolean | null>(null);

  /** True during a redirect to sign-in or sign-out (used to show a waiting indicator). */
  readonly busy = signal(false);

  async init(): Promise<void> {
    try {
      const response = await fetch('/bff/user', { credentials: 'same-origin', cache: 'no-store' });
      this.user.set(response.ok ? ((await response.json()) as BffUser) : null);
    } catch {
      this.user.set(null);
    }
    // Back button from Authentik: the page comes back from the cache, so the waiting indicator is removed.
    window.addEventListener('pageshow', (e) => {
      if (e.persisted) {
        this.busy.set(false);
      }
    });
  }

  /** Opens Authentik's sign-in page (through the BFF), then comes back to `returnUrl`. */
  async signIn(returnUrl = '/'): Promise<void> {
    if (this.busy()) {
      return;
    }
    this.busy.set(true);
    location.assign(`/bff/login?returnUrl=${encodeURIComponent(returnUrl)}`);
  }

  async signOut(): Promise<void> {
    if (this.busy()) {
      return;
    }
    this.busy.set(true);
    this.postForm('/bff/logout');
  }

  /** After the account is deleted: the session is pointless, so it is closed. */
  async endSession(): Promise<void> {
    this.busy.set(true);
    this.postForm('/bff/logout');
  }

  /** The session expired (401 from the API): sign in again, then come back here. */
  sessionExpired(): void {
    this.user.set(null);
    void this.signIn(location.pathname + location.search);
  }

  /** Sign-out is a POST request (another site cannot trigger it with the cookie). */
  private postForm(action: string): void {
    const form = document.createElement('form');
    form.method = 'POST';
    form.action = action;
    document.body.appendChild(form);
    form.submit();
  }
}
