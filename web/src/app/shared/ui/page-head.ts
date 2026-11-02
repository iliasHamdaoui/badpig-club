import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';

/** One step of the trail: a link, or the current page when it has no path. */
export interface Crumb {
  label: string;
  path?: string;
}

/**
 * The head of every page: the same trail, the same title and the same introduction everywhere.
 * A page that needs actions or a sub-navigation puts them after it, never inside.
 */
@Component({
  selector: 'app-page-head',
  imports: [RouterLink],
  template: `
    <header class="page-head" style="padding-block: 0">
      @if (trail().length > 0) {
        <p class="breadcrumb">
          @for (crumb of trail(); track crumb.label; let last = $last) {
            @if (crumb.path) { <a [routerLink]="crumb.path">{{ crumb.label }}</a> } @else { <span>{{ crumb.label }}</span> }
            @if (!last) { <span aria-hidden="true"> / </span> }
          }
        </p>
      }
      <h1>{{ title() }}</h1>
      @if (lead()) { <p class="lead">{{ lead() }}</p> }
    </header>
  `,
  styles: `
    :host { display: block; }
    /* The title of a page is one step below the one of the home page: it names a screen, not the site. */
    h1 { font-size: var(--text-h2); }
  `,
})
export class PageHead {
  readonly trail = input<Crumb[]>([]);
  readonly title = input.required<string>();
  readonly lead = input('');
}
