import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MEMBERSHIP_KINDS, MembershipKind } from '../../../../shared/core/membership';
import { AdminActivity } from '../../../../shared/infrastructure/admin-activity';
import { AuthService } from '../../../../shared/infrastructure/auth.service';
import { AccountsApi } from '../../infrastructure/accounts.api';

/** Form to invite a person (the account is created, the person receives a sign-up link by e-mail). */
@Component({
  selector: 'app-invite-user-form',
  imports: [FormsModule],
  template: `
    <form class="stack stack-s" (ngSubmit)="invite()">
      <div class="invite-fields">
        <div class="field">
          <label class="label" for="invite-name">Nom</label>
          <input class="input" id="invite-name" name="name" [(ngModel)]="name" required maxlength="200" autocomplete="off" />
        </div>
        <div class="field">
          <label class="label" for="invite-email">Adresse e-mail</label>
          <input class="input" id="invite-email" name="email" type="email" [(ngModel)]="email" required maxlength="150" autocomplete="off" />
        </div>
        <div class="field">
          <label class="label" for="invite-group">Groupe</label>
          <select class="select" id="invite-group" name="group" [(ngModel)]="group">
            <option value="members">Membres</option>
            <option value="board">Bureau</option>
            @if (auth.isAdmin()) { <option value="admin">Admin</option> }
          </select>
        </div>
        <div class="field">
          <label class="label" for="invite-kind">Type d'adhésion</label>
          <select class="select" id="invite-kind" name="kind" [(ngModel)]="kind">
            @for (k of kinds; track k.value) { <option [value]="k.value">{{ k.label }}</option> }
          </select>
        </div>
      </div>
      <p class="form-actions">
        <button class="btn" type="submit" [class.is-busy]="inviting()" [disabled]="inviting()">
          {{ inviting() ? 'Envoi…' : "Envoyer l'invitation" }}
        </button>
      </p>
    </form>
    <p class="small-text">La personne reçoit un e-mail avec un lien (valable 3 jours) pour choisir son mot de passe. Elle est « en attente d'inscription » jusqu'à sa première connexion. Le type d'adhésion (licencié du club ou d'un autre club) détermine la cotisation ; le bureau peut le changer en validant l'adhésion.</p>
  `,
  styles: `
    :host { display: grid; gap: var(--space-15); }
    .invite-fields { display: grid; gap: var(--space-15); grid-template-columns: repeat(auto-fit, minmax(min(22rem, 100%), 1fr)); }
  `,
})
export class InviteUserForm {
  private readonly api = inject(AccountsApi);
  private readonly activity = inject(AdminActivity);
  protected readonly auth = inject(AuthService);
  protected name = '';
  protected email = '';
  protected group: 'members' | 'board' | 'admin' = 'members';
  protected kind: MembershipKind = 'internal';
  protected readonly kinds = MEMBERSHIP_KINDS;
  protected inviting = () => this.activity.busy() === 'invite';

  protected async invite(): Promise<void> {
    const sentTo = this.email.trim();
    const ok = await this.activity.run(
      'invite',
      () => this.api.invite(this.name, this.email, this.group, this.kind),
      `Invitation envoyée à ${sentTo}.`,
      "L'invitation a échoué. Vérifiez le nom et l'adresse e-mail.",
    );
    if (ok) {
      this.name = '';
      this.email = '';
      this.group = 'members';
      this.kind = 'internal';
    }
  }
}
