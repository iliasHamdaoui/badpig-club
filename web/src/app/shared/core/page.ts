import { Signal, WritableSignal, computed, signal } from '@angular/core';

/** One page of a list (mirror of the API's `PagedResponse`). */
export interface Page<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export type SortDirection = 'asc' | 'desc';

/** Query string of every list endpoint of the API: search, sort, pagination. */
export interface ListQuery {
  search?: string;
  sort?: string;
  direction?: SortDirection;
  page?: number;
  pageSize?: number;
}

/** Query parameters of a list request (empty values are left out). */
export function listParams(query: ListQuery): Record<string, string> {
  const params: Record<string, string> = {};
  for (const [key, value] of Object.entries(query)) {
    if (value !== undefined && value !== null && value !== '') {
      params[key] = String(value);
    }
  }
  return params;
}

/** State of a list shown in a page: what the person typed, the sort and the current page. */
export class ListState {
  readonly search = signal('');
  readonly sort: WritableSignal<string>;
  readonly direction: WritableSignal<SortDirection>;
  readonly page = signal(1);
  /** The query to send to the API; changes when the search, the sort or the page change. */
  readonly query: Signal<ListQuery>;

  constructor(defaultSort: string, defaultDirection: SortDirection = 'asc', pageSize = 25) {
    this.sort = signal(defaultSort);
    this.direction = signal(defaultDirection);
    this.query = computed(() => ({
      search: this.search(),
      sort: this.sort(),
      direction: this.direction(),
      page: this.page(),
      pageSize,
    }));
  }

  setSearch(text: string): void {
    if (text !== this.search()) {
      this.search.set(text);
      this.page.set(1);
    }
  }

  /** Sorts on a field; a second click on the same field reverses the direction. */
  sortBy(field: string): void {
    if (this.sort() === field) {
      this.direction.set(this.direction() === 'asc' ? 'desc' : 'asc');
    } else {
      this.sort.set(field);
      this.direction.set('asc');
    }
    this.page.set(1);
  }

  goTo(page: number): void {
    this.page.set(Math.max(1, page));
  }
}
