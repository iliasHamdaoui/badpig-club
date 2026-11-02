import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ClubSessions } from '../core/club-sessions';
import { API } from './api';

/**
 * When the club plays, as the API computes it (Paris date, school holidays of the seasons taken out).
 * Read by a visitor as well as by a member, and loaded only once per visit of the site.
 */
@Injectable({ providedIn: 'root' })
export class ClubSessionsApi {
  private readonly http = inject(HttpClient);
  readonly sessions = signal<ClubSessions | null>(null);
  readonly failed = signal(false);
  private loading: Promise<void> | null = null;

  load(): Promise<void> {
    return (this.loading ??= this.fetch());
  }

  private async fetch(): Promise<void> {
    try {
      this.sessions.set(await firstValueFrom(this.http.get<ClubSessions>(`${API}/club/sessions`)));
      this.failed.set(false);
    } catch {
      this.failed.set(true);
    }
  }
}
