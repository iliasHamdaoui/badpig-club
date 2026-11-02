import { Component, effect, inject, signal, untracked } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ListState, Page } from '../../../../shared/core/page';
import { formatLongDate } from '../../../../shared/core/format';
import { AdminUser } from '../../../../shared/core/admin-user';
import { AdminActivity } from '../../../../shared/infrastructure/admin-activity';
import { AuthService } from '../../../../shared/infrastructure/auth.service';
import { UsersDirectory } from '../../../../shared/infrastructure/users-directory';
import { Pager, SearchBox } from '../../../../shared/ui/list-controls';
import { Tabs } from '../../../../shared/ui/tabs';
import { FixtureItem, Team, VENUE_LABELS, timeLabel } from '../../core/interclubs';
import { INTERCLUBS_TABS } from '../../core/tabs';
import { InterclubsApi } from '../../infrastructure/interclubs.api';
import { SeasonFilter } from '../../infrastructure/season-filter';
import { SeasonFilterControl } from '../season-filter/season-filter';
import { MenuDetails } from '../../../../shared/ui/menu-details';
import { Crumb, PageHead } from '../../../../shared/ui/page-head';

/** The teams of the club and their captains, for every adherent; the board creates the teams and designates the captains. */
@Component({
  selector: 'app-interclubs-teams',
  imports: [FormsModule, RouterLink, SearchBox, Pager, Tabs, MenuDetails, PageHead, SeasonFilterControl],
  host: { class: 'bleed' },
  providers: [AdminActivity],
  template: `
    <main class="container stack stack-l" style="padding-block: var(--space-30) var(--space-60)">
      <app-page-head [trail]="trail" title="Équipes" lead="Chaque équipe a sa page : son effectif et ses rencontres." />
      <app-tabs [tabs]="tabs" label="Pages des interclubs" />

      @if (activity.message()) {
        <p class="alert alert-success"><svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M5 12l5 5 9-10"></path></svg><span>{{ activity.message() }}</span></p>
      }
      @if (activity.error()) {
        <p class="alert alert-error"><svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M8 3h8l5 5v8l-5 5H8l-5-5V8z"></path><path d="M12 8v5M12 16h.01"></path></svg><span><span class="alert-title">Erreur.</span> {{ activity.error() }}</span></p>
      }

      @if (auth.isBoard()) {
        <details class="card panel">
          <summary class="card-title">
            <svg class="ic ic-s chevron" viewBox="0 0 24 24" aria-hidden="true"><path d="M9 6l6 6-6 6"></path></svg>
            Créer une équipe
          </summary>
          <form class="cols cols-2" style="align-items: end" (ngSubmit)="create()">
            <div class="field">
              <label class="label" for="team-name">Nom (D6, D5, PR…)</label>
              <input class="input" id="team-name" name="name" [(ngModel)]="newName" maxlength="30" required autocomplete="off" />
            </div>
            <div class="row" style="align-items: flex-end; flex-wrap: nowrap">
              <div class="field" style="flex: 1; min-width: 0">
                <label class="label" for="team-season">Saison</label>
                <select class="select" id="team-season" name="season" [(ngModel)]="newSeason" required>
                  @for (s of seasons.seasons(); track s.id) { <option [value]="s.id">{{ s.name }}</option> }
                </select>
              </div>
              <button class="btn" type="submit" style="flex: none" [class.is-busy]="activity.busy() === 'create'" [disabled]="activity.busy() !== null || !newSeason">Créer</button>
            </div>
          </form>
        </details>
      }

      <div class="stack" role="group" aria-label="Filtres">
        <app-season-filter />
        <app-search-box label="Nom d'équipe ou capitaine" (searched)="list.setSearch($event)" />
      </div>

      @if (failed()) {
        <p class="alert alert-error"><svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M8 3h8l5 5v8l-5 5H8l-5-5V8z"></path><path d="M12 8v5M12 16h.01"></path></svg><span><span class="alert-title">Erreur.</span> Les équipes n'ont pas pu être chargées.</span></p>
      } @else if (!loaded()) {
        <div class="stack" aria-hidden="true"><span class="skeleton" style="height: 12rem"></span><span class="skeleton" style="height: 12rem"></span></div>
      } @else if (teams().items.length === 0) {
        <p class="empty">
          <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M12 3l8 4v5c0 5-3.5 8-8 9-4.5-1-8-4-8-9V7z"></path></svg>
          <strong>Aucune équipe</strong>
          <span>{{ auth.isBoard() ? 'Créez la première équipe ci-dessus.' : 'Le bureau en créera une avant le début de la saison.' }}</span>
        </p>
      } @else {
        <ul class="cols cols-2" aria-label="Équipes">
          @for (t of teams().items; track t.id) {
            <li>
              <article class="card stack" [class.card-own]="t.isMyTeam || t.isPlayer">
                <div class="block-head">
                  <h2 style="font-size: var(--text-h4)"><a [routerLink]="['/interclubs/teams', t.id]">{{ t.name }}</a></h2>
                  @if (t.isMyTeam) { <span class="highlight highlight-secondary">Je suis capitaine</span> }
                  @else if (t.isPlayer) { <span class="highlight highlight-tertiary">Je joue ici</span> }
                </div>

                <dl class="stack stack-s" style="margin: 0">
                  <div class="kv"><dt>Capitaine</dt><dd>{{ t.captainName ?? 'pas encore désigné' }}</dd></div>
                  <div class="kv"><dt>Effectif</dt><dd>{{ t.memberCount }} joueur{{ t.memberCount > 1 ? 's' : '' }}</dd></div>
                  <div class="kv">
                    <dt>Prochaine rencontre</dt>
                    <dd>
                      @if (nextFixture(t.id); as f) {
                        {{ day(f.date) }}@if (f.startTime) { · {{ time(f.startTime) }} }<br />
                        <span class="small-text">{{ f.opponent }} · {{ venues[f.venue] }}</span>
                      } @else { <span class="small-text">aucune</span> }
                    </dd>
                  </div>
                </dl>

                <p class="actions">
                  <a class="btn btn-sm" [routerLink]="['/interclubs/teams', t.id]">Voir l'équipe</a>
                  @if (auth.isBoard()) {
                    <details class="more">
                      <summary class="btn btn-ghost btn-sm">Gérer</summary>
                      <div class="more-panel stack stack-s">
                        <div class="field">
                          <label class="label" [attr.for]="'cap-' + t.id">Capitaine</label>
                          <select class="select input" [id]="'cap-' + t.id" [disabled]="activity.busy() !== null" (change)="pick(t.id, $any($event.target).value)">
                            <option value="">— Choisir —</option>
                            @for (c of candidates(); track c.id) { <option [value]="c.id" [selected]="picked()[t.id] === c.id">{{ c.name }}</option> }
                          </select>
                        </div>
                        <button class="btn btn-sm" type="button" [disabled]="activity.busy() !== null || !picked()[t.id]" (click)="assign(t)">Désigner capitaine</button>
                        @if (t.captainId) { <button class="btn btn-ghost btn-sm" type="button" [disabled]="activity.busy() !== null" (click)="removeCaptain(t)">Retirer le capitaine</button> }
                        <button class="btn btn-ghost btn-sm" type="button" (click)="startEdit(t)">Renommer l'équipe</button>
                        <button class="btn btn-danger btn-sm" type="button" [disabled]="activity.busy() !== null" (click)="remove(t)">Supprimer l'équipe</button>
                      </div>
                    </details>
                  }
                </p>

                @if (editing() === t.id) {
                  <form class="stack stack-s" (ngSubmit)="saveEdit(t)">
                    <div class="field"><label class="label" [attr.for]="'name-' + t.id">Nom</label><input class="input" [id]="'name-' + t.id" name="editName" [(ngModel)]="editName" maxlength="30" required /></div>
                    <div class="field">
                      <label class="label" [attr.for]="'season-' + t.id">Saison</label>
                      <select class="select" [id]="'season-' + t.id" name="editSeason" [(ngModel)]="editSeason" required>
                        @for (s of seasons.seasons(); track s.id) { <option [value]="s.id">{{ s.name }}</option> }
                      </select>
                    </div>
                    <p class="actions">
                      <button class="btn btn-sm" type="submit" [class.is-busy]="activity.busy() === t.id" [disabled]="activity.busy() !== null">Enregistrer</button>
                      <button class="btn btn-ghost btn-sm" type="button" (click)="editing.set(null)">Annuler</button>
                    </p>
                  </form>
                }
              </article>
            </li>
          }
        </ul>
        <app-pager [page]="teams().page" [totalPages]="teams().totalPages" [totalCount]="teams().totalCount" (pageChange)="list.goTo($event)" />
      }
    </main>
  `,
  styles: `
    ul { list-style: none; }
    .card { height: 100%; }
  `,
})
export class TeamsPage {
  protected readonly trail: Crumb[] = [{ label: 'Interclubs', path: '/interclubs' }, { label: 'Équipes' }];
  protected readonly auth = inject(AuthService);
  protected readonly activity = inject(AdminActivity);
  private readonly api = inject(InterclubsApi);
  private readonly users = inject(UsersDirectory);
  protected readonly seasons = inject(SeasonFilter);
  protected readonly list = new ListState('name', 'asc', 25);
  protected readonly teams = signal<Page<Team>>({ items: [], page: 1, pageSize: 25, totalCount: 0, totalPages: 0 });
  protected readonly loaded = signal(false);
  protected readonly failed = signal(false);
  /** Active club members the board can designate as captain. */
  protected readonly candidates = signal<AdminUser[]>([]);
  /** The captain chosen in the list of each team, before the board confirms. */
  protected readonly picked = signal<Record<string, string>>({});
  protected readonly editing = signal<string | null>(null);
  protected newName = '';
  protected newSeason = '';
  protected editName = '';
  protected editSeason = '';
  protected readonly tabs = INTERCLUBS_TABS;
  protected readonly venues = VENUE_LABELS;
  protected readonly day = formatLongDate;
  protected readonly time = timeLabel;
  /** The next fixture of each team, for the overview the board asked for. */
  private readonly nextFixtures = signal<FixtureItem[]>([]);

