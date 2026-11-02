import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Membership } from '../../../../shared/core/membership';
import { apiErrorMessage, formatLongDate } from '../../../../shared/core/format';
import { AuthService } from '../../../../shared/infrastructure/auth.service';
import { AccountsApi } from '../../infrastructure/accounts.api';
import { Crumb, PageHead } from '../../../../shared/ui/page-head';

@Component({
  selector: 'app-my-profile',
  imports: [FormsModule, PageHead],
  host: { class: 'bleed' },
  template: `
    <main class="container stack stack-l" style="padding-block: var(--space-30) var(--space-60); max-width: 76rem">
      <app-page-head [trail]="trail" title="Mon profil" />
      @if (title()) { <p class="row"><span class="highlight highlight-secondary">{{ title() }} du bureau</span></p> }

      @if (loading()) {
        <div class="stack" aria-hidden="true"><span class="skeleton" style="height: 14rem"></span></div>
      } @else {
        @if (!membershipUpToDate()) {
          <p class="alert alert-warning">
            <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M12 3l9 17H3zM12 10v4M12 17h.01"></path></svg>
            <span>
              <span class="alert-title">Adhésion à valider.</span>
              Votre adhésion pour la saison en cours n'est pas encore validée : l'accès aux interclubs et à la boutique est suspendu.
              Réglez votre cotisation auprès d'un membre du bureau, qui validera votre adhésion.
            </span>
          </p>
        } @else if (membership() === 'grace') {
          <p class="alert alert-info">
            <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><circle cx="12" cy="12" r="9"></circle><path d="M12 11v5M12 8h.01"></path></svg>
            <span>Votre adhésion de la saison passée reste valable pour quelques semaines. Pensez à régler votre cotisation pour la nouvelle saison.</span>
          </p>
        }

        <section class="card stack" aria-labelledby="t-id">
          <h2 id="t-id" class="card-title">Mes coordonnées</h2>
          @if (message()) {
            <p class="alert alert-success"><svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M5 12l5 5 9-10"></path></svg><span>{{ message() }}</span></p>
          }
          @if (error()) {
            <p class="alert alert-error"><svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M8 3h8l5 5v8l-5 5H8l-5-5V8z"></path><path d="M12 8v5M12 16h.01"></path></svg><span><span class="alert-title">Erreur.</span> {{ error() }}</span></p>
          }
          <form class="stack" (ngSubmit)="save()">
            <div class="field">
              <label class="label" for="name">Nom</label>
              <input class="input" id="name" name="name" [(ngModel)]="name" required maxlength="200" autocomplete="name" />
            </div>
            <div class="field">
              <label class="label" for="email">Adresse e-mail</label>
              <input class="input" id="email" name="email" type="email" [(ngModel)]="email" required maxlength="254" autocomplete="email" />
            </div>
            <p class="form-actions">
              <button class="btn" type="submit" [class.is-busy]="saving()" [disabled]="saving()">{{ saving() ? 'Enregistrement…' : 'Enregistrer' }}</button>
            </p>
          </form>
        </section>

        <section class="card stack" aria-labelledby="t-licence">
          <h2 id="t-licence" class="card-title">Ma licence</h2>
          @if (licenceMessage()) {
            <p class="alert alert-success"><svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M5 12l5 5 9-10"></path></svg><span>{{ licenceMessage() }}</span></p>
          }
          @if (licenceError()) {
            <p class="alert alert-error"><svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M8 3h8l5 5v8l-5 5H8l-5-5V8z"></path><path d="M12 8v5M12 16h.01"></path></svg><span><span class="alert-title">Erreur.</span> {{ licenceError() }}</span></p>
          }
          <form class="stack" (ngSubmit)="saveLicence()">
            <div class="field">
              <label class="label" for="licence">Numéro de licence</label>
              <input class="input" id="licence" name="licence" [(ngModel)]="licence" maxlength="20" placeholder="07123456" autocomplete="off" />
            </div>
            <p class="form-actions">
              <button class="btn" type="submit" [class.is-busy]="savingLicence()" [disabled]="savingLicence()">{{ savingLicence() ? 'Enregistrement…' : 'Enregistrer' }}</button>
            </p>
          </form>
          <p class="small-text">Votre numéro de licence à la fédération (FFBaD). Un membre du bureau peut aussi le renseigner ; laissez le champ vide pour le retirer.</p>
          @if (memberSince()) {
            <dl><div class="kv"><dt>Adhérent depuis</dt><dd>{{ day(memberSince()!) }}</dd></div></dl>
          }
        </section>

        <section class="card stack stack-s" aria-labelledby="t-pwd">
          <h2 id="t-pwd" class="card-title">Mot de passe</h2>
          <p class="small-text">
            Pour changer votre mot de passe, déconnectez-vous puis utilisez « Mot de passe oublié ? » sur la page de connexion :
            vous recevrez un e-mail avec un lien pour en choisir un nouveau.
          </p>
        </section>

        <section class="card stack danger-zone" aria-labelledby="t-del">
          <h2 id="t-del" class="card-title">Supprimer mon compte</h2>
          <p>Cette action est <strong>définitive</strong> : votre compte et vos données personnelles seront effacés.</p>
          @if (!deleting()) {
            <p><button class="btn btn-danger" (click)="deleting.set(true)">Supprimer mon compte…</button></p>
          } @else {
            <div class="field">
              <label class="label" for="confirmation">Pour confirmer, saisissez votre adresse e-mail ({{ initialEmail() }})</label>
              <input class="input" id="confirmation" type="email" autocomplete="off" [(ngModel)]="confirmation" />
            </div>
            <p class="form-actions">
              <button class="btn btn-danger" [class.is-busy]="deletionInProgress()" [disabled]="!confirmationValid() || deletionInProgress()" (click)="deleteAccount()">
                {{ deletionInProgress() ? 'Suppression…' : 'Supprimer définitivement' }}
              </button>
              <button class="btn btn-ghost" [disabled]="deletionInProgress()" (click)="cancelDeletion()">Annuler</button>
            </p>
            @if (deletionError()) {
              <p class="alert alert-error"><svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M8 3h8l5 5v8l-5 5H8l-5-5V8z"></path><path d="M12 8v5M12 16h.01"></path></svg><span>{{ deletionError() }}</span></p>
            }
          }
        </section>
      }
    </main>
  `,
  styles: `.danger-zone { border-color: var(--alert-error-border); }`,
})
export class MyProfilePage implements OnInit {
  protected readonly trail: Crumb[] = [];
  private readonly api = inject(AccountsApi);
  private readonly auth = inject(AuthService);
  protected name = '';
  protected email = '';
  protected readonly loading = signal(true);
  protected readonly saving = signal(false);
  protected readonly message = signal('');
  protected readonly error = signal('');
  protected readonly membership = signal<Membership>('valid');
  protected readonly membershipUpToDate = signal(true);
  protected readonly title = signal('');
  protected licence = '';
  protected readonly savingLicence = signal(false);
  protected readonly licenceMessage = signal('');
  protected readonly licenceError = signal('');
  /** Day the person joined the club, as the API computes it from their first validated membership. */
  protected readonly memberSince = signal<string | null>(null);
  protected readonly day = formatLongDate;

