import { Component, computed, inject, input } from '@angular/core';
import { AdminUser } from '../../../../shared/core/admin-user';
import { AdminActivity } from '../../../../shared/infrastructure/admin-activity';
import { BoardApi } from '../../infrastructure/board.api';

/** Add a person to the board / remove them from it (with confirmation). */
@Component({
  selector: 'app-board-actions',
  template: `
    @if (user().boardAction) {
      @if (activity.confirming() === key()) {
        <span>{{ user().boardAction === 'add' ? 'Ajouter au bureau ?' : 'Retirer du bureau ?' }}</span>
        <button class="btn btn-sm" [class.is-busy]="busy()" [disabled]="activity.busy() !== null" (click)="change()">Oui</button>
        <button class="btn btn-ghost btn-sm" [disabled]="activity.busy() !== null" (click)="activity.confirming.set(null)">Annuler</button>
      } @else if (!activity.isConfirming(user().id)) {
        <button class="btn btn-ghost btn-sm" [disabled]="activity.busy() !== null" (click)="activity.confirming.set(key())">
          {{ user().boardAction === 'add' ? 'Ajouter au bureau' : 'Retirer du bureau' }}
        </button>
      }
    }
  `,
  host: { style: 'display: contents' },
})
export class BoardActions {
  readonly user = input.required<AdminUser>();
  protected readonly activity = inject(AdminActivity);
  private readonly api = inject(BoardApi);
  protected readonly key = computed(() => `${this.user().id}:board`);
  protected readonly busy = computed(() => this.activity.busy() === this.user().id);

  protected change(): Promise<boolean> {
    const u = this.user();
    const add = u.boardAction === 'add';
    return this.activity.run(
      u.id,
      () => this.api.changeBoard(u.id, add),
      add ? `${u.name} fait maintenant partie du bureau.` : `${u.name} ne fait plus partie du bureau.`,
    );
  }
}
