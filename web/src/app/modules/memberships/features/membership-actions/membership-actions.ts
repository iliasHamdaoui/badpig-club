import { Component, computed, inject, input, signal } from '@angular/core';
import { AdminUser } from '../../../../shared/core/admin-user';
import { MEMBERSHIP_KINDS, MembershipKind } from '../../../../shared/core/membership';
import { AdminActivity } from '../../../../shared/infrastructure/admin-activity';
import { MembershipsApi } from '../../infrastructure/memberships.api';

/** Validate (choosing the kind of membership) or cancel the membership of the current season. */
@Component({
  selector: 'app-membership-actions',
  template: `
    @if (user().canValidateMembership && !activity.isConfirming(user().id)) {
      @if (user().membership === 'valid') {
        <button class="btn btn-ghost btn-sm" [class.is-busy]="busy()" [disabled]="activity.busy() !== null" (click)="cancel()">Annuler l'adhésion</button>
      } @else {
        <select class="select input kind-select" [attr.aria-label]="'Type d’adhésion de ' + user().name" [disabled]="activity.busy() !== null"
                (change)="chosen.set($any($event.target).value)">
          @for (k of kinds; track k.value) { <option [value]="k.value" [selected]="kind() === k.value">{{ k.label }}</option> }
        </select>
        <button class="btn btn-sm" [class.is-busy]="busy()" [disabled]="activity.busy() !== null" (click)="validate()">Valider l'adhésion</button>
      }
    }
  `,
  styles: `select.kind-select.input { width: auto; max-width: 100%; min-height: 4rem; font-size: var(--text-small); padding: 0.4rem 0.8rem; }`,
  host: { style: 'display: contents' },
})
export class MembershipActions {
  readonly user = input.required<AdminUser>();
  protected readonly activity = inject(AdminActivity);
  private readonly api = inject(MembershipsApi);
  protected readonly kinds = MEMBERSHIP_KINDS;
  protected readonly busy = computed(() => this.activity.busy() === this.user().id);
  /** Kind picked by the board; until then, the kind chosen at the invitation (or the last membership's). */
  protected readonly chosen = signal<MembershipKind | null>(null);
  protected readonly kind = computed(() => this.chosen() ?? this.user().membershipKind);

  protected validate(): Promise<boolean> {
    const u = this.user();
    return this.activity.run(
      u.id,
      () => this.api.validate(u.id, this.kind()),
      `L'adhésion de ${u.name} est validée pour la saison en cours.`,
    );
  }

  protected cancel(): Promise<boolean> {
    const u = this.user();
    return this.activity.run(u.id, () => this.api.cancel(u.id), `La validation de ${u.name} est annulée.`);
  }
}