  constructor() {
    if (this.auth.isBoard()) {
      void this.loadCandidates();
    }
    void this.seasons.load();
    effect(() => {
      const query = this.list.query();
      const seasonId = this.seasons.selected();
      const ready = this.seasons.ready();
      this.activity.changed();
      if (!ready) {
        return;
      }

      // The creation form follows the season being looked at.
      untracked(() => {
        this.newSeason ||= seasonId;
        void this.load({ ...query, seasonId });
        void this.loadNextFixtures(seasonId);
      });
    });
  }

  protected nextFixture(teamId: string): FixtureItem | undefined {
    return this.nextFixtures().find((f) => f.teamId === teamId);
  }

  protected pick(teamId: string, captainId: string): void {
    this.picked.update((p) => ({ ...p, [teamId]: captainId }));
  }

  protected async create(): Promise<void> {
    const name = this.newName.trim();
    if (await this.activity.run('create', () => this.api.createTeam(name, this.newSeason.trim()), `Équipe ${name} créée.`)) {
      this.newName = '';
    }
  }

  protected startEdit(team: Team): void {
    this.editName = team.name;
    this.editSeason = team.seasonId;
    this.editing.set(team.id);
  }

  protected async saveEdit(team: Team): Promise<void> {
    if (await this.activity.run(team.id, () => this.api.updateTeam(team.id, this.editName.trim(), this.editSeason.trim()), 'Équipe modifiée.')) {
      this.editing.set(null);
    }
  }

