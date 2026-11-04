import { Injectable, inject } from '@angular/core';
import { ActivatedRouteSnapshot, NavigationEnd, Router } from '@angular/router';
import { filter } from 'rxjs';
import { API } from './api';
import { AuthService } from './auth.service';

/** A page left open all day must not count for more than this (seconds). */
const MAX_SECONDS = 30 * 60;

/**
 * Anonymous usage statistics: tells the API which page a signed-in person looked at and for how long.
 * The page is the route template (`/announcements/:id`), never the address with its identifiers; the time counts only while the tab is visible.
 * The API keeps one counter per day and per page, so nothing says who looked at what. No cookie, nothing sent to a third party.
 */
@Injectable({ providedIn: 'root' })
export class UsageTracker {
  private readonly router = inject(Router);
  private readonly auth = inject(AuthService);
  private path: string | null = null;
  private visibleSince: number | null = null;
  private visibleMs = 0;
  private counted = false;

  /** Starts to follow the navigation; called once, by the application. */
  start(): void {
    this.router.events.pipe(filter((event): event is NavigationEnd => event instanceof NavigationEnd)).subscribe(() => this.opened());
    document.addEventListener('visibilitychange', () => (document.visibilityState === 'hidden' ? this.hidden() : this.shown()));
    window.addEventListener('pagehide', () => this.hidden());
  }

  /** The route template of the page being shown, for example `/shop/orders/:id` (`/` for the home page). */
  private template(): string {
    const parts: string[] = [];
    let route: ActivatedRouteSnapshot | null = this.router.routerState.snapshot.root;
    while (route) {
      const path = route.routeConfig?.path;
      if (path) {
        parts.push(path);
      }
      route = route.firstChild;
    }
    return '/' + parts.join('/');
  }

  private opened(): void {
    this.hidden(); // reports the page that is being left
    this.path = this.template();
    this.visibleMs = 0;
    this.counted = false;
    this.visibleSince = document.visibilityState === 'visible' ? Date.now() : null;
  }

  private shown(): void {
    this.visibleSince ??= Date.now();
  }

  /** The page is left or the tab is hidden: what was seen so far is reported. */
  private hidden(): void {
    if (this.visibleSince !== null) {
      this.visibleMs += Date.now() - this.visibleSince;
      this.visibleSince = null;
    }
    this.report();
  }

  private report(): void {
    if (this.path === null || !this.auth.signedIn() || this.path === '/**') {
      return;
    }
    const seconds = Math.min(MAX_SECONDS, Math.round(this.visibleMs / 1000));
    const views = this.counted ? 0 : 1;
    if (views === 0 && seconds === 0) {
      return;
    }
    this.counted = true;
    this.visibleMs = 0;
    // keepalive: the report still leaves when the tab is being closed. A failure is ignored: statistics never get in the way.
    fetch(`${API}/usage/visits`, {
      method: 'POST',
      credentials: 'same-origin',
      keepalive: true,
      headers: { 'Content-Type': 'application/json', 'X-BadPig-CSRF': '1' },
      body: JSON.stringify({ path: this.path, views, seconds }),
    }).catch(() => undefined);
  }
}
