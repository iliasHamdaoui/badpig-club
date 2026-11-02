import { Component, computed, inject, input } from '@angular/core';
import { AdminUser } from '../../../../shared/core/admin-user';
import { Groups } from '../../../../shared/core/groups';
import { AdminActivity } from '../../../../shared/infrastructure/admin-activity';
import { TitleDef } from '../../core/title';
import { BoardApi } from '../../infrastructure/board.api';

/** Title of a board member (list), with a confirmation if the title already belongs to someone else. */
@Component({
  selector: 'app-title-select',
  template: `
    @if (user().groups.includes(boardGroup) && user().isActive) {
      @if (activity.confirming() === key()) {
        <span>{{ pending()!.message }}</span>
        <button class="btn btn-sm" [class.is-busy]="activity.busy() === user().id" [disabled]="activity.busy() !== null" (click)="confirm()">Oui</button>
        <button class="btn btn-ghost btn-sm" [disabled]="activity.busy() !== null" (click)="cancel()">Annuler</button>
      } @else {
        <select class="title-select" [attr.aria-label]="'Titre de ' + user().name" [disabled]="activity.busy() !== null"
                [value]="user().titleId ?? ''" (change)="choose($any($event.target).value, $event.target)">
          <option value="">Sans titre</option>
          @for (t of titles(); track t.id) { <option [value]="t.id" [selected]="user().titleId === t.id">{{ t.label }}</option> }
        </select>
      }
    }
  `,
  styles: `select.title-select { width: auto; margin-top: 0.4rem; font: inherit; font-size: var(--text-small); padding: 0.24rem 0.48rem; }`,
})
export class TitleSelect {
  readonly user = input.required<AdminUser>();
  readonly titles = input.required<TitleDef[]>();
  protected readonly activity = inject(AdminActivity);
  private readonly api = inject(BoardApi);
  protected readonly boardGroup = Groups.board;
  protected readonly key = computed(() => `${this.user().id}:title`);
  private pendingChange: { titleId: string; message: string } | null = null;
  protected readonly pending = () => (this.activity.confirming() === this.key() ? this.pendingChange : null);

  protected choose(value: string, select: EventTarget | null): void {
    const u = this.user();
    const titleId = value || null;
    (select as HTMLSelectElement).value = u.titleId ?? ''; // the display only changes after the API's answer
    if (titleId === (u.titleId ?? null)) {
      return;
    }
    const taken = titleId ? this.titles().find((t) => t.id === titleId && t.holderId && t.holderId !== u.id) : undefined;
    if (taken) {
      this.pendingChange = { titleId: titleId!, message: `${taken.holderName} est actuellement ${taken.label}. Le remplacer par ${u.name} ?` };
      this.activity.confirming.set(this.key());
      return;
    }
    void this.apply(titleId);
  }

  protected confirm(): Promise<void> {
    return this.apply(this.pendingChange!.titleId);
  }

  protected cancel(): void {
    this.pendingChange = null;
    this.activity.confirming.set(null);
  }

  private async apply(titleId: string | null): Promise<void> {
    const u = this.user();
    const label = this.titles().find((t) => t.id === titleId)?.label;
    await this.activity.run(
      u.id,
      () => this.api.changeTitle(u.id, titleId),
      titleId ? `${u.name} est maintenant ${label}.` : `${u.name} n'a plus de titre.`,
    );
    this.pendingChange = null;
  }
}
