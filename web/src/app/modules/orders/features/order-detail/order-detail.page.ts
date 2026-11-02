import { Component, OnDestroy, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { apiErrorMessage, formatCents } from '../../../../shared/core/format';
import { MyOrder, ORDER_STATUS_LABELS } from '../../core/orders';
import { OrdersApi } from '../../infrastructure/orders.api';
import { Crumb, PageHead } from '../../../../shared/ui/page-head';

/**
 * One order. This is also the page where the payment provider sends the person back (paid or not): reading the order asks the provider,
 * so the state shown is right even if its notification is late.
 */
@Component({
  selector: 'app-order-detail',
  imports: [PageHead],
  host: { class: 'bleed' },
  template: `
    <main class="container stack stack-l" style="padding-block: var(--space-30) var(--space-60)">
      <app-page-head [trail]="trail" title="Commande" />

      @if (failed()) {
        <p class="error">Cette commande est introuvable.</p>
      } @else if (order(); as o) {
        @if (error()) { <p class="error">{{ error() }}</p> }

        @switch (o.status) {
          @case ('pending') {
            @if (canceledPayment) { <p class="info">Le paiement n'a pas été fait. Votre commande est toujours réservée : vous pouvez payer ci-dessous.</p> }
            <p class="info">En attente de paiement. Les articles sont réservés pour vous jusqu'à <strong>{{ time(o.expiresAt) }}</strong>, puis la commande expire.</p>
          }
          @case ('paid') { <p class="success">✅ Paiement reçu. Un e-mail de confirmation vous a été envoyé. Le bureau vous remettra votre commande en main propre.</p> }
          @case ('delivered') { <p class="success">Commande remise le {{ day(o.deliveredAt!) }}.</p> }
          @case ('cancelled') { <p class="small-text">Commande annulée : les articles ont été remis en vente.</p> }
          @case ('expired') { <p class="small-text">Commande expirée faute de paiement à temps : les articles ont été remis en vente. Vous pouvez recommander depuis la boutique.</p> }
        }

        <table>
          <thead><tr><th>Article</th><th>Prix</th><th>Quantité</th><th>Total</th></tr></thead>
          <tbody>
            @for (l of o.lines; track l.productName + l.variantLabel) {
              <tr>
                <td>{{ l.productName }} <span class="small-text">({{ l.variantLabel }})</span></td>
                <td>{{ cents(l.unitPriceCents) }}</td>
                <td>{{ l.quantity }}</td>
                <td>{{ cents(l.unitPriceCents * l.quantity) }}</td>
              </tr>
            }
          </tbody>
          <tfoot><tr><th colspan="3">Total</th><th>{{ cents(o.totalCents) }}</th></tr></tfoot>
        </table>
        <p class="small-text">Commande du {{ day(o.createdAt) }} · {{ statusLabel(o.status) }}@if (o.paidAt) { · payée le {{ day(o.paidAt) }} }</p>

        @if (o.status === 'pending') {
          <p class="actions">
            @if (o.checkoutUrl) { <a class="btn" [href]="o.checkoutUrl">Payer {{ cents(o.totalCents) }}</a> }
            @if (confirming()) {
              <span>Annuler cette commande ?</span>
              <button class="btn btn-danger btn-sm" [class.is-busy]="busy()" [disabled]="busy()" (click)="cancel(o)">Oui, annuler</button>
              <button class="btn btn-ghost btn-sm" [disabled]="busy()" (click)="confirming.set(false)">Non</button>
            } @else {
              <button class="btn btn-ghost" [disabled]="busy()" (click)="confirming.set(true)">Annuler la commande</button>
              <button class="btn btn-ghost" [disabled]="busy()" (click)="reload()">Actualiser</button>
            }
          </p>
        }
      } @else {
        <p>Chargement…</p>
      }
    </main>
  `,
  styles: `
    .info { padding: 0.96rem 1.28rem; background: var(--alert-warning-background); border-radius: var(--radius-m); }
    td:nth-child(n+2), th:nth-child(n+2) { text-align: right; }
  `,
})
export class OrderDetailPage implements OnDestroy {
  protected readonly trail: Crumb[] = [{ label: 'Boutique', path: '/shop' }, { label: 'Mes commandes', path: '/shop/orders' }, { label: 'Détail' }];
  private readonly api = inject(OrdersApi);
  private readonly route = inject(ActivatedRoute);
  private readonly id = this.route.snapshot.paramMap.get('id') ?? '';
  private timer: ReturnType<typeof setTimeout> | undefined;

  protected readonly order = signal<MyOrder | null>(null);
  protected readonly failed = signal(false);
  protected readonly error = signal('');
  protected readonly busy = signal(false);
  protected readonly confirming = signal(false);
  protected readonly cents = formatCents;
  /** True when the person comes back from the payment page without paying (the provider adds ?canceled=1). */
  protected readonly canceledPayment = this.route.snapshot.queryParamMap.get('canceled') === '1';

  constructor() {
    void this.reload();
  }

  ngOnDestroy(): void {
    clearTimeout(this.timer);
  }

  protected statusLabel(status: MyOrder['status']): string {
    return ORDER_STATUS_LABELS[status];
  }

  protected day(iso: string): string {
    return new Date(iso).toLocaleDateString('fr-FR');
  }

  protected time(iso: string): string {
    return new Date(iso).toLocaleTimeString('fr-FR', { hour: '2-digit', minute: '2-digit' });
  }

  protected async reload(): Promise<void> {
    clearTimeout(this.timer);
    try {
      const order = await this.api.order(this.id);
      this.order.set(order);
      this.failed.set(false);
      // Just back from the payment: the payment may take a few seconds to be confirmed, so look again a few times.
      if (order.status === 'pending' && !this.canceledPayment && this.retries < 6 && document.referrer !== '') {
        this.retries++;
        this.timer = setTimeout(() => void this.reload(), 5000);
      }
    } catch {
      this.failed.set(true);
    }
  }

  private retries = 0;

  protected async cancel(order: MyOrder): Promise<void> {
    this.error.set('');
    this.busy.set(true);
    try {
      await this.api.cancel(order.id);
      this.confirming.set(false);
    } catch (e) {
      this.error.set(apiErrorMessage(e, "L'annulation a échoué. Actualisez la page."));
    } finally {
      this.busy.set(false);
      await this.reload();
    }
  }
}
