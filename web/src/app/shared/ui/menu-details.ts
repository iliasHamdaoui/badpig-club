import { Directive, ElementRef, inject } from '@angular/core';

/**
 * Closes a menu written as a `<details>` (the "more actions" panels and the account menu) when the person
 * clicks outside of it or presses Escape — a menu that only closes by clicking its own button again is a trap.
 * Applies on its own to every `details.more` and `details.account` of a component that imports it.
 */
@Directive({
  selector: 'details.more, details.account',
  host: {
    '(document:click)': 'closeIfOutside($event)',
    '(document:keydown.escape)': 'closeAndGiveBackFocus()',
  },
})
export class MenuDetails {
  private readonly element = inject<ElementRef<HTMLDetailsElement>>(ElementRef);

  protected closeIfOutside(event: Event): void {
    const details = this.element.nativeElement;
    if (details.open && !details.contains(event.target as Node)) {
      details.open = false;
    }
  }

  protected closeAndGiveBackFocus(): void {
    const details = this.element.nativeElement;
    if (details.open) {
      details.open = false;
      details.querySelector('summary')?.focus();
    }
  }
}