  protected async remove(team: Team): Promise<void> {
    await this.activity.run(team.id, () => this.api.deleteTeam(team.id), `Équipe ${team.name} supprimée.`);
  }

  protected async assign(team: Team): Promise<void> {
    const captainId = this.picked()[team.id];
    if (captainId) {
      await this.activity.run(team.id, () => this.api.assignCaptain(team.id, captainId), `Capitaine de ${team.name} désigné.`);
    }
  }

  protected async removeCaptain(team: Team): Promise<void> {
    await this.activity.run(team.id, () => this.api.removeCaptain(team.id), `${team.name} n'a plus de capitaine.`);
  }

  /** The fixtures to come, all teams together: each card shows the next one of its team. */
  private async loadNextFixtures(seasonId: string): Promise<void> {
    try {
      const page = await this.api.fixtures({ when: 'upcoming', sort: 'date', direction: 'asc', pageSize: 100, seasonId });
      this.nextFixtures.set(page.items);
    } catch {
      this.nextFixtures.set([]);
    }
  }

  /** Every active person of the club, whatever the number of pages: the captain can be any adherent. */
  private async loadCandidates(): Promise<void> {
    try {
      const everyone: AdminUser[] = [];
      let page = 1;
      let totalPages = 1;
      do {
        const result = await this.users.users({ sort: 'name', direction: 'asc', page, pageSize: 100 });
        everyone.push(...result.items);
        totalPages = result.totalPages;
        page++;
      } while (page <= totalPages);
      this.candidates.set(everyone.filter((u) => u.status === 'active' && u.groups.length > 0));
    } catch {
      this.candidates.set([]);
    }
  }

  private async load(query: Parameters<InterclubsApi['teams']>[0]): Promise<void> {
    try {
      this.teams.set(await this.api.teams(query));
      this.loaded.set(true);
      this.failed.set(false);
    } catch {
      this.failed.set(true);
    }
  }
}
