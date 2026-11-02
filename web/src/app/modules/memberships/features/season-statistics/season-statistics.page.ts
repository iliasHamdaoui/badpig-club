import { Component, effect, inject, signal, untracked } from '@angular/core';
import { ListState, Page } from '../../../../shared/core/page';
import { formatDate, formatEuro } from '../../../../shared/core/format';
import { Pager, SearchBox, SortHeader } from '../../../../shared/ui/list-controls';
import { SeasonStatistics } from '../../core/season';
import { MembershipsApi } from '../../infrastructure/memberships.api';
import { Crumb, PageHead } from '../../../../shared/ui/page-head';

/** Memberships and fees collected per season, for club licence holders and external members. */
@Component({
  selector: 'app-season-statistics',
  imports: [SearchBox, SortHeader, Pager, PageHead],
  host: { class: 'bleed' },
  template: `
    <main class="container stack stack-l" style="padding-block: var(--space-30) var(--space-60)">
      <app-page-head [trail]="trail" title="Statistiques par saison" />
      @if (error()) { <p class="error">{{ error() }}</p> }
      <app-search-box label="Rechercher une saison…" (searched)="list.setSearch($event)" />
      @if (!loaded()) {
        <p>Chargement…</p>
      } @else if (statistics().items.length === 0) {
        <p>{{ list.search() ? 'Aucun résultat.' : 'Aucune saison pour le moment.' }}</p>
      } @else {
        <table>
          <thead>
            <tr>
              <th><app-sort-header [state]="list" field="name" label="Saison" /></th>
              <th><app-sort-header [state]="list" field="startsOn" label="Dates" /></th>
              <th>Licenciés du club</th>
              <th>Licenciés ailleurs</th>
              <th><app-sort-header [state]="list" field="totalCount" label="Adhérents" /></th>
              <th>Recettes club</th>
              <th>Recettes externes</th>
              <th><app-sort-header [state]="list" field="totalRevenue" label="Recettes" /></th>
            </tr>
          </thead>
          <tbody>
            @for (s of statistics().items; track s.seasonId) {
              <tr>
                <td>{{ s.name }}</td>
                <td>{{ date(s.startsOn) }} → {{ date(s.endsOn) }}</td>
                <td>{{ s.internalCount }}</td>
                <td>{{ s.externalCount }}</td>
                <td><strong>{{ s.totalCount }}</strong></td>
                <td>{{ euro(s.internalRevenue) }}</td>
                <td>{{ euro(s.externalRevenue) }}</td>
                <td><strong>{{ euro(s.totalRevenue) }}</strong></td>
              </tr>
            }
          </tbody>
        </table>
        <app-pager [page]="statistics().page" [totalPages]="statistics().totalPages" [totalCount]="statistics().totalCount" (pageChange)="list.goTo($event)" />
        <p class="small-text">Les montants sont ceux enregistrés au moment de la validation de chaque adhésion. Les adhésions d'un compte supprimé ne sont plus comptées (droit à l'effacement).</p>
      }
    </main>
  `,
})
export class SeasonStatisticsPage {
  protected readonly trail: Crumb[] = [{ label: 'Administration', path: '/admin' }, { label: 'Saisons', path: '/admin/seasons' }, { label: 'Statistiques' }];
  private readonly api = inject(MembershipsApi);
  protected readonly list = new ListState('startsOn', 'desc');
  protected readonly statistics = signal<Page<SeasonStatistics>>({ items: [], page: 1, pageSize: 25, totalCount: 0, totalPages: 0 });
  protected readonly loaded = signal(false);
  protected readonly error = signal('');
  protected readonly date = formatDate;
  protected readonly euro = formatEuro;

  constructor() {
    effect(() => {
      const query = this.list.query();
      untracked(() => void this.reload(query));
    });
  }

  private async reload(query: ReturnType<ListState['query']>): Promise<void> {
    try {
      this.statistics.set(await this.api.statistics(query));
      this.error.set('');
      this.loaded.set(true);
    } catch {
      this.error.set('Impossible de charger les statistiques.');
    }
  }
}
