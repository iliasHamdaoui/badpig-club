import { Component, effect, inject, signal, untracked } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ListState, Page } from '../../../../shared/core/page';
import { formatDate, formatEuro } from '../../../../shared/core/format';
import { AdminActivity } from '../../../../shared/infrastructure/admin-activity';
import { Pager, SearchBox, SortHeader } from '../../../../shared/ui/list-controls';
import { Season, SeasonInput, defaultEnd, defaultName, nextDay } from '../../core/season';
import { MembershipsApi } from '../../infrastructure/memberships.api';
import { Crumb, PageHead } from '../../../../shared/ui/page-head';

/** Seasons of the club: dates, grace period and the two yearly fees (club licence holders / external members). */
@Component({
  selector: 'app-manage-seasons',
  imports: [FormsModule, SearchBox, SortHeader, Pager, PageHead, RouterLink],
  providers: [AdminActivity],
  host: { class: 'bleed' },
  template: `
    <main class="container stack stack-l" style="padding-block: var(--space-30) var(--space-60)">
      <app-page-head [trail]="trail" title="Saisons et cotisations" />

      @if (loaded() && !coversToday()) {
        <p class="error">Aucune saison ne couvre la date du jour : créez la saison en cours pour pouvoir valider des adhésions.</p>
      }

      <section class="card stack stack-s">
        <h2 class="card-title">Créer une saison</h2>
        <form class="new-season" (ngSubmit)="create()">
        <div>
          <label for="s-start">Début</label>
          <input class="input" id="s-start" name="startsOn" type="date" [ngModel]="draft.startsOn" (ngModelChange)="startChanged($event)" required />
        </div>
        <div>
          <label for="s-end">Fin</label>
          <input class="input" id="s-end" name="endsOn" type="date" [(ngModel)]="draft.endsOn" required />
        </div>
        <div>
          <label for="s-name">Nom</label>
          <input class="input" id="s-name" name="name" [(ngModel)]="draft.name" [placeholder]="placeholderName()" maxlength="60" autocomplete="off" />
        </div>
        <div>
          <label for="s-grace">Tolérance (jours)</label>
          <input class="input" id="s-grace" name="graceDays" type="number" min="0" max="120" [(ngModel)]="draft.graceDays" required />
        </div>
        <div>
          <label for="s-internal">Cotisation licencié du club (€)</label>
          <input class="input" id="s-internal" name="internalFee" type="number" min="0" step="0.01" [(ngModel)]="draft.internalFee" required />
        </div>
        <div>
          <label for="s-external">Cotisation licencié ailleurs (€)</label>
          <input class="input" id="s-external" name="externalFee" type="number" min="0" step="0.01" [(ngModel)]="draft.externalFee" required />
        </div>
          <button class="btn" type="submit" [class.is-busy]="creating()" [disabled]="creating()">{{ creating() ? 'Création…' : 'Créer' }}</button>
        </form>
        <p class="small-text">Par défaut une saison va du 1er septembre au 31 août. La tolérance est le nombre de jours après le début pendant lesquels l'adhésion de la saison précédente reste acceptée.</p>
      </section>

      @if (activity.message()) { <p class="success">{{ activity.message() }}</p> }
      @if (activity.error()) { <p class="error">{{ activity.error() }}</p> }

      <h2>Saisons</h2>
      <app-search-box label="Rechercher une saison…" (searched)="list.setSearch($event)" />
      @if (!loaded()) {
        <p>Chargement…</p>
      } @else if (seasons().items.length === 0) {
        <p>{{ list.search() ? 'Aucun résultat.' : 'Aucune saison pour le moment.' }}</p>
      } @else {
        <table>
          <thead>
            <tr>
              <th><app-sort-header [state]="list" field="name" label="Saison" /></th>
              <th><app-sort-header [state]="list" field="startsOn" label="Dates" /></th>
              <th>Tolérance</th>
              <th>Licencié du club</th>
              <th>Licencié ailleurs</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            @for (s of seasons().items; track s.id) {
              <tr>
                @if (editing() === s.id) {
                  <td><input class="input" [name]="'n-' + s.id" [(ngModel)]="edited.name" maxlength="60" aria-label="Nom de la saison" /></td>
                  <td>
                    <input class="input" [name]="'a-' + s.id" type="date" [(ngModel)]="edited.startsOn" aria-label="Début" />
                    <input class="input" [name]="'b-' + s.id" type="date" [(ngModel)]="edited.endsOn" aria-label="Fin" />
                  </td>
                  <td><input class="input" [name]="'g-' + s.id" type="number" min="0" max="120" [(ngModel)]="edited.graceDays" aria-label="Tolérance en jours" /></td>
                  <td><input class="input" [name]="'i-' + s.id" type="number" min="0" step="0.01" [(ngModel)]="edited.internalFee" aria-label="Cotisation licencié du club" /></td>
                  <td><input class="input" [name]="'e-' + s.id" type="number" min="0" step="0.01" [(ngModel)]="edited.externalFee" aria-label="Cotisation licencié ailleurs" /></td>
                } @else {
                  <td>{{ s.name }} @if (s.isCurrent) { <strong>(en cours)</strong> }</td>
                  <td>{{ date(s.startsOn) }} → {{ date(s.endsOn) }}</td>
                  <td>{{ s.graceDays }} j</td>
                  <td>{{ euro(s.internalFee) }}</td>
                  <td>{{ euro(s.externalFee) }}</td>
                }
                <td>
                  <div class="actions">
                    @if (editing() === s.id) {
                      <button class="btn btn-sm" [class.is-busy]="activity.busy() === s.id" [disabled]="activity.busy() !== null" (click)="save(s)">Enregistrer</button>
                      <button class="btn btn-ghost btn-sm" [disabled]="activity.busy() !== null" (click)="editing.set(null)">Annuler</button>
                    } @else if (toDelete() === s.id) {
                      <span>Supprimer cette saison ?</span>
                      <button class="btn btn-danger btn-sm" [class.is-busy]="activity.busy() === s.id" [disabled]="activity.busy() !== null" (click)="remove(s)">Oui, supprimer</button>
                      <button class="btn btn-ghost btn-sm" [disabled]="activity.busy() !== null" (click)="toDelete.set(null)">Annuler</button>
                    } @else {
                      <a class="btn btn-ghost btn-sm" [routerLink]="['/admin/seasons', s.id, 'holidays']">Vacances scolaires</a>
                      <button class="btn btn-ghost btn-sm" [disabled]="activity.busy() !== null" (click)="edit(s)">Modifier</button>
                      <button class="btn btn-danger btn-sm" [disabled]="activity.busy() !== null" (click)="toDelete.set(s.id)">Supprimer</button>
                    }
                  </div>
                </td>
              </tr>
            }
          </tbody>
        </table>
        <app-pager [page]="seasons().page" [totalPages]="seasons().totalPages" [totalCount]="seasons().totalCount" (pageChange)="list.goTo($event)" />
        <p class="small-text">Changer une cotisation ne modifie pas les adhésions déjà validées : elles gardent le montant au moment de la validation. Une saison qui a des adhésions validées ne peut pas être supprimée.</p>
      }
    </main>
  `,
  styles: `
    .new-season { display: flex; flex-wrap: wrap; gap: 1.2rem; align-items: flex-end; }
    .new-season div { flex: 1 1 19.2rem; }
    .new-season label { margin-top: 0; }
    td input { width: 100%; min-width: 9.6rem; }
  `,
})
export class ManageSeasonsPage {
  protected readonly trail: Crumb[] = [{ label: 'Administration', path: '/admin' }, { label: 'Saisons' }];
  private readonly api = inject(MembershipsApi);
  protected readonly activity = inject(AdminActivity);
  protected readonly list = new ListState('startsOn', 'desc');
  protected readonly seasons = signal<Page<Season>>({ items: [], page: 1, pageSize: 25, totalCount: 0, totalPages: 0 });
  protected readonly loaded = signal(false);
  protected readonly coversToday = signal(true);
  protected readonly editing = signal<string | null>(null);
  protected readonly toDelete = signal<string | null>(null);
  protected draft: SeasonInput = this.initialDraft();
  protected edited: SeasonInput = this.initialDraft();
  protected readonly creating = () => this.activity.busy() === 'create';
  protected readonly date = formatDate;
  protected readonly euro = formatEuro;

