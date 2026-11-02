import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FixtureItem, VENUE_LABELS, timeLabel } from '../../core/interclubs';
import { InterclubsApi } from '../../infrastructure/interclubs.api';

/** How often the banner asks the API again while the person stays on the site. */
const REFRESH_MS = 60_000;

/**
 * "Live" banner: the fixtures being played right now (the API decides), with the score as it is entered and a link to the fixture.
 * Shows nothing at all when there is none, or when the person may not read the calendar.
 */
@Component({
  selector: 'app-live-fixtures',
  imports: [RouterLink],
  template: `
    @if (fixtures().length > 0) {
      <aside class="live" aria-label="Rencontre en cours">
        @for (f of fixtures(); track f.id) {
          <a class="row" [routerLink]="['/interclubs/fixtures', f.id]">
            <span class="live-dot" aria-hidden="true">●</span>
            <span class="tag">En direct</span>
            <span class="who">{{ f.teamName }} contre {{ f.opponent }}</span>
            <span class="score">{{ f.clubMatches }} – {{ f.opponentMatches }}</span>
            <span class="detail">
              @if (f.decidedMatches === 0) { à jouer } @else { {{ f.decidedMatches }} match{{ f.decidedMatches > 1 ? 's' : '' }} sur 7 }
              · {{ venues[f.venue] }}@if (f.startTime) { · {{ time(f.startTime) }} }
            </span>
            <span class="go">Suivre →</span>
          </a>
        }
      </aside>
    }
  `,
  styles: `
    .live { background: var(--primary-shade); color: var(--color-text-inverse); }
    .row { display: flex; flex-wrap: wrap; align-items: baseline; gap: var(--space-5) var(--space-15); width: var(--container-width); max-width: var(--container-max); margin-inline: auto; padding-block: var(--space-10); color: var(--color-text-inverse); text-decoration: none; min-height: var(--tap-min); }
    .row:hover { color: var(--color-text-inverse); }
    .live-dot { animation: pulse 2s ease-in-out infinite; }
    .tag { font-weight: var(--weight-bold); text-transform: uppercase; font-size: var(--text-small); letter-spacing: 0.06em; }
    .who { font-weight: var(--weight-bold); }
    .score { font-weight: var(--weight-bold); font-variant-numeric: tabular-nums; }
    .detail { font-size: var(--text-small); }
    .go { margin-left: auto; font-size: var(--text-small); text-decoration: underline; }
    @keyframes pulse { 50% { opacity: .3; } }
    @media (prefers-reduced-motion: reduce) { .live-dot { animation: none; } }
  `,
})
export class LiveFixturesBanner implements OnInit {
  private readonly api = inject(InterclubsApi);
  private readonly destroyed = inject(DestroyRef);
  protected readonly fixtures = signal<FixtureItem[]>([]);
  protected readonly venues = VENUE_LABELS;
  protected readonly time = timeLabel;

  ngOnInit(): void {
    void this.load();
    const timer = setInterval(() => void this.load(), REFRESH_MS);
    this.destroyed.onDestroy(() => clearInterval(timer));
  }

  private async load(): Promise<void> {
    try {
      this.fixtures.set((await this.api.fixtures({ when: 'live', sort: 'date', direction: 'asc', pageSize: 5 })).items);
    } catch {
      // A person who may not read the calendar, or an API out of reach: the banner simply stays hidden.
      this.fixtures.set([]);
    }
  }
}