  // Account deletion
  protected readonly initialEmail = signal('');
  protected readonly deleting = signal(false);
  protected readonly confirmation = signal('');
  protected readonly deletionInProgress = signal(false);
  protected readonly deletionError = signal('');
  protected readonly confirmationValid = computed(
    () => this.confirmation().trim().toLowerCase() === this.initialEmail().trim().toLowerCase() && this.initialEmail() !== '',
  );

  async ngOnInit(): Promise<void> {
    try {
      const p = await this.api.myProfile();
      this.name = p.name;
      this.email = p.email;
      this.initialEmail.set(p.email);
      this.membership.set(p.membership);
      this.membershipUpToDate.set(p.membershipUpToDate);
      this.title.set(p.title ?? '');
      this.licence = p.licence ?? '';
      this.memberSince.set(p.memberSince);
    } catch {
      this.error.set('Impossible de charger votre profil.');
    } finally {
      this.loading.set(false);
    }
  }

  async save(): Promise<void> {
    this.message.set('');
    this.error.set('');
    this.saving.set(true);
    try {
      await this.api.updateMyProfile(this.name, this.email);
      const p = await this.api.myProfile();
      this.name = p.name;
      this.email = p.email;
      this.initialEmail.set(p.email);
      this.message.set('Profil enregistré.');
    } catch {
      this.error.set("L'enregistrement a échoué. Vérifiez les champs et réessayez.");
    } finally {
      this.saving.set(false);
    }
  }

  protected async saveLicence(): Promise<void> {
    this.licenceMessage.set('');
    this.licenceError.set('');
    this.savingLicence.set(true);
    try {
      await this.api.setMyLicence(this.licence.trim());
      const p = await this.api.myProfile();
      this.licence = p.licence ?? '';
      this.licenceMessage.set(p.licence ? 'Licence enregistrée.' : 'Licence retirée.');
    } catch (e) {
      this.licenceError.set(apiErrorMessage(e, "L'enregistrement a échoué. Vérifiez le numéro et réessayez."));
    } finally {
      this.savingLicence.set(false);
    }
  }

  protected cancelDeletion(): void {
    this.deleting.set(false);
    this.confirmation.set('');
    this.deletionError.set('');
  }

  protected async deleteAccount(): Promise<void> {
    if (!this.confirmationValid()) {
      return;
    }
    this.deletionError.set('');
    this.deletionInProgress.set(true);
    try {
      await this.api.deleteMyAccount();
    } catch (e) {
      const apiMessage = e instanceof HttpErrorResponse && e.status === 409 ? (e.error?.title as string | undefined) : undefined;
      this.deletionError.set(apiMessage ?? 'La suppression a échoué. Réessayez plus tard.');
      this.deletionInProgress.set(false);
      return;
    }
    await this.auth.endSession(); // the account no longer exists: the session is forgotten and we go back to the home page
  }
}
