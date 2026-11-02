import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { MailboxStatus } from '../core/club-mailbox';
import { API } from './api';

/** New mail waiting in the club mailbox, for the board home page. Unreachable mailbox: nothing is shown. */
@Injectable({ providedIn: 'root' })
export class ClubMailbox {
  private readonly http = inject(HttpClient);

  async status(): Promise<MailboxStatus | null> {
    try {
      const status = await firstValueFrom(this.http.get<MailboxStatus>(`${API}/admin/mailbox`));
      return status.isAvailable ? status : null;
    } catch {
      return null;
    }
  }
}