  constructor() {
    // Loads at start, after every action, and when the search, the sort or the page change.
    effect(() => {
      this.activity.changed();
      const query = this.list.query();
      untracked(() => void this.reload(query));
    });
  }

  private initialDraft(): SeasonInput {
    const now = new Date();
    const year = now.getMonth() >= 8 ? now.getFullYear() : now.getFullYear() - 1; // season opened on 1 September
    const startsOn = `${year}-09-01`;
    return { name: null, startsOn, endsOn: defaultEnd(startsOn), graceDays: 30, internalFee: null, externalFee: null };
  }

  private async reload(query: ReturnType<ListState['query']>): Promise<void> {
    try {
      const seasons = await this.api.seasons(query);
      if (seasons.items.length === 0 && seasons.page > 1) {
        this.list.goTo(seasons.totalPages); // the last page disappeared
        return;
      }
      this.seasons.set(seasons);
      // The create form starts after the latest season, with its fees as a base.
      const latest = (await this.api.seasons({ sort: 'startsOn', direction: 'desc', pageSize: 1 })).items[0];
      if (latest && this.draft.internalFee === null && this.draft.externalFee === null) {
        const startsOn = nextDay(latest.endsOn);
        this.draft = { name: null, startsOn, endsOn: defaultEnd(startsOn), graceDays: latest.graceDays, internalFee: latest.internalFee, externalFee: latest.externalFee };
      }
      const today = new Date().toISOString().slice(0, 10);
      this.coversToday.set(latest === undefined ? false : latest.startsOn <= today && today <= latest.endsOn);
      this.loaded.set(true);
    } catch {
      this.activity.error.set('Impossible de charger les saisons.');
    }
  }

