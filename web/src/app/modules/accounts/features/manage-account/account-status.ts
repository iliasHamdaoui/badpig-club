import { Component, input } from '@angular/core';
import { AdminUser } from '../../../../shared/core/admin-user';
import { formatDate } from '../../../../shared/core/format';

/** Account status in the user list (active, disabled, waiting for sign-up) with its details. */
@Component({
  selector: 'app-account-status',
  template: `
    <span class="highlight" [class]="badge()">
      @switch (user().status) {
        @case ('pending') {
          <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><circle cx="12" cy="12" r="9"></circle><path d="M12 7v5l3 2"></path></svg>
          En attente d'inscription
        }
        @case ('disabled') {
          <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><circle cx="12" cy="12" r="9"></circle><path d="M6 18L18 6"></path></svg>
          Désactivé
        }
        @default {
          <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M5 12l5 5 9-10"></path></svg>
          Actif
        }
      }
    </span>
    @if (user().automaticDeletionAt) { <span class="small-text">Suppression automatique le {{ date(user().automaticDeletionAt!) }}</span> }
    @if (user().invitedByName) { <span class="small-text">Invité par {{ user().invitedByName }}{{ user().invitedAt ? ' le ' + date(user().invitedAt!) : '' }}</span> }
  `,
  styles: `:host { display: grid; gap: var(--space-5); justify-items: start; }`,
})
export class AccountStatus {
  readonly user = input.required<AdminUser>();

  protected badge(): string {
    switch (this.user().status) {
      case 'pending':
        return 'highlight-warning';
      case 'disabled':
        return 'highlight-neutral';
      default:
        return 'highlight-success';
    }
  }

  protected date = formatDate;
}
