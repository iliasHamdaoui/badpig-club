import { Component, input } from '@angular/core';
import { AdminUser } from '../../../../shared/core/admin-user';
import { formatDate, formatEuro } from '../../../../shared/core/format';
import { kindLabel } from '../../../../shared/core/membership';

/** Membership of the current season in the user list. */
@Component({
  selector: 'app-membership-status',
  template: `
    <span class="highlight" [class]="badge()">
      @switch (user().membership) {
        @case ('valid') {
          <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M5 12l5 5 9-10"></path></svg>
          À jour
        }
        @case ('grace') {
          <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><circle cx="12" cy="12" r="9"></circle><path d="M12 7v5l3 2"></path></svg>
          En tolérance
        }
        @default {
          <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M12 3v6M12 15v6M3 12h6M15 12h6"></path></svg>
          À valider
        }
      }
    </span>
    @if (user().membershipSeason) { <span class="small-text">{{ user().membershipSeason }} · {{ kind() }}{{ amount() }}</span> }
    @if (user().membershipValidatedByName && user().membershipValidatedAt) {
      <span class="small-text">Validée par {{ user().membershipValidatedByName }} le {{ date(user().membershipValidatedAt!) }}</span>
    }
  `,
  styles: `:host { display: grid; gap: var(--space-5); justify-items: start; }`,
})
export class MembershipStatus {
  readonly user = input.required<AdminUser>();

  protected badge(): string {
    switch (this.user().membership) {
      case 'valid':
        return 'highlight-success';
      case 'grace':
        return 'highlight-warning';
      default:
        return 'highlight-secondary';
    }
  }

  protected kind(): string {
    return kindLabel(this.user().membershipKind);
  }

  protected amount(): string {
    const amount = this.user().membershipAmount;
    return amount === null ? '' : ` · ${formatEuro(amount)}`;
  }

  protected date = formatDate;
}
