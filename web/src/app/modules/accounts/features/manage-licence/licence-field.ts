import { Component, computed, inject, input, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AdminUser } from '../../../../shared/core/admin-user';
import { AdminActivity } from '../../../../shared/infrastructure/admin-activity';
import { AccountsApi } from '../../infrastructure/accounts.api';

/**
 * The federation licence number in the list of the members: the board reads it and changes it here,
 * the person changes their own from their profile.
 */
@Component({
  selector: 'app-licence-field',
  imports: [FormsModule],
  template: `
    @if (editing()) {
      <form class="stack stack-s" (ngSubmit)="save()">
        <input class="input" [id]="'lic-' + user().id" name="licence" [(ngModel)]="number" maxlength="20"
               placeholder="07123456" autocomplete="off" aria-label="Numéro de licence" />
        <span class="form-actions">
          <button class="btn btn-sm" type="submit" [class.is-busy]="busy()" [disabled]="activity.busy() !== null">Enregistrer</button>
          <button class="btn btn-ghost btn-sm" type="button" [disabled]="activity.busy() !== null" (click)="editing.set(false)">Annuler</button>
        </span>
      </form>
    } @else {
      <span class="row" style="--row-gap: var(--space-5)">
        @if (user().licence) { <span>{{ user().licence }}</span> } @else { <span class="small-text">non renseignée</span> }
        <button class="btn btn-ghost btn-sm" type="button" [disabled]="activity.busy() !== null" (click)="edit()">
          {{ user().licence ? 'Modifier' : 'Ajouter' }}
        </button>
      </span>
    }
  `,
  host: { style: 'display: block' },
})
export class LicenceField {
  readonly user = input.required<AdminUser>();
  protected readonly activity = inject(AdminActivity);
  private readonly api = inject(AccountsApi);
  protected readonly editing = signal(false);
  protected readonly busy = computed(() => this.activity.busy() === this.licenceKey());
  protected number = '';

  protected licenceKey(): string {
    return `${this.user().id}:licence`;
  }

  protected edit(): void {
    this.number = this.user().licence ?? '';
    this.editing.set(true);
  }

  protected async save(): Promise<void> {
    const u = this.user();
    const number = this.number.trim();
    const message = number ? `Licence de ${u.name} enregistrée.` : `Licence de ${u.name} retirée.`;
    if (await this.activity.run(this.licenceKey(), () => this.api.setLicence(u.id, number), message)) {
      this.editing.set(false);
    }
  }
}
