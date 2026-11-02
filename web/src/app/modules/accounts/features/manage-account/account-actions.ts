import { Component, computed, inject, input } from '@angular/core';
import { AdminUser } from '../../../../shared/core/admin-user';
import { AdminActivity } from '../../../../shared/infrastructure/admin-activity';
import { AccountsApi } from '../../infrastructure/accounts.api';

/** Account actions of the user list: resend the invitation, disable / re-enable, delete. */
@Component({
  selector: 'app-account-actions',
  template: `
    @if (user().canManage) {
      @if (activity.confirming() === deleteKey()) {
        <span>Supprimer définitivement ?</span>
        <button class="btn btn-danger btn-sm" [class.is-busy]="busy()" [disabled]="activity.busy() !== null" (click)="remove()">Oui, supprimer</button>
        <button class="btn btn-ghost btn-sm" [disabled]="activity.busy() !== null" (click)="activity.confirming.set(null)">Annuler</button>
      } @else if (!activity.isConfirming(user().id)) {
        @if (user().status === 'pending') {
          <button class="btn btn-ghost btn-sm" [class.is-busy]="busy()" [disabled]="activity.busy() !== null" (click)="resendInvitation()">Renvoyer l'invitation</button>
        }
        <button class="btn btn-ghost btn-sm" [class.is-busy]="busy()" [disabled]="activity.busy() !== null" (click)="toggleActive()">
          {{ user().isActive ? 'Désactiver' : 'Réactiver' }}
        </button>
        <button class="btn btn-danger btn-sm" [disabled]="activity.busy() !== null" (click)="activity.confirming.set(deleteKey())">Supprimer</button>
      }
    }
  `,
  host: { style: 'display: contents' },
})
export class AccountActions {
  readonly user = input.required<AdminUser>();
  protected readonly activity = inject(AdminActivity);
  private readonly api = inject(AccountsApi);
  protected readonly deleteKey = computed(() => `${this.user().id}:delete`);
  protected readonly busy = computed(() => this.activity.busy() === this.user().id);

  protected resendInvitation(): Promise<boolean> {
    const u = this.user();
    return this.activity.run(u.id, () => this.api.resendInvitation(u.id), `Invitation renvoyée à ${u.name}.`);
  }

  protected toggleActive(): Promise<boolean> {
    const u = this.user();
    return u.isActive
      ? this.activity.run(u.id, () => this.api.disable(u.id), `Le compte de ${u.name} a été désactivé.`)
      : this.activity.run(u.id, () => this.api.enable(u.id), `Le compte de ${u.name} a été réactivé.`);
  }

  protected remove(): Promise<boolean> {
    const u = this.user();
    return this.activity.run(u.id, () => this.api.deleteUser(u.id), `Le compte de ${u.name} a été supprimé.`);
  }
}
