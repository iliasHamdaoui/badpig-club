import { Component, inject, input, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { productImageUrl } from '../../../../shared/core/product-image';
import { AdminActivity } from '../../../../shared/infrastructure/admin-activity';
import { ShopProduct, parseEuros } from '../../core/shop';
import { ShopApi } from '../../infrastructure/shop.api';
import { VariantRow } from '../variant-row/variant-row';
import { MenuDetails } from '../../../../shared/ui/menu-details';

type Mode = 'view' | 'edit' | 'delete' | 'addVariant';

/** An article with its variants: rename, hide, delete, add a variant. */
@Component({
  selector: 'app-product-card',
  imports: [FormsModule, VariantRow, MenuDetails],
  template: `
    <article class="card stack" [class.inactive]="!product().isActive">
      <header class="head">
        @if (product().imageVersion !== null) {
          <img class="picture" [src]="imageUrl()" [alt]="product().name" />
        } @else {
          <span class="placeholder-img picture" aria-hidden="true">Photo</span>
        }
        <div class="stack stack-s" style="flex: 1; min-width: 0">
          <h3 class="card-title">{{ product().name }}</h3>
          <p class="row" style="--row-gap: var(--space-5)">
            @if (product().hasLowStock) { <span class="highlight highlight-warning">Stock bas</span> }
            @if (!product().isActive) { <span class="highlight highlight-neutral">Masqué</span> }
            <span class="small-text">{{ product().variants.length }} variante{{ product().variants.length > 1 ? 's' : '' }}</span>
          </p>
          @if (product().description) { <p class="small-text">{{ product().description }}</p> }
        </div>
        @if (mode() === 'view') {
          <details class="more">
            <summary class="btn btn-ghost btn-sm">Gérer l'article</summary>
            <div class="more-panel">
              <button class="btn btn-ghost btn-sm" type="button" [disabled]="busy()" (click)="edit()">Modifier l'article</button>
              <button class="btn btn-ghost btn-sm" type="button" [disabled]="busy()" (click)="picker.click()">{{ product().imageVersion === null ? 'Ajouter une image' : "Changer l'image" }}</button>
              @if (product().imageVersion !== null) { <button class="btn btn-ghost btn-sm" type="button" [disabled]="busy()" (click)="removeImage()">Retirer l'image</button> }
              <button class="btn btn-danger btn-sm" type="button" [disabled]="busy()" (click)="mode.set('delete')">Supprimer l'article</button>
            </div>
          </details>
        }
        <input #picker type="file" class="file" accept="image/jpeg,image/png,image/webp" (change)="chooseImage($event)" />
      </header>

      @switch (mode()) {
        @case ('edit') {
          <form class="stack stack-s" (ngSubmit)="save()">
            <div class="cols cols-2">
              <div class="field"><label class="label" [for]="'pn-' + product().id">Nom</label><input class="input" [id]="'pn-' + product().id" name="name" [(ngModel)]="name" maxlength="100" required /></div>
              <div class="field"><label class="label" [for]="'pd-' + product().id">Description</label><input class="input" [id]="'pd-' + product().id" name="description" [(ngModel)]="description" maxlength="500" /></div>
            </div>
            <label class="choice"><input type="checkbox" name="active" [(ngModel)]="active" /> Proposé aux adhérents</label>
            <p class="form-actions">
              <button class="btn btn-sm" type="submit" [class.is-busy]="busy()" [disabled]="busy()">Enregistrer</button>
              <button class="btn btn-ghost btn-sm" type="button" (click)="mode.set('view')">Annuler</button>
            </p>
          </form>
        }
        @case ('delete') {
          <p class="alert alert-error">
            <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M8 3h8l5 5v8l-5 5H8l-5-5V8z"></path><path d="M12 8v5M12 16h.01"></path></svg>
            <span class="stack stack-s">
              <span>Supprimer l'article « {{ product().name }} » et ses variantes ?</span>
              <span class="form-actions">
                <button class="btn btn-danger btn-sm" [class.is-busy]="busy()" [disabled]="busy()" (click)="remove()">Oui, supprimer</button>
                <button class="btn btn-ghost btn-sm" (click)="mode.set('view')">Annuler</button>
              </span>
            </span>
          </p>
        }
      }

      <div>
        @for (v of product().variants; track v.id) {
          <app-variant-row [variant]="v" [productName]="product().name" />
        }
        @if (product().variants.length === 0) {
          <p class="small-text">Aucune variante : ajoutez-en une pour pouvoir vendre cet article.</p>
        }
      </div>

      @if (mode() === 'addVariant') {
        <form class="card card-flat stack stack-s" style="background: var(--color-surface-subtle)" (ngSubmit)="addVariant()">
          <h4 class="card-title">Nouvelle variante</h4>
          <div class="cols cols-2">
            <div class="field"><label class="label" [for]="'vl-' + product().id">Nom (taille, type…)</label><input class="input" [id]="'vl-' + product().id" name="label" [(ngModel)]="label" maxlength="60" required /></div>
            <div class="field"><label class="label" [for]="'vp-' + product().id">Prix (€)</label><input class="input" [id]="'vp-' + product().id" name="price" [(ngModel)]="price" inputmode="decimal" required /></div>
            <div class="field"><label class="label" [for]="'vq-' + product().id">Stock initial</label><input class="input" [id]="'vq-' + product().id" name="quantity" type="number" min="0" [(ngModel)]="quantity" /></div>
            <div class="field"><label class="label" [for]="'vt-' + product().id">Seuil d'alerte</label><input class="input" [id]="'vt-' + product().id" name="threshold" type="number" min="0" [(ngModel)]="threshold" /></div>
          </div>
          <p class="form-actions">
            <button class="btn btn-sm" type="submit" [class.is-busy]="busy()" [disabled]="busy()">Ajouter</button>
            <button class="btn btn-ghost btn-sm" type="button" (click)="mode.set('view')">Annuler</button>
          </p>
        </form>
      } @else if (mode() === 'view') {
        <p><button class="btn btn-outline btn-sm" [disabled]="busy()" (click)="openAddVariant()">
          <svg class="ic ic-s" viewBox="0 0 24 24" aria-hidden="true"><path d="M12 5v14M5 12h14"></path></svg>
          Ajouter une variante
        </button></p>
      }
    </article>
  `,
  styles: `
    :host { display: block; }
    .inactive { background: var(--color-surface-subtle); }
    .head { display: flex; gap: var(--space-15); align-items: flex-start; flex-wrap: wrap; }
    .picture { width: 7.2rem; height: 7.2rem; object-fit: cover; border-radius: var(--radius-m); flex: none; }
    .file { display: none; }
  `,
})
export class ProductCard {
  readonly product = input.required<ShopProduct>();

  private readonly api = inject(ShopApi);
  private readonly activity = inject(AdminActivity);
  protected readonly mode = signal<Mode>('view');
  protected name = '';
  protected description = '';
  protected active = true;
  protected label = '';
  protected price = '';
  protected quantity = 0;
  protected threshold = 5;

  protected imageUrl(): string {
    return productImageUrl(this.product().id, this.product().imageVersion ?? 0);
  }

  protected async chooseImage(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = ''; // so that choosing the same file again works
    if (!file) {
      return;
    }
    if (!['image/jpeg', 'image/png', 'image/webp'].includes(file.type)) {
      this.activity.error.set("L'image doit être au format JPEG, PNG ou WebP.");
      return;
    }
    if (file.size > 3 * 1024 * 1024) {
      this.activity.error.set("L'image est trop lourde (3 Mo au plus).");
      return;
    }
    const p = this.product();
    await this.activity.run(p.id, () => this.api.setImage(p.id, file), 'Image enregistrée.', "L'envoi de l'image a échoué.");
  }

  protected async removeImage(): Promise<void> {
    const p = this.product();
    await this.activity.run(p.id, () => this.api.removeImage(p.id), 'Image retirée.', "Le retrait de l'image a échoué.");
  }

  protected busy(): boolean {
    return this.activity.busy() !== null;
  }

  protected edit(): void {
    const p = this.product();
    this.name = p.name;
    this.description = p.description;
    this.active = p.isActive;
    this.mode.set('edit');
  }

  protected openAddVariant(): void {
    this.label = '';
    this.price = '';
    this.quantity = 0;
    this.threshold = 5;
    this.mode.set('addVariant');
  }

  protected async save(): Promise<void> {
    const p = this.product();
    if (await this.activity.run(p.id, () => this.api.updateProduct(p.id, this.name, this.description, this.active), 'Article modifié.', 'La modification a échoué.')) {
      this.mode.set('view');
    }
  }

  protected async remove(): Promise<void> {
    const p = this.product();
    if (await this.activity.run(p.id, () => this.api.deleteProduct(p.id), `Article « ${p.name} » supprimé.`, 'La suppression a échoué.')) {
      this.mode.set('view');
    }
  }

  protected async addVariant(): Promise<void> {
    const cents = parseEuros(this.price);
    if (cents === null) {
      this.activity.error.set('Le prix doit être un montant, par exemple 18,50.');
      return;
    }
    const p = this.product();
    if (await this.activity.run(p.id, () => this.api.createVariant(p.id, this.label, cents, this.quantity, this.threshold), `Variante « ${this.label.trim()} » ajoutée.`, "L'ajout a échoué.")) {
      this.mode.set('view');
    }
  }
}
