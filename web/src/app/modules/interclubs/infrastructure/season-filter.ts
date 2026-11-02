import { Injectable, computed, inject, signal } from '@angular/core';
import { InterclubSeason } from '../core/interclubs';
import { InterclubsApi } from './interclubs.api';

/**
 * The season the interclub pages are looking at. It is shared by the calendar and the teams, loaded once,
 * and starts on the current season of the club; the choice follows the person from one page to the other.
 */
@Injectable({ providedIn: 'root' })
export class SeasonFilter {
  private readonly api = inject(InterclubsApi);
  readonly seasons = signal<InterclubSeason[]>([]);
  readonly selected = signal('');
  /** False until the seasons are known: the pages wait, so that they load their list only once. */
  readonly ready = signal(false);
  readonly current = computed(() => this.seasons().find((s) => s.id === this.selected()) ?? null);
  private loading: Promise<void> | null = null;

  load(): Promise<void> {
    return (this.loading ??= this.fetch());
  }

  select(id: string): void {
    this.selected.set(id);
  }

  private async fetch(): Promise<void> {
    try {
      const page = await this.api.seasons({ sort: 'start', direction: 'desc', pageSize: 100 });
      this.seasons.set(page.items);
      if (!this.selected()) {
        this.selected.set((page.items.find((s) => s.isCurrent) ?? page.items[0])?.id ?? '');
      }
    } catch {
      this.seasons.set([]);
    } finally {
      this.ready.set(true);
    }
  }
}