  protected placeholderName(): string {
    return this.draft.startsOn && this.draft.endsOn ? defaultName(this.draft.startsOn, this.draft.endsOn) : '';
  }

  protected startChanged(startsOn: string): void {
    this.draft.startsOn = startsOn;
    if (startsOn) {
      this.draft.endsOn = defaultEnd(startsOn);
    }
  }

  protected async create(): Promise<void> {
    const name = this.draft.name?.trim() || this.placeholderName();
    const ok = await this.activity.run('create', () => this.api.createSeason({ ...this.draft, name: this.draft.name?.trim() || null }), `Saison « ${name} » créée.`, 'La création a échoué. Vérifiez les champs.');
    if (ok) {
      this.draft = { ...this.initialDraft(), internalFee: this.draft.internalFee, externalFee: this.draft.externalFee, graceDays: this.draft.graceDays };
    }
  }

  protected edit(s: Season): void {
    this.edited = { name: s.name, startsOn: s.startsOn, endsOn: s.endsOn, graceDays: s.graceDays, internalFee: s.internalFee, externalFee: s.externalFee };
    this.editing.set(s.id);
  }

  protected async save(s: Season): Promise<void> {
    const ok = await this.activity.run(s.id, () => this.api.updateSeason(s.id, this.edited), `Saison « ${this.edited.name} » enregistrée.`, "L'enregistrement a échoué. Vérifiez les champs.");
    if (ok) {
      this.editing.set(null);
    }
  }

  protected async remove(s: Season): Promise<void> {
    const ok = await this.activity.run(s.id, () => this.api.deleteSeason(s.id), `Saison « ${s.name} » supprimée.`);
    if (ok) {
      this.toDelete.set(null);
    }
  }
}
