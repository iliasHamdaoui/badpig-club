import { Component, effect, inject, signal, untracked } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ListState, Page } from '../../../../shared/core/page';
import { apiErrorMessage, formatLongDate } from '../../../../shared/core/format';
import { AuthService } from '../../../../shared/infrastructure/auth.service';
import { Pager, SearchBox } from '../../../../shared/ui/list-controls';
import { Tab, Tabs } from '../../../../shared/ui/tabs';
import { FixtureForm, FixtureItem, Team, VENUE_LABELS, timeLabel } from '../../core/interclubs';
import { INTERCLUBS_TABS } from '../../core/tabs';
import { InterclubsApi } from '../../infrastructure/interclubs.api';
import { SeasonFilter } from '../../infrastructure/season-filter';
import { SeasonFilterControl } from '../season-filter/season-filter';
import { FixtureFormComponent } from '../fixture-form/fixture-form';
import { Crumb, PageHead } from '../../../../shared/ui/page-head';

/** The calendar of the interclub fixtures, for every adherent; the captains (and the board) add fixtures to it. */
@Component({
  selector: 'app-interclubs-calendar',
  imports: [RouterLink, SearchBox, Pager, Tabs, FixtureFormComponent, PageHead, SeasonFilterControl],
  host: { class: 'bleed' },
  template: `
    <main class="container stack stack-l" style="padding-block: var(--space-30) var(--space-60)">
      <app-page-head [trail]="trail" title="Interclubs" lead="Les rencontres des équipes du club." />
      <app-tabs [tabs]="tabs" label="Pages des interclubs" />

      @if (message()) {
        <p class="alert alert-success">
          <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M5 12l5 5 9-10"></path></svg>
          <span>{{ message() }}</span>
        </p>
      }
      @if (error()) {
        <p class="alert alert-error">
          <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M8 3h8l5 5v8l-5 5H8l-5-5V8z"></path><path d="M12 8v5M12 16h.01"></path></svg>
          <span><span class="alert-title">Erreur.</span> {{ error() }}</span>
        </p>
      }

      @if (managedTeams().length > 0) {
        @if (adding()) {
          <section class="card stack" aria-label="Ajouter une rencontre">
            <h2 class="card-title">Ajouter une rencontre</h2>
            <app-fixture-form [teams]="managedTeams()" submitLabel="Ajouter" [busy]="busy()" (saved)="add($event)" (cancelled)="adding.set(false)" />
          </section>
        } @else {
          <p><button class="btn" (click)="adding.set(true)">
            <svg class="ic ic-s" viewBox="0 0 24 24" aria-hidden="true"><path d="M12 5v14M5 12h14"></path></svg>
            Ajouter une rencontre
          </button></p>
        }
      }

      <div class="stack" role="group" aria-label="Filtres">
        <app-season-filter />
        <div class="stack stack-s">
          <span class="label" id="f-period">Période</span>
          <div class="chip-row" role="group" aria-labelledby="f-period">
            @for (p of periods; track p.value) {
              <button class="chip" type="button" [attr.aria-pressed]="when() === p.value" (click)="setWhen(p.value)">{{ p.label }}</button>
            }
          </div>
        </div>
        @if (teams().length > 0) {
          <div class="stack stack-s">
            <span class="label" id="f-team">Équipe</span>
            <div class="chip-row" role="group" aria-labelledby="f-team">
              <button class="chip" type="button" [attr.aria-pressed]="teamId() === ''" (click)="setTeam('')">Toutes</button>
              @for (t of teams(); track t.id) {
                <button class="chip" type="button" [attr.aria-pressed]="teamId() === t.id" (click)="setTeam(t.id)">{{ t.name }}</button>
              }
            </div>
          </div>
        }
        <app-search-box label="Club adverse ou équipe" (searched)="list.setSearch($event)" />
      </div>

      @if (failed()) {
        <p class="alert alert-error">
          <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M8 3h8l5 5v8l-5 5H8l-5-5V8z"></path><path d="M12 8v5M12 16h.01"></path></svg>
          <span><span class="alert-title">Erreur.</span> Le calendrier n'a pas pu être chargé.</span>
        </p>
      } @else if (!loaded()) {
        <div class="stack" aria-hidden="true"><span class="skeleton" style="height: 9rem"></span><span class="skeleton" style="height: 9rem"></span></div>
      } @else if (fixtures().items.length === 0) {
        <p class="empty">
          <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><rect x="3" y="5" width="18" height="16" rx="2"></rect><path d="M8 3v4M16 3v4M3 11h18"></path></svg>
          <strong>Aucune rencontre</strong>
          <span>{{ list.search() ? 'Essayez une autre recherche.' : 'Rien pour cette période.' }}</span>
        </p>
      } @else {
        <ul class="cols cols-2" aria-label="Rencontres">
          @for (f of fixtures().items; track f.id) {
            <li>
              <a class="match" [class.match-own]="isMine(f)" [routerLink]="['/interclubs/fixtures', f.id]">
                <span class="team-tag" [attr.aria-label]="'Équipe ' + f.teamName">{{ f.teamName }}</span>
                <span>
                  <span class="match-title">{{ f.opponent }}</span>
                  <span class="match-meta">
                    @if (isMine(f)) { <span class="highlight highlight-tertiary">Mon équipe</span> }
                    <span class="highlight highlight-neutral">{{ venues[f.venue] }}</span>
                    @if (f.isLive) { <span class="highlight highlight-error">En direct</span> }
                  </span>
                  <span class="match-meta">
                    <time>{{ day(f.date) }}@if (f.startTime) { · {{ time(f.startTime) }} }</time>
                    @if (f.place) { <span>{{ f.place }}</span> }
                  </span>
                  <span class="match-meta">
                    @if (f.clubScore !== null) {
                      <span class="score"><span class="sr-only">{{ f.clubScore }} matchs gagnés, {{ f.opponentScore }} perdus</span><span aria-hidden="true">{{ f.clubScore }} – {{ f.opponentScore }}</span></span>
                      <span class="highlight" [class]="f.clubScore > f.opponentScore! ? 'highlight-success' : f.clubScore < f.opponentScore! ? 'highlight-neutral' : 'highlight-warning'">
                        {{ f.clubScore > f.opponentScore! ? 'Victoire' : f.clubScore < f.opponentScore! ? 'Défaite' : 'Égalité' }}
                      </span>
                    } @else if (f.decidedMatches > 0) {
                      <span class="score"><span aria-hidden="true">{{ f.clubMatches }} – {{ f.opponentMatches }}</span></span>
                      <span class="highlight highlight-warning">{{ f.decidedMatches }} match{{ f.decidedMatches > 1 ? 's' : '' }} sur 7</span>
                    }
                  </span>
                </span>
              </a>
            </li>
          }
        </ul>
        <app-pager [page]="fixtures().page" [totalPages]="fixtures().totalPages" [totalCount]="fixtures().totalCount" (pageChange)="list.goTo($event)" />
      }
    </main>
  `,
  styles: `
    ul { list-style: none; }
    .match { height: 100%; }
  `,
})
export class CalendarPage {
  protected readonly trail: Crumb[] = [];
  private readonly api = inject(InterclubsApi);
  private readonly auth = inject(AuthService);
  protected readonly seasons = inject(SeasonFilter);
  protected readonly list = new ListState('date', 'asc', 25);
  protected readonly when = signal<'live' | 'upcoming' | 'past' | ''>('upcoming');
  // A link from a team page carries its team: the calendar opens already filtered on it.
  protected readonly teamId = signal(inject(ActivatedRoute).snapshot.queryParamMap.get('team') ?? '');
  protected readonly teams = signal<Team[]>([]);
  protected readonly managedTeams = signal<Team[]>([]);
  protected readonly fixtures = signal<Page<FixtureItem>>({ items: [], page: 1, pageSize: 25, totalCount: 0, totalPages: 0 });
  protected readonly loaded = signal(false);
  protected readonly failed = signal(false);
  protected readonly adding = signal(false);
  protected readonly busy = signal(false);
  protected readonly message = signal('');
  protected readonly error = signal('');
  protected readonly venues = VENUE_LABELS;
  protected readonly day = formatLongDate;
  protected readonly time = timeLabel;
  protected readonly tabs: Tab[] = INTERCLUBS_TABS;
  protected readonly periods = [
    { value: 'upcoming', label: 'À venir' },
    { value: 'past', label: 'Passées' },
    { value: '', label: 'Toutes' },
  ];
  private readonly reloads = signal(0);

