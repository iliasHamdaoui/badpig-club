import { Component, effect, inject, signal, untracked } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ListState, Page } from '../../../../shared/core/page';
import { Pager, SortHeader } from '../../../../shared/ui/list-controls';
import { STOCK_REASON_LABELS, StockMovement } from '../../core/shop';
import { ShopApi } from '../../infrastructure/shop.api';
import { Crumb, PageHead } from '../../../../shared/ui/page-head';

/** Every movement of the stock of one variant: restocks, corrections, sales. */
@Component({
  selector: 'app-stock-history',
  imports: [RouterLink, SortHeader, Pager, PageHead],
  host: { class: 'bleed' },
  template: `
    <main class="container stack stack-l" style="padding-block: var(--space-30) var(--space-60)">
      <app-page-head [trail]="trail" title="Historique du stock" />
      @if (name) { <h2>{{ name }}</h2> }
      <p><a routerLink="/admin/shop">← Articles et stock</a></p>

      @if (failed()) {
        <p class="error">Impossible de charger l'historique.</p>
      } @else if (!loaded()) {
        <p>Chargement…</p>
      } @else if (movements().items.length === 0) {
        <p>Aucun mouvement.</p>
      } @else {
        <div class="scroll">
          <table>
            <thead>
              <tr>
                <th><app-sort-header [state]="list" field="createdAt" label="Date" /></th>
                <th>Mouvement</th>
                <th>Quantité</th>
                <th>Note</th>
                <th>Par</th>
              </tr>
            </thead>
            <tbody>
              @for (m of movements().items; track m.id) {
                <tr>
                  <td>{{ when(m.createdAt) }}</td>
                  <td>{{ reason(m) }}</td>
                  <td [class.minus]="m.change < 0">{{ m.change > 0 ? '+' : '' }}{{ m.change }}</td>
                  <td>{{ m.note ?? '' }}</td>
                  <td>{{ m.authorName ?? (m.reason === 'sale' ? 'Commande en ligne' : '') }}</td>
                </tr>
              }
            </tbody>
          </table>
        </div>
        <app-pager [page]="movements().page" [totalPages]="movements().totalPages" [totalCount]="movements().totalCount" (pageChange)="list.goTo($event)" />
      }
    </main>
  `,
  styles: `
    .scroll { overflow-x: auto; }
    .minus { color: var(--color-error-text); }
  `,
})
export class StockHistoryPage {
  protected readonly trail: Crumb[] = [{ label: 'Administration', path: '/admin' }, { label: 'Boutique', path: '/admin/shop' }, { label: 'Historique' }];
  private readonly api = inject(ShopApi);
  private readonly route = inject(ActivatedRoute);
  private readonly id = this.route.snapshot.paramMap.get('id') ?? '';
  protected readonly name = this.route.snapshot.queryParamMap.get('name') ?? '';
  protected readonly list = new ListState('createdAt', 'desc');
  protected readonly movements = signal<Page<StockMovement>>({ items: [], page: 1, pageSize: 25, totalCount: 0, totalPages: 0 });
  protected readonly loaded = signal(false);
  protected readonly failed = signal(false);

  constructor() {
    effect(() => {
      const query = this.list.query();
      untracked(() => void this.load(query));
    });
  }

  protected reason(m: StockMovement): string {
    return STOCK_REASON_LABELS[m.reason];
  }

  protected when(iso: string): string {
    return new Date(iso).toLocaleString('fr-FR', { dateStyle: 'short', timeStyle: 'short' });
  }

  private async load(query: Parameters<ShopApi['movements']>[1]): Promise<void> {
    try {
      this.movements.set(await this.api.movements(this.id, query));
      this.loaded.set(true);
      this.failed.set(false);
    } catch {
      this.failed.set(true);
    }
  }
}
