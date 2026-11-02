import { Component, effect, inject, signal, untracked } from '@angular/core';
import { ListState, Page } from '../../../../shared/core/page';
import { formatCents, formatDate } from '../../../../shared/core/format';
import { AdminActivity } from '../../../../shared/infrastructure/admin-activity';
import { Pager, SearchBox, SortHeader } from '../../../../shared/ui/list-controls';
import { SHOP_ORDER_STATUS_LABELS, ShopOrderItem } from '../../core/shop';
import { ShopApi } from '../../infrastructure/shop.api';
import { Crumb, PageHead } from '../../../../shared/ui/page-head';

/** Orders received by the shop: what to hand over, and what was already handed over (board). */
@Component({
  selector: 'app-shop-orders',
  imports: [SearchBox, SortHeader, Pager, PageHead],
  providers: [AdminActivity],
  host: { class: 'bleed' },
  template: `
    <main class="container stack stack-l" style="padding-block: var(--space-30) var(--space-60)">
      <app-page-head [trail]="trail" title="Commandes reçues" />

      @if (activity.message()) { <p class="success">{{ activity.message() }}</p> }
      @if (activity.error()) { <p class="error">{{ activity.error() }}</p> }

      <div class="filters">
        <app-search-box label="Rechercher un acheteur…" (searched)="list.setSearch($event)" />
        <select class="select input" aria-label="Filtrer par état" (change)="setStatus($any($event.target).value)">
          <option value="" [selected]="status() === ''">Tous les états</option>
          @for (s of statuses; track s[0]) { <option [value]="s[0]" [selected]="status() === s[0]">{{ s[1] }}</option> }
        </select>
      </div>

      @if (!loaded()) {
        <p>Chargement…</p>
      } @else if (orders().items.length === 0) {
        <p>{{ status() || list.search() ? 'Aucune commande pour ce filtre.' : 'Aucune commande.' }}</p>
      } @else {
        <div class="scroll">
          <table>
            <thead>
              <tr>
                <th><app-sort-header [state]="list" field="createdAt" label="Date" /></th>
                <th><app-sort-header [state]="list" field="buyerName" label="Acheteur" /></th>
                <th>Articles</th>
                <th><app-sort-header [state]="list" field="totalCents" label="Total" /></th>
                <th>État</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              @for (o of orders().items; track o.id) {
                <tr>
                  <td>{{ date(o.createdAt) }}</td>
                  <td>{{ o.buyerName }}</td>
                  <td>
                    @for (l of o.lines; track l.productName + l.variantLabel) {
                      <div>{{ l.quantity }} × {{ l.productName }} <span class="small-text">({{ l.variantLabel }})</span></div>
                    }
                  </td>
                  <td>{{ cents(o.totalCents) }}</td>
                  <td><span class="badge" [class.todo]="o.status === 'paid'">{{ label(o.status) }}</span>@if (o.deliveredAt) { <div class="small-text">le {{ when(o.deliveredAt) }}@if (o.deliveredByName) { par {{ o.deliveredByName }} }</div> }</td>
                  <td>
                    @if (o.status === 'paid') {
                      @if (activity.confirming() === o.id + ':deliver') {
                        <div class="actions">
                          <button class="btn btn-sm" [class.is-busy]="activity.busy() === o.id" [disabled]="activity.busy() !== null" (click)="deliver(o)">Confirmer la remise</button>
                          <button class="btn btn-ghost btn-sm" (click)="activity.confirming.set(null)">Annuler</button>
                        </div>
                      } @else {
                        <button class="btn btn-sm" [disabled]="activity.busy() !== null" (click)="activity.confirming.set(o.id + ':deliver')">Marquer remise</button>
                      }
                    }
                  </td>
                </tr>
              }
            </tbody>
          </table>
        </div>
        <app-pager [page]="orders().page" [totalPages]="orders().totalPages" [totalCount]="orders().totalCount" (pageChange)="list.goTo($event)" />
      }
    </main>
  `,
  styles: `
    .scroll { overflow-x: auto; }
  `,
})
export class ShopOrdersPage {
  protected readonly trail: Crumb[] = [{ label: 'Administration', path: '/admin' }, { label: 'Boutique', path: '/admin/shop' }, { label: 'Commandes' }];
  private readonly api = inject(ShopApi);
  protected readonly activity = inject(AdminActivity);
  protected readonly list = new ListState('createdAt', 'desc');
  protected readonly status = signal('paid');
  protected readonly orders = signal<Page<ShopOrderItem>>({ items: [], page: 1, pageSize: 25, totalCount: 0, totalPages: 0 });
  protected readonly loaded = signal(false);
  protected readonly statuses = Object.entries(SHOP_ORDER_STATUS_LABELS);
  protected readonly cents = formatCents;
  protected readonly date = formatDate;

  protected when(iso: string): string {
    return new Date(iso).toLocaleString('fr-FR', { dateStyle: 'short', timeStyle: 'short' });
  }

  constructor() {
    effect(() => {
      this.activity.changed();
      const query = { ...this.list.query(), status: this.status() };
      untracked(() => void this.reload(query));
    });
  }

  protected label(status: ShopOrderItem['status']): string {
    return SHOP_ORDER_STATUS_LABELS[status];
  }

  protected setStatus(value: string): void {
    this.status.set(value);
    this.list.goTo(1);
  }

  protected async deliver(o: ShopOrderItem): Promise<void> {
    await this.activity.run(o.id, () => this.api.deliver(o.id), `Commande de ${o.buyerName} marquée comme remise.`, 'Le changement a échoué.');
  }

  private async reload(query: Parameters<ShopApi['orders']>[0]): Promise<void> {
    try {
      const orders = await this.api.orders(query);
      if (orders.items.length === 0 && orders.page > 1) {
        this.list.goTo(orders.totalPages);
        return;
      }
      this.orders.set(orders);
      this.loaded.set(true);
    } catch {
      this.activity.error.set('Impossible de charger les commandes.');
    }
  }
}
