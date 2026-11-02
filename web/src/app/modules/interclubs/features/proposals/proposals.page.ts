import { Component, effect, inject, signal, untracked } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ListState, Page } from '../../../../shared/core/page';
import { apiErrorMessage, formatLongDate } from '../../../../shared/core/format';
import { AuthService } from '../../../../shared/infrastructure/auth.service';
import { Pager, SearchBox } from '../../../../shared/ui/list-controls';
import { Proposal, ProposalForm, Team, VENUE_LABELS, Venue, emptyProposalForm, timeLabel } from '../../core/interclubs';
import { InterclubsApi } from '../../infrastructure/interclubs.api';

/** The dates offered by the captains for their next fixtures; a captain (or the board) offers new ones here. */
@Component({
  selector: 'app-interclubs-proposals',
  imports: [FormsModule, RouterLink, SearchBox, Pager],
  template: `
    <section class="card">
      <h1>Interclubs : dates proposées</h1>
      <p><a routerLink="/interclubs">← Calendrier des rencontres</a> · <a routerLink="/interclubs/teams">Équipes et capitaines</a></p>

      @if (message()) { <p class="success">{{ message() }}</p> }
      @if (error()) { <p class="error">{{ error() }}</p> }

      @if (managedTeams().length > 0) {
        @if (adding()) {
          <h2>Proposer des dates</h2>
          <form class="form" (ngSubmit)="create()">
            <div>
              <label for="pr-team">Équipe</label>
              <select id="pr-team" name="team" [ngModel]="teamId()" (ngModelChange)="teamId.set($event)" required>
                @for (t of managedTeams(); track t.id) { <option [value]="t.id">{{ t.name }}</option> }
              </select>
            </div>
            <div><label for="pr-opponent">Club adverse</label><input id="pr-opponent" name="opponent" [(ngModel)]="form.opponent" maxlength="100" required autocomplete="off" /></div>
            <div>
              <label for="pr-venue">Lieu de la rencontre</label>
              <select id="pr-venue" name="venue" [(ngModel)]="form.venue">
                @for (v of venueList; track v[0]) { <option [value]="v[0]">{{ v[1] }}</option> }
              </select>
            </div>
            <div><label for="pr-place">Gymnase ou adresse (facultatif)</label><input id="pr-place" name="place" [(ngModel)]="form.place" maxlength="200" autocomplete="off" /></div>

            <fieldset>
              <legend>Dates proposées (2 au moins)</legend>
              @for (d of form.dates; track $index) {
                <div class="date-row">
                  <input type="date" [name]="'date' + $index" [attr.aria-label]="'Date ' + ($index + 1)" [(ngModel)]="d.date" required />
                  <input type="time" [name]="'time' + $index" [attr.aria-label]="'Heure ' + ($index + 1)" [(ngModel)]="d.startTime" />
                  @if (form.dates.length > 2) { <button type="button" class="btn btn-ghost btn-sm" (click)="removeDate($index)">Retirer</button> }
                </div>
              }
              @if (form.dates.length < 10) { <button type="button" class="btn btn-ghost btn-sm" (click)="addDate()">+ Ajouter une date</button> }
            </fieldset>

            <div class="actions">
              <button type="submit" [class.is-busy]="busy()" [disabled]="busy() || !canCreate()">Proposer ces dates</button>
              <button type="button" class="btn btn-ghost" [disabled]="busy()" (click)="adding.set(false)">Annuler</button>
            </div>
          </form>
        } @else {
          <p><button (click)="startAdding()">Proposer des dates</button></p>
        }
      }

      <div class="filters">
        <app-search-box label="Rechercher un club ou une équipe…" (searched)="list.setSearch($event)" />
        <select aria-label="État" (change)="setStatus($any($event.target).value)">
          <option value="open" [selected]="status() === 'open'">En attente de choix</option>
          <option value="chosen" [selected]="status() === 'chosen'">Date choisie</option>
          <option value="" [selected]="status() === ''">Toutes</option>
        </select>
      </div>

      @if (failed()) {
        <p class="error">Impossible de charger les propositions.</p>
      } @else if (!loaded()) {
        <p>Chargement…</p>
      } @else if (proposals().items.length === 0) {
        <p>Aucune proposition de dates.</p>
      } @else {
        @for (p of proposals().items; track p.id) {
          <article class="proposal">
            <h3>{{ p.teamName }} contre {{ p.opponent }} <span class="small-text">({{ venues[p.venue] }})</span></h3>
            <ul>
              @for (d of p.dates; track d.id) {
                <li>
                  {{ day(d.date) }}@if (d.startTime) { à {{ time(d.startTime) }} } :
                  <span class="small-text">{{ d.yes }} disponible{{ d.yes > 1 ? 's' : '' }}, {{ d.maybe }} peut-être, {{ d.no }} indisponible{{ d.no > 1 ? 's' : '' }}</span>
                </li>
              }
            </ul>
            <p class="actions">
              @if (p.isOpen) {
                <a class="btn small" [class.btn-ghost]="!p.canAnswer" [routerLink]="['/interclubs/proposals', p.id]">{{ p.canAnswer ? 'Répondre' : 'Voir' }}</a>
              } @else {
                <span class="badge">Date choisie</span>
                @if (p.fixtureId) { <a class="btn secondary small" [routerLink]="['/interclubs/fixtures', p.fixtureId]">Voir la rencontre</a> }
                <a class="btn secondary small" [routerLink]="['/interclubs/proposals', p.id]">Voir les réponses</a>
              }
            </p>
          </article>
        }
        <app-pager [page]="proposals().page" [totalPages]="proposals().totalPages" [totalCount]="proposals().totalCount" (pageChange)="list.goTo($event)" />
      }
    </section>
  `,
  styles: `
    .form { display: grid; gap: 0.4rem; max-width: 44.8rem; margin: 0.8rem 0 1.6rem; }
    fieldset { border: 0.1rem solid var(--input-border); border-radius: var(--radius-m); margin: 1.2rem 0; }
    .date-row { display: flex; gap: 0.8rem; align-items: center; margin-bottom: 0.8rem; }
    .proposal { border: 0.1rem solid var(--color-border); border-radius: var(--radius-m); padding: 0.8rem 1.6rem; margin: 1.6rem 0; }
  `,
})
export class ProposalsPage {
  private readonly api = inject(InterclubsApi);
  private readonly auth = inject(AuthService);
  protected readonly list = new ListState('createdAt', 'desc', 10);
  protected readonly status = signal<'open' | 'chosen' | ''>('open');
  protected readonly proposals = signal<Page<Proposal>>({ items: [], page: 1, pageSize: 10, totalCount: 0, totalPages: 0 });
  protected readonly managedTeams = signal<Team[]>([]);
  protected readonly teamId = signal('');
  protected readonly loaded = signal(false);
  protected readonly failed = signal(false);
  protected readonly adding = signal(false);
  protected readonly busy = signal(false);
  protected readonly message = signal('');
  protected readonly error = signal('');
  protected readonly venues = VENUE_LABELS;
  protected readonly venueList = Object.entries(VENUE_LABELS) as [Venue, string][];
  protected readonly day = formatLongDate;
  protected readonly time = timeLabel;
  protected form: ProposalForm = emptyProposalForm();
  private readonly reloads = signal(0);

