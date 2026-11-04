import { Component, computed, input } from '@angular/core';
import { UsageItem, pageName } from '../../core/usage-item';

/** The pages that come first for a measure, as horizontal bars (HTML, so they follow the width of the screen). */
@Component({
  selector: 'app-top-pages',
  template: `
    @if (rows().length === 0) {
      <p class="small-text">Pas encore de données.</p>
    } @else {
      <ol class="bars">
        @for (row of rows(); track row.path) {
          <li>
            <span class="name">{{ row.name }}</span>
            <span class="track" aria-hidden="true"><span class="fill" [style.width.%]="row.percent"></span></span>
            <span class="value">{{ row.text }}</span>
          </li>
        }
      </ol>
    }
  `,
  styles: `
    .bars { display: grid; gap: var(--space-10); list-style: none; padding: 0; margin: 0; }
    li { display: grid; grid-template-columns: 1fr auto; gap: var(--space-5) var(--space-10); align-items: center; }
    .name { grid-column: 1; overflow-wrap: anywhere; }
    .value { grid-column: 2; grid-row: 1; font-weight: var(--weight-bold); font-variant-numeric: tabular-nums; }
    .track { grid-column: 1 / -1; height: 1rem; border-radius: var(--radius-pill); background: var(--color-surface-subtle); overflow: hidden; }
    .fill { display: block; height: 100%; border-radius: var(--radius-pill); background: var(--color-brand); min-width: 0.4rem; }
  `,
})
export class TopPages {
  readonly items = input.required<UsageItem[]>();
  /** What is measured: the number of views or the total time. */
  readonly measure = input.required<'views' | 'seconds'>();

  protected readonly rows = computed(() => {
    const measure = this.measure();
    const value = (item: UsageItem): number => (measure === 'views' ? item.views : item.totalSeconds);
    const max = Math.max(1, ...this.items().map(value));
    return this.items().map((item) => ({
      path: item.path,
      name: pageName(item.path),
      percent: (value(item) / max) * 100,
      text: measure === 'views' ? `${item.views}` : duration(item.totalSeconds),
    }));
  });
}

/** « 45 s », « 3 min 05 s », « 1 h 20 min ». */
export function duration(seconds: number): string {
  if (seconds < 60) {
    return `${seconds} s`;
  }
  const minutes = Math.floor(seconds / 60);
  if (minutes < 60) {
    return `${minutes} min ${String(seconds % 60).padStart(2, '0')} s`;
  }
  return `${Math.floor(minutes / 60)} h ${String(minutes % 60).padStart(2, '0')} min`;
}
