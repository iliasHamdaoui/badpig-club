import { Component, input, output } from '@angular/core';
import { ListState, SortDirection } from '../core/page';

/** Search field: tells its parent what was typed, once the person stops typing. */
@Component({
  selector: 'app-search-box',
  template: `
    <div class="field">
      <label class="label" [for]="id">{{ title() }}</label>
      <div class="search">
        <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><circle cx="11" cy="11" r="7"></circle><path d="M20 20l-4-4"></path></svg>
        <input class="input" [id]="id" type="search" [placeholder]="label()" [attr.aria-label]="label()" [value]="value()" (input)="typed($any($event.target).value)" autocomplete="off" />
      </div>
    </div>
  `,
})
export class SearchBox {
  /** What the person is looking for, shown in the field itself (and read by a screen reader). */
  readonly label = input('Rechercher…');
  /** The visible title of the field, above it. */
  readonly title = input('Rechercher');
  readonly value = input('');
  readonly searched = output<string>();
  protected readonly id = `search-${Math.random().toString(36).slice(2, 8)}`;
  private timer: ReturnType<typeof setTimeout> | undefined;

  protected typed(text: string): void {
    clearTimeout(this.timer);
    this.timer = setTimeout(() => this.searched.emit(text.trim()), 300);
  }
}

/** One sortable field of a list. */
export interface SortField {
  field: string;
  label: string;
}

/**
 * Sort of a list, as a field to choose and a button that reverses the order.
 * Works at every width, unlike a sortable column title, which disappears when the table becomes cards.
 */
@Component({
  selector: 'app-sort-select',
  template: `
    <div class="row" style="align-items: flex-end; flex-wrap: nowrap">
      <div class="field" style="flex: 1; min-width: 0">
        <label class="label" [for]="id">Trier par</label>
        <select class="select input" [id]="id" (change)="sortBy($any($event.target).value)">
          @for (f of fields(); track f.field) {
            <option [value]="f.field" [selected]="state().sort() === f.field">{{ f.label }}</option>
          }
        </select>
      </div>
      <button class="btn btn-ghost" type="button" style="flex: none" [attr.aria-label]="ascending() ? 'Ordre croissant, appuyer pour inverser' : 'Ordre décroissant, appuyer pour inverser'" (click)="reverse()">
        @if (ascending()) {
          <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M12 19V5M6 11l6-6 6 6"></path></svg> A→Z
        } @else {
          <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M12 5v14M6 13l6 6 6-6"></path></svg> Z→A
        }
      </button>
    </div>
  `,
})
export class SortSelect {
  readonly state = input.required<ListState>();
  readonly fields = input.required<SortField[]>();
  protected readonly id = `sort-${Math.random().toString(36).slice(2, 8)}`;

  protected ascending(): boolean {
    return this.state().direction() === 'asc';
  }

  protected sortBy(field: string): void {
    const state = this.state();
    const direction: SortDirection = state.direction();
    state.sort.set(field);
    state.direction.set(direction);
    state.goTo(1);
  }

  protected reverse(): void {
    const state = this.state();
    state.direction.set(this.ascending() ? 'desc' : 'asc');
    state.goTo(1);
  }
}

/** Column title that sorts the list when clicked (a second click reverses the direction). Only on wide screens. */
@Component({
  selector: 'app-sort-header',
  template: `
    <button type="button" class="sort-btn" (click)="state().sortBy(field())" [attr.aria-label]="'Trier par ' + label()">
      {{ label() }}
      @if (state().sort() === field()) {
        @if (state().direction() === 'asc') {
          <svg class="ic ic-s" viewBox="0 0 24 24" aria-hidden="true"><path d="M12 19V5M6 11l6-6 6 6"></path></svg>
        } @else {
          <svg class="ic ic-s" viewBox="0 0 24 24" aria-hidden="true"><path d="M12 5v14M6 13l6 6 6-6"></path></svg>
        }
      }
    </button>
  `,
})
export class SortHeader {
  readonly state = input.required<ListState>();
  readonly field = input.required<string>();
  readonly label = input.required<string>();
}

/** Previous / next buttons with the position in the list. */
@Component({
  selector: 'app-pager',
  template: `
    <nav class="pagination" aria-label="Pagination">
      @if (totalPages() > 1) {
        <div class="pagination-nav">
          <button class="btn btn-outline" type="button" [disabled]="page() <= 1" (click)="pageChange.emit(page() - 1)">
            <svg class="ic ic-s" viewBox="0 0 24 24" aria-hidden="true"><path d="M15 6l-6 6 6 6"></path></svg> Précédent
          </button>
          <button class="btn btn-outline" type="button" [disabled]="page() >= totalPages()" (click)="pageChange.emit(page() + 1)">
            Suivant <svg class="ic ic-s" viewBox="0 0 24 24" aria-hidden="true"><path d="M9 6l6 6-6 6"></path></svg>
          </button>
        </div>
        <p class="pagination-info" aria-live="polite">Page {{ page() }} sur {{ totalPages() }} · {{ totalCount() }} résultats</p>
      } @else {
        <p class="pagination-info" aria-live="polite">{{ totalCount() }} résultat{{ totalCount() > 1 ? 's' : '' }}</p>
      }
    </nav>
  `,
})
export class Pager {
  readonly page = input.required<number>();
  readonly totalPages = input.required<number>();
  readonly totalCount = input.required<number>();
  readonly pageChange = output<number>();
}

/** How many rows a page holds. Sits in the filters row, next to the search and the sort. */
@Component({
  selector: 'app-page-size',
  template: `
    <div class="field">
      <label class="label" [for]="id">Par page</label>
      <select class="select input" [id]="id" (change)="state().setPageSize(+$any($event.target).value)">
        @for (size of sizes; track size) {
          <option [value]="size" [selected]="state().pageSize() === size">{{ size }}</option>
        }
      </select>
    </div>
  `,
})
export class PageSize {
  readonly state = input.required<ListState>();
  protected readonly sizes = [10, 25, 50, 100];
  protected readonly id = `page-size-${Math.random().toString(36).slice(2, 8)}`;
}
