import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { formatLongDate } from '../../../../shared/core/format';
import { AuthService } from '../../../../shared/infrastructure/auth.service';
import { FixtureForm, FixtureItem, Team, TeamMember, VENUE_LABELS, timeLabel } from '../../core/interclubs';
import { InterclubsApi } from '../../infrastructure/interclubs.api';
import { FixtureFormComponent } from '../fixture-form/fixture-form';
import { apiErrorMessage } from '../../../../shared/core/format';

/**
 * The page of one team: who plays in it, its fixtures, the dates waiting for answers, and the actions of its captain.
 * Everything about a team is gathered here instead of being spread over the three pages of the section.
 */
@Component({
  selector: 'app-interclubs-team',
  imports: [RouterLink, FixtureFormComponent],
  host: { class: 'bleed' },
  template: `
    <main class="container stack stack-l" style="padding-block: var(--space-30) var(--space-60)">
      <header class="page-head" style="padding-block: 0">
        <p class="breadcrumb"><a routerLink="/interclubs">Interclubs</a> / <a routerLink="/interclubs/teams">Équipes</a> / {{ team()?.name ?? '…' }}</p>
        <h1 style="font-size: var(--text-h2)">
          {{ team()?.name ?? 'Équipe' }}
        </h1>
        @if (team(); as t) {
          <p class="row">
            <span class="highlight highlight-neutral">Capitaine : {{ t.captainName ?? 'pas encore désigné' }}</span>
            @if (t.isPlayer) { <span class="highlight highlight-tertiary">Je joue dans cette équipe</span> }
            @if (t.isMyTeam) { <span class="highlight highlight-secondary">Je suis capitaine</span> }
          </p>
        }
      </header>

      @if (!loaded()) {
        <div class="stack" aria-hidden="true"><span class="skeleton" style="height: 10rem"></span><span class="skeleton" style="height: 10rem"></span></div>
      } @else if (!team()) {
        <p class="empty">
          <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><circle cx="12" cy="12" r="9"></circle><path d="M12 8v5M12 16h.01"></path></svg>
          <strong>Équipe introuvable</strong>
          <span>Elle a peut-être été supprimée. <a routerLink="/interclubs/teams">Retour aux équipes</a></span>
        </p>
      } @else {
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

        @if (canManage()) {
          @if (adding()) {
            <section class="card stack" aria-label="Ajouter une rencontre">
              <h2 class="card-title">Ajouter une rencontre à {{ team()!.name }}</h2>
              <app-fixture-form submitLabel="Ajouter" [busy]="busy()" (saved)="add($event.form)" (cancelled)="adding.set(false)" />
            </section>
          } @else {
            <nav class="actions row" aria-label="Actions de l'équipe">
              <button class="btn" type="button" (click)="startAdding()">
                <svg class="ic ic-s" viewBox="0 0 24 24" aria-hidden="true"><path d="M12 5v14M5 12h14"></path></svg>
                Ajouter une rencontre
              </button>
              <a class="btn btn-ghost" [routerLink]="['/interclubs/teams', id, 'members']">Gérer l'effectif</a>
            </nav>
          }
        }

        <section class="stack" aria-labelledby="t-next">
          <div class="block-head"><h2 id="t-next">Prochaines rencontres</h2><a class="link-action" routerLink="/interclubs" [queryParams]="{ team: id }">Tout le calendrier</a></div>
          @if (upcoming().length === 0) {
            <p class="empty"><strong>Aucune rencontre à venir</strong><span>Les dates se fixent depuis les disponibilités.</span></p>
          } @else {
            @for (f of upcoming(); track f.id) {
              <a class="match" [routerLink]="['/interclubs/fixtures', f.id]">
                <span class="team-tag" aria-hidden="true">{{ f.teamName }}</span>
                <span>
                  <span class="match-title">{{ f.opponent }}</span>
                  <span class="match-meta"><span class="highlight highlight-neutral">{{ venues[f.venue] }}</span></span>
                  <span class="match-meta"><time>{{ day(f.date) }}@if (f.startTime) { · {{ time(f.startTime) }} }</time>@if (f.place) { <span>{{ f.place }}</span> }</span>
                </span>
              </a>
            }
          }
        </section>

        <section class="stack" aria-labelledby="t-roster">
          <div class="block-head">
            <h2 id="t-roster">Effectif <span class="small-text">({{ members().length }})</span></h2>
            @if (canManage()) { <a class="link-action" [routerLink]="['/interclubs/teams', id, 'members']">Gérer</a> }
          </div>
          @if (members().length === 0) {
            <p class="empty">
              <strong>Aucun joueur</strong>
              <span>Seuls les joueurs de l'équipe répondent aux dates proposées.</span>
            </p>
          } @else {
            <ul class="chip-row">
              @for (m of members(); track m.accountId) {
                <li class="highlight" [class.highlight-tertiary]="m.isCaptain" [class.highlight-neutral]="!m.isCaptain">
                  {{ m.name }}@if (m.isCaptain) { · capitaine }
                </li>
              }
            </ul>
          }
        </section>

        @if (past().length > 0) {
          <section class="stack" aria-labelledby="t-past">
            <h2 id="t-past">Dernières rencontres</h2>
            @for (f of past(); track f.id) {
              <a class="match" [routerLink]="['/interclubs/fixtures', f.id]">
                <span class="team-tag" aria-hidden="true">{{ f.teamName }}</span>
                <span>
                  <span class="match-title">{{ f.opponent }}</span>
                  <span class="match-meta"><time>{{ day(f.date) }}</time><span class="highlight highlight-neutral">{{ venues[f.venue] }}</span></span>
                  <span class="match-meta">
                    @if (f.clubScore !== null) {
                      <span class="score">{{ f.clubScore }} – {{ f.opponentScore }}</span>
                      <span class="highlight" [class]="f.clubScore > f.opponentScore! ? 'highlight-success' : f.clubScore < f.opponentScore! ? 'highlight-neutral' : 'highlight-warning'">
                        {{ f.clubScore > f.opponentScore! ? 'Victoire' : f.clubScore < f.opponentScore! ? 'Défaite' : 'Égalité' }}
                      </span>
                    } @else { <span class="highlight highlight-warning">Résultat à saisir</span> }
                  </span>
                </span>
              </a>
            }
          </section>
        }
      }
    </main>
  `,
  styles: `ul { list-style: none; }`,
})
export class TeamPage {
  protected readonly id = inject(ActivatedRoute).snapshot.paramMap.get('id') ?? '';
  private readonly api = inject(InterclubsApi);
  private readonly auth = inject(AuthService);
  protected readonly team = signal<Team | null>(null);
  protected readonly upcoming = signal<FixtureItem[]>([]);
  protected readonly past = signal<FixtureItem[]>([]);
  protected readonly members = signal<TeamMember[]>([]);
  protected readonly loaded = signal(false);
  protected readonly adding = signal(false);
  protected readonly busy = signal(false);
  protected readonly message = signal('');
  protected readonly error = signal('');
  protected readonly venues = VENUE_LABELS;
  protected readonly day = formatLongDate;
  protected readonly time = timeLabel;

