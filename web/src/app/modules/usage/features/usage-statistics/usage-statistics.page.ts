import { Component, computed, effect, inject, signal, untracked } from '@angular/core';
import { ListState, Page } from '../../../../shared/core/page';
import { Pager, SearchBox, SortField, SortSelect } from '../../../../shared/ui/list-controls';
import { Crumb, PageHead } from '../../../../shared/ui/page-head';
import { PERIODS, UsageDay, UsageItem, pageName } from '../../core/usage-item';
import { UsageApi } from '../../infrastructure/usage.api';
import { DailyChart } from './daily-chart';
import { TopPages, duration } from './top-pages';

/** Which pages of the website are the most opened and for how long: anonymous figures, for the board. */
@Component({
  selector: 'app-usage-statistics',
  imports: [PageHead, SearchBox, SortSelect, Pager, DailyChart, TopPages],
  host: { class: 'bleed' },
  template: `
    <main class="container stack stack-l" style="padding-block: var(--space-30) var(--space-60)">
      <app-page-head [trail]="trail" title="Usage du site" lead="Les pages les plus consultées et le temps qu'on y passe. Les chiffres sont anonymes : on ne sait pas qui a regardé quoi." />

      <div class="chip-row" role="group" aria-label="Période">
        @for (p of periods; track p) {
          <button type="button" class="chip" [attr.aria-pressed]="days() === p" (click)="setDays(p)">{{ periodLabel(p) }}</button>
        }
      </div>

      @if (error()) {
        <p class="alert alert-error" role="alert"><svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M8 3h8l5 5v8l-5 5H8l-5-5V8z"></path><path d="M12 8v5M12 16h.01"></path></svg><span>{{ error() }}</span></p>
      }

      <div class="grid-auto" style="--col-min: 20rem">
        <p class="card kpi"><span class="eyebrow">Consultations</span><strong>{{ totalViews() }}</strong></p>
        <p class="card kpi"><span class="eyebrow">Temps passé</span><strong>{{ duration(totalSeconds()) }}</strong></p>
        <p class="card kpi"><span class="eyebrow">Temps moyen par page</span><strong>{{ duration(averageSeconds()) }}</strong></p>
      </div>

      <section class="card stack" aria-labelledby="t-daily">
        <h2 id="t-daily" class="card-title">Consultations {{ days() > 90 ? 'par semaine' : 'par jour' }}</h2>
        @if (trend().length > 0) { <app-daily-chart [days]="trend()" /> } @else { <span class="skeleton" style="height: 20rem" aria-hidden="true"></span> }
      </section>

      <div class="grid-auto" style="--col-min: 30rem">
        <section class="card stack" aria-labelledby="t-most">
          <h2 id="t-most" class="card-title">Les pages les plus ouvertes</h2>
          <app-top-pages [items]="mostOpened()" measure="views" />
        </section>
        <section class="card stack" aria-labelledby="t-time">
          <h2 id="t-time" class="card-title">Là où l'on passe le plus de temps</h2>
          <app-top-pages [items]="mostTime()" measure="seconds" />
        </section>
      </div>

      <details class="card panel">
        <summary class="card-title">Détail page par page</summary>
        <div class="stack">
      <div class="filters">
        <app-search-box label="Rechercher une page…" (searched)="list.setSearch($event)" />
        <app-sort-select [state]="list" [fields]="fields" />
      </div>

      @if (!loaded()) {
        <div class="stack" aria-hidden="true"><span class="skeleton" style="height: 16rem"></span></div>
      } @else if (items().items.length === 0) {
        <p class="empty">
          <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M4 20V10M10 20V4M16 20v-7M22 20H2"></path></svg>
          <strong>{{ list.search() ? 'Aucun résultat' : 'Pas encore de données' }}</strong>
          <span>{{ list.search() ? 'Aucune page ne correspond à cette recherche.' : 'Les chiffres apparaissent dès que des adhérents consultent le site.' }}</span>
        </p>
      } @else {
        <table class="rtable">
          <thead>
            <tr><th>Page</th><th class="num">Consultations</th><th class="num">Temps total</th><th class="num">Temps moyen</th></tr>
          </thead>
          <tbody>
            @for (item of items().items; track item.path) {
              <tr>
                <td class="cell-main" data-label="Page">
                  <strong>{{ name(item.path) }}</strong>
                  @if (name(item.path) !== item.path) { <span class="small-text">{{ item.path }}</span> }
                </td>
                <td class="num" data-label="Consultations">{{ item.views }}</td>
                <td class="num" data-label="Temps total">{{ duration(item.totalSeconds) }}</td>
                <td class="num" data-label="Temps moyen">{{ duration(item.averageSeconds) }}</td>
              </tr>
            }
          </tbody>
        </table>
        <app-pager [page]="items().page" [totalPages]="items().totalPages" [totalCount]="items().totalCount" (pageChange)="list.goTo($event)" />
      }
        </div>
      </details>
      <p class="small-text">
        Une page est comptée à chaque ouverture par un adhérent connecté ; le temps ne compte que lorsque l'onglet est visible
        (30 minutes au plus par ouverture). Seuls des compteurs par jour et par page sont conservés : ni nom, ni adresse, ni cookie.
      </p>
    </main>
  `,
  styles: `
    .num { text-align: right; font-variant-numeric: tabular-nums; }
    .kpi { display: grid; gap: var(--space-5); margin: 0; }
    .kpi strong { font-size: var(--text-h3); line-height: 1.1; }
  `,
})
export class UsageStatisticsPage {
  protected readonly trail: Crumb[] = [{ label: 'Administration', path: '/admin' }, { label: 'Usage du site' }];
  protected readonly periods = PERIODS;
  protected readonly fields: SortField[] = [
    { field: 'views', label: 'Consultations' },
    { field: 'seconds', label: 'Temps total' },
    { field: 'average', label: 'Temps moyen' },
    { field: 'path', label: 'Page' },
  ];
  private readonly api = inject(UsageApi);
  protected readonly list = new ListState('views', 'desc');
  protected readonly days = signal<number>(30);
  protected readonly items = signal<Page<UsageItem>>({ items: [], page: 1, pageSize: 25, totalCount: 0, totalPages: 0 });
  protected readonly loaded = signal(false);
  protected readonly error = signal('');
  protected readonly name = pageName;
  protected readonly duration = duration;
  protected readonly trend = signal<UsageDay[]>([]);
  protected readonly mostOpened = signal<UsageItem[]>([]);
  protected readonly mostTime = signal<UsageItem[]>([]);
  protected readonly totalViews = computed(() => this.trend().reduce((sum, d) => sum + d.views, 0));
  protected readonly totalSeconds = computed(() => this.trend().reduce((sum, d) => sum + d.seconds, 0));
  protected readonly averageSeconds = computed(() => (this.totalViews() > 0 ? Math.round(this.totalSeconds() / this.totalViews()) : 0));

