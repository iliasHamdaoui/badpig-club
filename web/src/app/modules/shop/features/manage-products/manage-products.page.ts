import { Component, effect, inject, signal, untracked } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ListState, Page } from '../../../../shared/core/page';
import { AdminActivity } from '../../../../shared/infrastructure/admin-activity';
import { Pager, SearchBox } from '../../../../shared/ui/list-controls';
import { Tabs } from '../../../../shared/ui/tabs';
import { SHOP_ADMIN_TABS } from '../../core/tabs';
import { ShopProduct } from '../../core/shop';
import { ShopApi } from '../../infrastructure/shop.api';
import { ProductCard } from '../product-card/product-card';
import { Crumb, PageHead } from '../../../../shared/ui/page-head';

/** Articles, variants and stock of the shop (board). */
@Component({
  selector: 'app-manage-products',
  imports: [FormsModule, SearchBox, Pager, ProductCard, Tabs, PageHead],
  host: { class: 'bleed' },
  providers: [AdminActivity],
  template: `
    <main class="container stack stack-l" style="padding-block: var(--space-30) var(--space-60)">
      <app-page-head [trail]="trail" title="Articles et stock" lead="Les articles proposés aux adhérents et leur stock." />
      <app-tabs [tabs]="tabs" label="Pages de la boutique" />

      @if (activity.message()) {
        <p class="alert alert-success"><svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M5 12l5 5 9-10"></path></svg><span>{{ activity.message() }}</span></p>
      }
      @if (activity.error()) {
        <p class="alert alert-error"><svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M8 3h8l5 5v8l-5 5H8l-5-5V8z"></path><path d="M12 8v5M12 16h.01"></path></svg><span><span class="alert-title">Erreur.</span> {{ activity.error() }}</span></p>
      }

      <details class="card panel">
        <summary class="card-title">
          <svg class="ic ic-s chevron" viewBox="0 0 24 24" aria-hidden="true"><path d="M9 6l6 6-6 6"></path></svg>
          Créer un article
        </summary>
        <form class="stack stack-s" (ngSubmit)="create()">
          <div class="cols cols-2">
            <div class="field"><label class="label" for="new-name">Nom</label><input class="input" id="new-name" name="newName" [(ngModel)]="newName" maxlength="100" required autocomplete="off" /></div>
            <div class="field"><label class="label" for="new-desc">Description <span class="opt">(facultatif)</span></label><input class="input" id="new-desc" name="newDescription" [(ngModel)]="newDescription" maxlength="500" autocomplete="off" /></div>
          </div>
          <p class="form-actions">
            <button class="btn" type="submit" [class.is-busy]="activity.busy() === 'create'" [disabled]="activity.busy() !== null || !newName.trim()">Créer l'article</button>
          </p>
        </form>
      </details>

      <div class="stack">
        <app-search-box label="Nom d'un article" (searched)="list.setSearch($event)" />
        <div class="chip-row" role="group" aria-label="Filtre">
          <button class="chip" type="button" [attr.aria-pressed]="!lowStock()" (click)="setLowStock(false)">Tous les articles</button>
          <button class="chip" type="button" [attr.aria-pressed]="lowStock()" (click)="setLowStock(true)">Stock bas uniquement</button>
        </div>
      </div>

      @if (!loaded()) {
        <div class="stack" aria-hidden="true"><span class="skeleton" style="height: 14rem"></span><span class="skeleton" style="height: 14rem"></span></div>
      } @else if (products().items.length === 0) {
        <p class="empty">
          <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M4 7h16l-1.5 12h-13zM9 7a3 3 0 0 1 6 0"></path></svg>
          <strong>Aucun article</strong>
          <span>{{ list.search() || lowStock() ? 'Essayez un autre filtre.' : 'Créez le premier article ci-dessus.' }}</span>
        </p>
      } @else {
        <div class="stack">
          @for (p of products().items; track p.id) { <app-product-card [product]="p" /> }
        </div>
        <app-pager [page]="products().page" [totalPages]="products().totalPages" [totalCount]="products().totalCount" (pageChange)="list.goTo($event)" />
      }
    </main>
  `,
  styles: `summary { cursor: pointer; }`,
})
export class ManageProductsPage {
  protected readonly trail: Crumb[] = [{ label: 'Administration', path: '/admin' }, { label: 'Boutique' }];
  protected readonly tabs = SHOP_ADMIN_TABS;
  private readonly api = inject(ShopApi);
  protected readonly activity = inject(AdminActivity);
  protected readonly list = new ListState('name');
  protected readonly lowStock = signal(false);
  protected readonly products = signal<Page<ShopProduct>>({ items: [], page: 1, pageSize: 25, totalCount: 0, totalPages: 0 });
  protected readonly loaded = signal(false);
  protected newName = '';
  protected newDescription = '';

  constructor() {
    effect(() => {
      this.activity.changed();
      const query = { ...this.list.query(), lowStock: this.lowStock() };
      untracked(() => void this.reload(query));
    });
  }

  protected setLowStock(value: boolean): void {
    this.lowStock.set(value);
    this.list.goTo(1);
  }

  protected async create(): Promise<void> {
    const name = this.newName.trim();
    const ok = await this.activity.run('create', () => this.api.createProduct(name, this.newDescription.trim()), `Article « ${name} » créé. Ajoutez-lui des variantes.`, 'La création a échoué.');
    if (ok) {
      this.newName = '';
      this.newDescription = '';
    }
  }

  private async reload(query: Parameters<ShopApi['products']>[0]): Promise<void> {
    try {
      const products = await this.api.products(query);
      if (products.items.length === 0 && products.page > 1) {
        this.list.goTo(products.totalPages);
        return;
      }
      this.products.set(products);
      this.loaded.set(true);
    } catch {
      this.activity.error.set('Impossible de charger les articles.');
    }
  }
}
