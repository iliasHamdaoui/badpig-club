import { Component, computed, input, signal } from '@angular/core';
import { API } from '../infrastructure/api';

/** Address of the profile picture of a person; `version` changes when the picture is replaced, so the browser may keep it. */
export function photoUrl(accountId: string, version: number): string {
  return `${API}/members/${accountId}/photo?v=${version}`;
}

/**
 * Round profile picture of a person, or their initials when they have none (or when the picture cannot be loaded:
 * only the members with an up-to-date membership may look at pictures).
 */
@Component({
  selector: 'app-avatar',
  template: `
    @if (src(); as url) {
      <img [src]="url" alt="" loading="lazy" (error)="failed.set(true)" />
    } @else {
      <span aria-hidden="true">{{ initials() }}</span>
    }
  `,
  host: { '[class]': '"avatar avatar-" + size()' },
  styles: `
    :host { display: inline-grid; place-items: center; flex: none; border-radius: 50%; overflow: hidden; background: var(--badge-neutral-background); color: var(--badge-neutral-text); font-weight: var(--weight-bold); line-height: 1; }
    :host(.avatar-s) { width: 3.2rem; height: 3.2rem; font-size: 1.3rem; }
    :host(.avatar-m) { width: 4.8rem; height: 4.8rem; font-size: 1.8rem; }
    :host(.avatar-l) { width: 9.6rem; height: 9.6rem; font-size: 3.4rem; }
    :host(.avatar-xl) { width: min(100%, 16rem); aspect-ratio: 1; font-size: 5.6rem; }
    img { width: 100%; height: 100%; object-fit: cover; display: block; }
  `,
})
export class Avatar {
  readonly accountId = input.required<string>();
  readonly name = input('');
  /** Version of the picture; null or undefined when the person has none. */
  readonly version = input<number | null | undefined>(null);
  readonly size = input<'s' | 'm' | 'l' | 'xl'>('m');

  protected readonly failed = signal(false);
  protected readonly src = computed(() => {
    const version = this.version();
    return version && !this.failed() ? photoUrl(this.accountId(), version) : null;
  });
  protected readonly initials = computed(() =>
    this.name().split(/\s+/).filter(Boolean).slice(0, 2).map((part) => part[0]?.toUpperCase() ?? '').join(''),
  );
}
