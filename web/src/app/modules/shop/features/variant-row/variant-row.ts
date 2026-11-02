import { Component, inject, input, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { formatCents } from '../../../../shared/core/format';
import { AdminActivity } from '../../../../shared/infrastructure/admin-activity';
import { ShopVariant, centsToInput, parseEuros } from '../../core/shop';
import { ShopApi } from '../../infrastructure/shop.api';
import { MenuDetails } from '../../../../shared/ui/menu-details';

type Mode = 'view' | 'edit' | 'restock' | 'correct' | 'delete';

/** One variant (size, type…) of an article with its stock and the actions of the board. */
@Component({
  selector: 'app-variant-row',
  imports: [FormsModule, RouterLink, MenuDetails],
  template: `
    <div class="variant" [class.inactive]="!variant().isActive">
      <div class="line">
        <span class="who">
          <strong>{{ variant().label }}</strong>
          <span class="price">{{ cents(variant().priceCents) }}</span>
        </span>
        <span class="state">
          <span class="highlight" [class]="stockBadge()">Stock : {{ variant().quantity }}</span>
          @if (variant().reserved > 0) {
            <span class="small-text">{{ variant().reserved }} réservé{{ variant().reserved > 1 ? 's' : '' }} · {{ variant().available }} disponible{{ variant().available > 1 ? 's' : '' }}</span>
          }
          @if (!variant().isActive) { <span class="highlight highlight-neutral">Masquée</span> }
        </span>
        @if (mode() === 'view') {
          <span class="actions">
            <button class="btn btn-sm" type="button" [disabled]="busy()" (click)="open('restock')">Réapprovisionner</button>
            <details class="more">
              <summary class="btn btn-ghost btn-sm">Plus d'actions</summary>
              <div class="more-panel">
                <button class="btn btn-ghost btn-sm" type="button" [disabled]="busy()" (click)="open('correct')">Corriger le stock</button>
                <button class="btn btn-ghost btn-sm" type="button" [disabled]="busy()" (click)="open('edit')">Modifier la variante</button>
                <a class="btn btn-ghost btn-sm" [routerLink]="['/admin/shop/variants', variant().id, 'history']" [queryParams]="{ name: productName() + ' – ' + variant().label }">Historique du stock</a>
                <button class="btn btn-danger btn-sm" type="button" [disabled]="busy()" (click)="open('delete')">Supprimer</button>
              </div>
            </details>
          </span>
        }
      </div>

      @switch (mode()) {
        @case ('edit') {
          <form class="stack stack-s" (ngSubmit)="save()">
            <div class="cols cols-3">
              <div class="field"><label class="label" [for]="id('label')">Nom</label><input class="input" [id]="id('label')" name="label" [(ngModel)]="label" maxlength="60" required /></div>
              <div class="field"><label class="label" [for]="id('price')">Prix (€)</label><input class="input" [id]="id('price')" name="price" [(ngModel)]="price" inputmode="decimal" required /></div>
              <div class="field"><label class="label" [for]="id('threshold')">Seuil d'alerte</label><input class="input" [id]="id('threshold')" name="threshold" type="number" min="0" [(ngModel)]="threshold" /></div>
            </div>
            <label class="choice"><input type="checkbox" name="active" [(ngModel)]="active" /> Proposée aux adhérents</label>
            <p class="form-actions">
              <button class="btn btn-sm" type="submit" [class.is-busy]="busy()" [disabled]="busy()">Enregistrer</button>
              <button class="btn btn-ghost btn-sm" type="button" (click)="mode.set('view')">Annuler</button>
            </p>
          </form>
        }
        @case ('restock') {
          <form class="stack stack-s" (ngSubmit)="restock()">
            <div class="cols cols-2">
              <div class="field"><label class="label" [for]="id('qty')">Quantité reçue</label><input class="input" [id]="id('qty')" name="qty" type="number" min="1" [(ngModel)]="quantity" required /></div>
              <div class="field"><label class="label" [for]="id('note')">Note <span class="opt">(facultatif)</span></label><input class="input" [id]="id('note')" name="note" [(ngModel)]="note" maxlength="200" /></div>
            </div>
            <p class="form-actions">
              <button class="btn btn-sm" type="submit" [class.is-busy]="busy()" [disabled]="busy()">Ajouter au stock</button>
              <button class="btn btn-ghost btn-sm" type="button" (click)="mode.set('view')">Annuler</button>
            </p>
          </form>
        }
        @case ('correct') {
          <form class="stack stack-s" (ngSubmit)="correct()">
            <div class="cols cols-2">
              <div class="field"><label class="label" [for]="id('newqty')">Stock réel constaté</label><input class="input" [id]="id('newqty')" name="newqty" type="number" min="0" [(ngModel)]="quantity" required /></div>
              <div class="field"><label class="label" [for]="id('why')">Raison</label><input class="input" [id]="id('why')" name="why" [(ngModel)]="note" maxlength="200" required /></div>
            </div>
            <p class="form-actions">
              <button class="btn btn-sm" type="submit" [class.is-busy]="busy()" [disabled]="busy() || !note.trim()">Corriger</button>
              <button class="btn btn-ghost btn-sm" type="button" (click)="mode.set('view')">Annuler</button>
            </p>
          </form>
        }
        @case ('delete') {
          <p class="alert alert-error">
            <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M8 3h8l5 5v8l-5 5H8l-5-5V8z"></path><path d="M12 8v5M12 16h.01"></path></svg>
            <span class="stack stack-s">
              <span>Supprimer « {{ variant().label }} » ?</span>
              <span class="form-actions">
                <button class="btn btn-danger btn-sm" [class.is-busy]="busy()" [disabled]="busy()" (click)="remove()">Oui, supprimer</button>
                <button class="btn btn-ghost btn-sm" (click)="mode.set('view')">Annuler</button>
              </span>
            </span>
          </p>
        }
      }
    </div>
  `,
  styles: `
    .variant { border-top: 0.1rem solid var(--color-border); padding: var(--space-15) 0; display: grid; gap: var(--space-10); }
    .variant.inactive { opacity: .7; }
    .line { display: flex; flex-wrap: wrap; gap: var(--space-10) var(--space-20); align-items: center; }
    .who { display: flex; flex-wrap: wrap; gap: var(--space-10); align-items: baseline; min-width: 16rem; }
    .price { font-variant-numeric: tabular-nums; }
    .state { display: flex; flex-wrap: wrap; gap: var(--space-5) var(--space-10); align-items: center; flex: 1; }
    .actions { display: flex; flex-wrap: wrap; gap: var(--space-10); margin-left: auto; }
  `,
})
export class VariantRow {
  /** Green when the stock is comfortable, amber when the alert threshold is reached, framboise when there is nothing left. */
  protected stockBadge(): string {
    if (this.variant().quantity === 0) {
      return 'highlight-error';
    }
    return this.variant().isLowStock ? 'highlight-warning' : 'highlight-success';
  }

  readonly variant = input.required<ShopVariant>();
  readonly productName = input.required<string>();

  private readonly api = inject(ShopApi);
  private readonly activity = inject(AdminActivity);
  protected readonly mode = signal<Mode>('view');
  protected readonly cents = formatCents;
  protected label = '';
  protected price = '';
  protected threshold = 0;
  protected active = true;
  protected quantity = 1;
  protected note = '';

  protected busy(): boolean {
    return this.activity.busy() !== null;
  }

  protected id(field: string): string {
    return `${field}-${this.variant().id}`;
  }

  protected open(mode: Mode): void {
    const v = this.variant();
    this.label = v.label;
    this.price = centsToInput(v.priceCents);
    this.threshold = v.lowStockThreshold;
    this.active = v.isActive;
    this.quantity = mode === 'correct' ? v.quantity : 1;
    this.note = '';
    this.mode.set(mode);
  }

  protected async save(): Promise<void> {
    const cents = parseEuros(this.price);
    if (cents === null) {
      this.activity.error.set('Le prix doit être un montant, par exemple 18,50.');
      return;
    }
    const v = this.variant();
    if (await this.activity.run(v.id, () => this.api.updateVariant(v.id, this.label, cents, this.threshold, this.active), 'Variante modifiée.', 'La modification a échoué.')) {
      this.mode.set('view');
    }
  }

  protected async restock(): Promise<void> {
    const v = this.variant();
    if (await this.activity.run(v.id, () => this.api.restock(v.id, this.quantity, this.note.trim()), `${this.quantity} ajouté(s) au stock.`, 'Le réapprovisionnement a échoué.')) {
      this.mode.set('view');
    }
  }

  protected async correct(): Promise<void> {
    const v = this.variant();
    if (await this.activity.run(v.id, () => this.api.correctStock(v.id, this.quantity, this.note.trim()), 'Stock corrigé.', 'La correction a échoué.')) {
      this.mode.set('view');
    }
  }

  protected async remove(): Promise<void> {
    const v = this.variant();
    if (await this.activity.run(v.id, () => this.api.deleteVariant(v.id), `Variante « ${v.label} » supprimée.`, 'La suppression a échoué.')) {
      this.mode.set('view');
    }
  }
}
