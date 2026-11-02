import { Component, effect, inject, signal, untracked } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ListState, Page } from '../../../../shared/core/page';
import { apiErrorMessage, formatCents } from '../../../../shared/core/format';
import { productImageUrl } from '../../../../shared/core/product-image';
import { Pager, SearchBox } from '../../../../shared/ui/list-controls';
import { BasketLine, CatalogProduct, CatalogVariant, MAX_QUANTITY_PER_LINE, basketTotal } from '../../core/orders';
import { OrdersApi } from '../../infrastructure/orders.api';
import { Crumb, PageHead } from '../../../../shared/ui/page-head';

/** The shop for the members: the offered articles, a basket, then the payment page of the payment provider. */
@Component({
  selector: 'app-catalogue',
  imports: [RouterLink, SearchBox, Pager, PageHead],
  host: { class: 'bleed' },
  template: `
    <main class="container stack stack-l" style="padding-block: var(--space-30) var(--space-60)">
      <app-page-head
        [trail]="trail"
        title="Boutique du club"
        lead="Volants et maillots du club. Une fois la commande validée, vous avez 35 minutes pour la payer en ligne ; le bureau vous remet ensuite votre commande en main propre."
      />

      @if (error()) {
        <p class="alert alert-error">
          <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M12 8v5M12 16h.01"></path><circle cx="12" cy="12" r="9"></circle></svg>
          <span>{{ error() }} @if (hasUnpaidOrder()) { <a routerLink="/shop/orders">Voir mes commandes</a> }</span>
        </p>
      }

      <app-search-box label="Rechercher un article…" (searched)="list.setSearch($event)" />

      @if (failed()) {
        <p class="alert alert-error">
          <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M12 8v5M12 16h.01"></path><circle cx="12" cy="12" r="9"></circle></svg>
          <span>Impossible de charger la boutique.</span>
        </p>
      } @else if (!loaded()) {
        <div class="stack stack-s" aria-hidden="true"><span class="skeleton" style="height: 12rem"></span><span class="skeleton" style="height: 12rem"></span></div>
      } @else if (products().items.length === 0) {
        <p class="empty">
          <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M6 7h12l1 13H5zM9 7V5a3 3 0 0 1 6 0v2"></path></svg>
          <strong>{{ list.search() ? 'Aucun résultat' : 'Boutique vide' }}</strong>
          <span>{{ list.search() ? 'Essayez un autre mot.' : 'Aucun article en vente pour le moment.' }}</span>
        </p>
      } @else {
        <div class="grid-auto" style="--col-min: 32rem">
          @for (p of products().items; track p.id) {
            <article class="card stack">
              <header class="head">
                @if (p.imageVersion !== null) {
                  <img class="picture" [src]="image(p.id, p.imageVersion)" [alt]="p.name" loading="lazy" />
                } @else {
                  <span class="placeholder-img picture" aria-hidden="true">Photo</span>
                }
                <div class="stack stack-s" style="flex: 1; min-width: 0">
                  <h2 class="card-title">{{ p.name }}</h2>
                  @if (p.description) { <p>{{ p.description }}</p> }
                </div>
              </header>
              <ul class="variants">
                @for (v of p.variants; track v.id) {
                  <li class="variant" [class.out]="!v.inStock">
                    <span class="stack stack-s">
                      <span class="name">{{ v.label }}</span>
                      @if (v.inStock) {
                        @if (v.available <= 5) {
                          <span class="highlight highlight-warning">Plus que {{ v.available }}</span>
                        } @else {
                          <span class="highlight highlight-success">En stock</span>
                        }
                      } @else {
                        <span class="highlight highlight-neutral">Épuisé</span>
                      }
                    </span>
                    <span class="price">{{ cents(v.priceCents) }}</span>
                    @if (v.inStock) {
                      <span class="qty">
                        <button type="button" aria-label="Retirer un" [disabled]="quantityOf(v) === 0 || busy()" (click)="change(p, v, -1)">−</button>
                        <output aria-live="polite">{{ quantityOf(v) }}</output>
                        <button type="button" aria-label="Ajouter un" [disabled]="quantityOf(v) >= limit(v) || busy()" (click)="change(p, v, 1)">+</button>
                      </span>
                    }
                  </li>
                }
              </ul>
            </article>
          }
        </div>
        <app-pager [page]="products().page" [totalPages]="products().totalPages" [totalCount]="products().totalCount" (pageChange)="list.goTo($event)" />
      }

      @if (basket().length > 0) {
        <aside class="card stack" aria-labelledby="t-basket">
          <h2 id="t-basket" class="card-title">Mon panier</h2>
          <dl>
            @for (l of basket(); track l.variantId) {
              <div class="kv">
                <dt>{{ l.quantity }} × {{ l.productName }} <span class="small-text">({{ l.variantLabel }})</span></dt>
                <dd>{{ cents(l.unitPriceCents * l.quantity) }}</dd>
              </div>
            }
            <div class="kv total"><dt>Total</dt><dd>{{ cents(total()) }}</dd></div>
          </dl>
          <p class="form-actions">
            <button class="btn" [class.is-busy]="busy()" [disabled]="busy()" (click)="order()">{{ busy() ? 'Redirection vers le paiement…' : 'Commander et payer' }}</button>
            <button class="btn btn-ghost" [disabled]="busy()" (click)="basket.set([])">Vider le panier</button>
          </p>
        </aside>
      }
    </main>
  `,
  styles: `
    .head { display: flex; gap: var(--space-15); align-items: flex-start; }
    .picture { width: 7.2rem; height: 7.2rem; object-fit: cover; border-radius: var(--radius-m); flex: none; }
    .variants { list-style: none; display: grid; }
    .variant { display: flex; flex-wrap: wrap; align-items: center; gap: var(--space-10); padding-block: var(--space-10); border-top: 0.1rem solid var(--color-border); }
    .variant .name { font-weight: var(--weight-bold); }
    .variant .stack { flex: 1; min-width: 0; justify-items: start; }
    .variant .price { margin-left: auto; }
    .out { color: var(--color-text-muted); }
    .total dt, .total dd { font-size: var(--text-h5); }
  `,
})
export class CataloguePage {
  protected readonly trail: Crumb[] = [];
  private readonly api = inject(OrdersApi);
  protected readonly image = productImageUrl;
  protected readonly list = new ListState('name', 'asc', 25);
  protected readonly products = signal<Page<CatalogProduct>>({ items: [], page: 1, pageSize: 25, totalCount: 0, totalPages: 0 });
  protected readonly loaded = signal(false);
  protected readonly failed = signal(false);
  protected readonly basket = signal<BasketLine[]>([]);
  protected readonly busy = signal(false);
  protected readonly error = signal('');
  protected readonly hasUnpaidOrder = signal(false);
  protected readonly cents = formatCents;