  constructor() {
    void this.load();
  }

  protected startAdding(): void {
    this.message.set('');
    this.error.set('');
    this.adding.set(true);
  }

  /** A fixture added here always belongs to this team: nothing else is offered. */
  protected async add(form: FixtureForm): Promise<void> {
    this.busy.set(true);
    this.error.set('');
    try {
      await this.api.createFixture(this.id, form);
      this.adding.set(false);
      this.message.set('Rencontre ajoutée.');
      await this.load();
    } catch (e) {
      this.error.set(apiErrorMessage(e, "La rencontre n'a pas pu être ajoutée."));
    } finally {
      this.busy.set(false);
    }
  }

  /** The captain of the team and the board manage it; the API checks it again. */
  protected canManage(): boolean {
    return this.auth.isBoard() || (this.team()?.isMyTeam ?? false);
  }

  private async load(): Promise<void> {
    try {
      // There is no endpoint for a single team: the club has a handful of them.
      const teams = await this.api.teams({ pageSize: 100 });
      this.team.set(teams.items.find((t) => t.id === this.id) ?? null);
      const [upcoming, past, members] = await Promise.all([
        this.api.fixtures({ teamId: this.id, when: 'upcoming', sort: 'date', direction: 'asc', pageSize: 5 }),
        this.api.fixtures({ teamId: this.id, when: 'past', sort: 'date', direction: 'desc', pageSize: 3 }),
        this.api.members(this.id, { sort: 'name', direction: 'asc', pageSize: 100 }),
      ]);
      this.upcoming.set(upcoming.items);
      this.past.set(past.items);
      this.members.set(members.items);
    } catch {
      this.team.set(this.team());
    } finally {
      this.loaded.set(true);
    }
  }
}
