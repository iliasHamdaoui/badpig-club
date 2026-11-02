import { Component, inject } from '@angular/core';
import { SeasonFilter } from '../../infrastructure/season-filter';

/** The season the interclub pages are looking at: the same control on the calendar and on the teams. */
@Component({
  selector: 'app-season-filter',
  template: `
    @if (filter.seasons().length > 1) {
      <div class="stack stack-s">
        <span class="label" id="f-season">Saison</span>
        <div class="chip-row" role="group" aria-labelledby="f-season">
          @for (s of filter.seasons(); track s.id) {
            <button class="chip" type="button" [attr.aria-pressed]="filter.selected() === s.id" (click)="filter.select(s.id)">{{ s.name }}</button>
          }
        </div>
      </div>
    }
  `,
  styles: `:host { display: block; }`,
})
export class SeasonFilterControl {
  protected readonly filter = inject(SeasonFilter);
}