  constructor() {
    void this.seasons.load();
    effect(() => {
      const seasonId = this.seasons.selected();
      const ready = this.seasons.ready();
      const query = { ...this.list.query(), when: this.when(), teamId: this.teamId(), seasonId };
      this.reloads();
      if (!ready) {
        return;
      }

      untracked(() => {
        void this.loadTeams(seasonId);
        void this.load(query);
      });
    });
  }

  protected setWhen(value: string): void {
    this.when.set(value as 'live' | 'upcoming' | 'past' | '');
    this.list.sort.set('date');
    this.list.direction.set(value === 'past' ? 'desc' : 'asc');
    this.list.goTo(1);
  }

  /** A fixture of one of my teams, highlighted in the list. */
  protected isMine(fixture: FixtureItem): boolean {
    return this.teams().some((t) => t.id === fixture.teamId && (t.isPlayer || t.isMyTeam));
  }

  protected setTeam(value: string): void {
    this.teamId.set(value);
    this.list.goTo(1);
  }

  protected async add(event: { teamId: string; form: FixtureForm }): Promise<void> {
    this.message.set('');
    this.error.set('');
    this.busy.set(true);
    try {
      await this.api.createFixture(event.teamId, event.form);
      this.adding.set(false);
      this.message.set('Rencontre ajoutée au calendrier.');
      this.reloads.update((n) => n + 1);
    } catch (e) {
      this.error.set(apiErrorMessage(e, "Impossible d'ajouter la rencontre."));
    } finally {
      this.busy.set(false);
    }
  }

  private async loadTeams(seasonId: string): Promise<void> {
    try {
      const teams = (await this.api.teams({ pageSize: 100, seasonId })).items;
      // Changing season: a team of the previous one is no longer a possible filter.
      if (this.teamId() && !teams.some((t) => t.id === this.teamId())) {
        this.setTeam('');
      }

      this.teams.set(teams);
      // The board adds fixtures to any team; a captain only to their own.
      this.managedTeams.set(this.auth.isBoard() ? teams : teams.filter((t) => t.isMyTeam));
    } catch {
      this.teams.set([]);
    }
  }

  private async load(query: Parameters<InterclubsApi['fixtures']>[0]): Promise<void> {
    try {
      this.fixtures.set(await this.api.fixtures(query));
      this.loaded.set(true);
      this.failed.set(false);
    } catch {
      this.failed.set(true);
    }
  }
}
