import { Component, OnInit, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FixtureItem, Proposal, VENUE_LABELS, timeLabel } from '../../core/interclubs';
import { InterclubsApi } from '../../infrastructure/interclubs.api';

/** "sam. 7 nov." : a day written short, for a dense list. */
function shortDate(iso: string): string {
  return new Date(iso).toLocaleDateString('fr-FR', { weekday: 'short', day: 'numeric', month: 'short' });
}

/** The next fixtures of the club in date order, those of my own teams marked, for the dashboard of a member. */
@Component({
  selector: 'app-next-fixtures',
  imports: [RouterLink],
  template: `
    @if (!loaded()) {
      <span class="skeleton" style="height: 8rem" aria-hidden="true"></span>
    } @else if (fixtures().length === 0) {
      <p class="empty">
        <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><rect x="3" y="5" width="18" height="16" rx="2"></rect><path d="M8 3v4M16 3v4M3 11h18"></path></svg>
        <strong>Aucune rencontre à venir</strong>
        <span>Le calendrier se remplira dès que les capitaines auront fixé les dates.</span>
      </p>
    } @else {
      @for (f of fixtures(); track f.id) {
        <a class="match" [class.match-own]="isMine(f)" [routerLink]="['/interclubs/fixtures', f.id]">
          <span class="team-tag" [attr.aria-label]="'Équipe ' + f.teamName">{{ f.teamName }}</span>
          <span>
            <span class="match-title">{{ f.opponent }}</span>
            <span class="match-meta">
              @if (isMine(f)) { <span class="highlight highlight-tertiary">Mon équipe</span> }
              <span class="highlight highlight-neutral">{{ venues[f.venue] }}</span>
            </span>
            <span class="match-meta">
              <time>{{ day(f.date) }} · {{ f.startTime ? time(f.startTime) : 'heure à confirmer' }}</time>
              @if (f.place) { <span>{{ f.place }}</span> }
            </span>
          </span>
        </a>
      }
    }
  `,
  styles: `:host { display: grid; gap: var(--space-10); }`,
})
export class NextFixtures implements OnInit {
  readonly limit = input(3);
  private readonly api = inject(InterclubsApi);
  protected readonly fixtures = signal<FixtureItem[]>([]);
  protected readonly loaded = signal(false);
  protected readonly venues = VENUE_LABELS;
  protected readonly day = shortDate;
  protected readonly time = timeLabel;
  private readonly myTeams = signal<string[]>([]);

  async ngOnInit(): Promise<void> {
    try {
      const teams = await this.api.teams({ pageSize: 100 });
      this.myTeams.set(teams.items.filter((t) => t.isPlayer || t.isMyTeam).map((t) => t.id));
      const page = await this.api.fixtures({ when: 'upcoming', sort: 'date', direction: 'asc', pageSize: 20 });
      // Chronological order, as the API returns it; my own teams are only marked, not moved up.
      this.fixtures.set(page.items.slice(0, this.limit()));
    } catch {
      this.fixtures.set([]);
    } finally {
      this.loaded.set(true);
    }
  }

  protected isMine(fixture: FixtureItem): boolean {
    return this.myTeams().includes(fixture.teamId);
  }
}

/** The date proposals of my teams where I have not answered every date yet. */
@Component({
  selector: 'app-pending-answers',
  imports: [RouterLink],
  template: `
    @if (pending().length > 0) {
      <section class="stack" aria-labelledby="t-pending">
        <div class="block-head">
          <h2 id="t-pending">À répondre</h2>
          <span class="highlight highlight-warning">{{ pending().length }} en attente</span>
        </div>
        @for (p of pending(); track p.id) {
          <div class="card stack stack-s waiting">
            <p><strong>Disponibilités {{ p.teamName }}</strong><br /><span class="small-text">Contre {{ p.opponent }}, {{ p.dates.length }} dates proposées</span></p>
            <p><a class="btn btn-sm" [routerLink]="['/interclubs/proposals', p.id]">Répondre</a></p>
          </div>
        }
      </section>
    }
  `,
  styles: `
    :host { display: block; }
    .waiting { border-width: 0.2rem; border-color: var(--alert-warning-border); }
  `,
})
export class PendingAnswers implements OnInit {
  private readonly api = inject(InterclubsApi);
  protected readonly pending = signal<Proposal[]>([]);

  async ngOnInit(): Promise<void> {
    try {
      const page = await this.api.proposals({ status: 'open', pageSize: 20 });
      this.pending.set(page.items.filter((p) => p.canAnswer && p.dates.some((d) => d.myAnswer === null)));
    } catch {
      this.pending.set([]);
    }
  }
}