  constructor() {
    effect(() => {
      const query = this.list.query();
      const days = this.days();
      untracked(() => void this.load(query, days));
    });
    // The charts follow the period only: neither the search nor the page of the table changes them.
    effect(() => {
      const days = this.days();
      untracked(() => void this.loadCharts(days));
    });
  }

  protected setDays(days: number): void {
    this.days.set(days);
    this.list.goTo(1);
  }

  protected periodLabel(days: number): string {
    return days === 365 ? '1 an' : `${days} jours`;
  }

  private async load(query: ReturnType<ListState['query']>, days: number): Promise<void> {
    try {
      this.items.set(await this.api.statistics(query, days));
      this.error.set('');
    } catch {
      this.error.set('Impossible de charger les statistiques.');
    } finally {
      this.loaded.set(true);
    }
  }

  private async loadCharts(days: number): Promise<void> {
    const top = (sort: string) => this.api.statistics({ sort, direction: 'desc', page: 1, pageSize: 8 }, days);
    try {
      const [trend, opened, time] = await Promise.all([this.api.trend(days), top('views'), top('seconds')]);
      this.trend.set(trend);
      this.mostOpened.set(opened.items);
      this.mostTime.set(time.items);
      this.error.set('');
    } catch {
      this.error.set('Impossible de charger les statistiques.');
    }
  }
}