  constructor() {
    effect(() => {
      const query = this.list.query();
      untracked(() => void this.load(query));
    });
  }

  protected total(): number {
    return basketTotal(this.basket());
  }

  protected quantityOf(variant: CatalogVariant): number {
    return this.basket().find((l) => l.variantId === variant.id)?.quantity ?? 0;
  }

  /** Most units a person can take of this variant: the stock, and the limit of one line. */
  protected limit(variant: CatalogVariant): number {
    return Math.min(variant.available, MAX_QUANTITY_PER_LINE);
  }

  protected change(product: CatalogProduct, variant: CatalogVariant, delta: number): void {
    const quantity = Math.max(0, Math.min(this.limit(variant), this.quantityOf(variant) + delta));
    const others = this.basket().filter((l) => l.variantId !== variant.id);
    this.basket.set(quantity === 0 ? others : [...others, {
      variantId: variant.id, productName: product.name, variantLabel: variant.label, unitPriceCents: variant.priceCents, quantity,
    }]);
  }

  /** The order holds the stock for 35 minutes, then the person is sent to the payment page. */
  protected async order(): Promise<void> {
    this.error.set('');
    this.hasUnpaidOrder.set(false);
    this.busy.set(true);
    try {
      const { id } = await this.api.createOrder(this.basket().map((l) => ({ variantId: l.variantId, quantity: l.quantity })));
      const order = await this.api.order(id);
      if (order.checkoutUrl) {
        window.location.href = order.checkoutUrl;
        return; // the page is left: the button stays busy until then
      }
      window.location.href = `/shop/orders/${id}`;
    } catch (e) {
      const message = apiErrorMessage(e, 'La commande a échoué. Réessayez.');
      this.error.set(message);
      this.hasUnpaidOrder.set(message.includes('en attente de paiement'));
      this.busy.set(false);
      void this.load(this.list.query()); // the stock may have changed
    }
  }

  private async load(query: Parameters<OrdersApi['catalog']>[0]): Promise<void> {
    try {
      this.products.set(await this.api.catalog(query));
      this.loaded.set(true);
      this.failed.set(false);
      this.dropUnavailable();
    } catch {
      this.failed.set(true);
    }
  }

  /** After a refresh, the basket never asks for more than what is left. */
  private dropUnavailable(): void {
    const variants = new Map(this.products().items.flatMap((p) => p.variants.map((v) => [v.id, v] as const)));
    this.basket.update((lines) => lines
      .map((l) => ({ ...l, quantity: Math.min(l.quantity, variants.has(l.variantId) ? this.limit(variants.get(l.variantId)!) : l.quantity) }))
      .filter((l) => l.quantity > 0));
  }
}