  constructor() {
    void this.loadTeams();
    effect(() => {
      const query = { ...this.list.query(), status: this.status() };
      this.reloads();
      untracked(() => void this.load(query));
    });
  }


  protected setStatus(value: string): void {
    this.status.set(value as 'open' | 'chosen' | '');
    this.list.goTo(1);
  }

  protected startAdding(): void {
    this.form = emptyProposalForm();
    this.teamId.set(this.managedTeams()[0]?.id ?? '');
    this.adding.set(true);
  }

  protected addDate(): void {
    this.form.dates.push({ date: '', startTime: '' });
  }

  protected removeDate(index: number): void {
    this.form.dates.splice(index, 1);
  }

  protected canCreate(): boolean {
    return !!this.teamId() && !!this.form.opponent.trim() && this.form.dates.length >= 2 && this.form.dates.every((d) => !!d.date);
  }

  protected async create(): Promise<void> {
    this.message.set('');
    this.error.set('');
    this.busy.set(true);
    try {
      await this.api.createProposal(this.teamId(), this.form);
      this.adding.set(false);
      this.message.set('Dates proposées : les adhérents peuvent répondre.');
      this.reloads.update((n) => n + 1);
    } catch (e) {
      this.error.set(apiErrorMessage(e, 'Impossible de proposer ces dates.'));
    } finally {
      this.busy.set(false);
    }
  }

  private async loadTeams(): Promise<void> {
    try {
      const teams = (await this.api.teams({ pageSize: 100 })).items;
      this.managedTeams.set(this.auth.isBoard() ? teams : teams.filter((t) => t.isMyTeam));
    } catch {
      this.managedTeams.set([]);
    }
  }

  private async load(query: Parameters<InterclubsApi['proposals']>[0]): Promise<void> {
    try {
      this.proposals.set(await this.api.proposals(query));
      this.loaded.set(true);
      this.failed.set(false);
    } catch {
      this.failed.set(true);
    }
  }
}
