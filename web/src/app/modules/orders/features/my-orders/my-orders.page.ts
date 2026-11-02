import { Component, effect, inject, signal, untracked } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ListState, Page } from '../../../../shared/core/page';
import { formatCents, formatDate } from '../../../../shared/core/format';
import { Pager, SortHeader } from '../../../../shared/ui/list-controls';
import { MyOrderItem, ORDER_STATUS_LABELS } from '../../core/orders';
import { OrdersApi } from '../../infrastructure/orders.api';
import { Crumb, PageHead } from '../../../../shared/ui/page-head';

/** The orders of the signed-in member, whatever their state. */
@Component({
  selector: 'app-my-orders',
  imports: [RouterLink, SortHeader, Pager, PageHead],
  host: { class: 'bleed' },
  template: `
    <main class="container stack stack-l" style="padding-block: var(--space-30) var(--space-60)">
      <app-page-head [trail]="trail" title="Mes commandes" />

      <div class="filters">
        <select class="select input" aria-label="Filtrer par état" (change)="setStatus($any($event.target).value)">
          <option value="">Toutes les commandes</option>
          @for (s of statuses; track s[0]) { <option [value]="s[0]" [selected]="status() === s[0]">{{ s[1] }}</option> }
        </select>
      </div>

      @if (failed()) {
        <p class="error">Impossible de charger vos commandes.</p>
      } @else if (!loaded()) {
        <p>Chargement…</p>
      } @else if (orders().items.length === 0) {
        <p>Vous n'avez pas encore de commande.</p>
      } @else {
        <div class="scroll">
          <table>
            <thead>
              <tr>
                <th><app-sort-header [state]="list" field="createdAt" label="Date" /></th>
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
                  <td>{{ o.itemCount }}</td>
                  <td>{{ cents(o.totalCents) }}</td>
                  <td><span class="badge" [class.todo]="o.status === 'pending'">{{ label(o.status) }}</span></td>
                  <td><a class="btn btn-ghost btn-sm" [routerLink]="['/shop/orders', o.id]">{{ o.status === 'pending' ? 'Payer' : 'Voir' }}</a></td>
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
export class MyOrdersPage {
  protected readonly trail: Crumb[] = [{ label: 'Boutique', path: '/shop' }, { label: 'Mes commandes' }];
  private readonly api = inject(OrdersApi);
  protected readonly list = new ListState('createdAt', 'desc', 25);
  protected readonly status = signal('');
  protected readonly orders = signal<Page<MyOrderItem>>({ items: [], page: 1, pageSize: 25, totalCount: 0, totalPages: 0 });
  protected readonly loaded = signal(false);
  protected readonly failed = signal(false);
  protected readonly statuses = Object.entries(ORDER_STATUS_LABELS);
  protected readonly cents = formatCents;
  protected readonly date = formatDate;

  constructor() {
    effect(() => {
      const query = { ...this.list.query(), status: this.status() };
      untracked(() => void this.load(query));
    });
  }

  protected label(status: MyOrderItem['status']): string {
    return ORDER_STATUS_LABELS[status];
  }

  protected setStatus(value: string): void {
    this.status.set(value);
    this.list.goTo(1);
  }

  private async load(query: Parameters<OrdersApi['myOrders']>[0]): Promise<void> {
    try {
      this.orders.set(await this.api.myOrders(query));
      this.loaded.set(true);
      this.failed.set(false);
    } catch {
      this.failed.set(true);
    }
  }
}
