import { Component, effect, inject, signal, untracked } from '@angular/core';
import { ListState, Page } from '../../../../shared/core/page';
import { Pager, SearchBox, SortHeader } from '../../../../shared/ui/list-controls';
import { ShopAuditEntry, shopAuditActionLabel } from '../../core/shop';
import { ShopApi } from '../../infrastructure/shop.api';
import { Crumb, PageHead } from '../../../../shared/ui/page-head';

/** What the board did in the shop: articles, variants, stock, handing over of orders. Most recent first. */
@Component({
  selector: 'app-shop-audit',
  imports: [SearchBox, SortHeader, Pager, PageHead],
  host: { class: 'bleed' },
  template: `
    <main class="container stack stack-l" style="padding-block: var(--space-30) var(--space-60)">
      <app-page-head [trail]="trail" title="Journal de la boutique" />

      <app-search-box label="Rechercher un membre du bureau, un article, une action…" (searched)="list.setSearch($event)" />

      @if (failed()) {
        <p class="error">Impossible de charger le journal.</p>
      } @else if (!loaded()) {
        <p>Chargement…</p>
      } @else if (entries().items.length === 0) {
        <p>{{ list.search() ? 'Aucun résultat.' : 'Aucune action pour le moment.' }}</p>
      } @else {
        <div class="scroll">
          <table>
            <thead>
              <tr>
                <th><app-sort-header [state]="list" field="date" label="Date" /></th>
                <th><app-sort-header [state]="list" field="actor" label="Par" /></th>
                <th><app-sort-header [state]="list" field="action" label="Action" /></th>
                <th><app-sort-header [state]="list" field="subject" label="Article" /></th>
                <th>Détails</th>
              </tr>
            </thead>
            <tbody>
              @for (e of entries().items; track e.id) {
                <tr>
                  <td>{{ when(e.occurredAt) }}</td>
                  <td>{{ e.actor }}</td>
                  <td>{{ label(e.action) }}</td>
                  <td>{{ e.subject ?? '' }}</td>
                  <td>{{ e.details ?? '' }}</td>
                </tr>
              }
            </tbody>
          </table>
        </div>
        <app-pager [page]="entries().page" [totalPages]="entries().totalPages" [totalCount]="entries().totalCount" (pageChange)="list.goTo($event)" />
      }
    </main>
  `,
  styles: `.scroll { overflow-x: auto; }`,
})
export class ShopAuditPage {
  protected readonly trail: Crumb[] = [{ label: 'Administration', path: '/admin' }, { label: 'Boutique', path: '/admin/shop' }, { label: 'Journal' }];
  private readonly api = inject(ShopApi);
  protected readonly list = new ListState('date', 'desc');
  protected readonly entries = signal<Page<ShopAuditEntry>>({ items: [], page: 1, pageSize: 25, totalCount: 0, totalPages: 0 });
  protected readonly loaded = signal(false);
  protected readonly failed = signal(false);
  protected readonly label = shopAuditActionLabel;

  constructor() {
    effect(() => {
      const query = this.list.query();
      untracked(() => void this.load(query));
    });
  }

  protected when(iso: string): string {
    return new Date(iso).toLocaleString('fr-FR', { dateStyle: 'short', timeStyle: 'short' });
  }

  private async load(query: Parameters<ShopApi['auditLog']>[0]): Promise<void> {
    try {
      this.entries.set(await this.api.auditLog(query));
      this.loaded.set(true);
      this.failed.set(false);
    } catch {
      this.failed.set(true);
    }
  }
}
