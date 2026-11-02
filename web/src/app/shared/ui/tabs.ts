import { Component, input } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

/** One page of a section. */
export interface Tab {
  path: string;
  label: string;
  /** True when only the exact address makes this tab the current one. */
  exact?: boolean;
}

/** Navigation inside a section (interclubs, boutique…): the pages as chips, the current one marked. */
@Component({
  selector: 'app-tabs',
  imports: [RouterLink, RouterLinkActive],
  template: `
    <nav class="chip-row" [attr.aria-label]="label()">
      @for (tab of tabs(); track tab.path) {
        <a class="chip" [routerLink]="tab.path" routerLinkActive ariaCurrentWhenActive="page" [routerLinkActiveOptions]="{ exact: tab.exact ?? false }">{{ tab.label }}</a>
      }
    </nav>
  `,
  styles: `:host { display: block; }`,
})
export class Tabs {
  readonly tabs = input.required<Tab[]>();
  readonly label = input('Pages de la section');
}
