import { Injectable, signal } from '@angular/core';
import { apiErrorMessage } from '../core/format';

/**
 * State shared by the pieces of one administration page (provided by the page itself, not globally):
 * which action is running (the other buttons are disabled), which confirmation is open,
 * the success / error message, and a counter the page watches to reload its data.
 */
@Injectable()
export class AdminActivity {
  /** Id of the row (account, title…) being processed. */
  readonly busy = signal<string | null>(null);
  /** Open confirmation, as `<row id>:<kind>`; only one at a time. */
  readonly confirming = signal<string | null>(null);
  readonly message = signal('');
  readonly error = signal('');
  /** Incremented after every action (even a failed one: the data may have changed). */
  readonly changed = signal(0);

  /** True if a confirmation is open for this row (the normal buttons are then hidden). */
  isConfirming(rowId: string): boolean {
    return this.confirming()?.startsWith(`${rowId}:`) ?? false;
  }

  /** Runs an action on a row. Returns true if it succeeded. */
  async run(rowId: string, action: () => Promise<unknown>, success: string, failure = "L'action a échoué. Rechargez la page et réessayez."): Promise<boolean> {
    this.message.set('');
    this.error.set('');
    this.busy.set(rowId);
    try {
      await action();
      this.confirming.set(null);
      this.message.set(success);
      return true;
    } catch (e) {
      this.error.set(apiErrorMessage(e, failure));
      return false;
    } finally {
      this.busy.set(null);
      this.changed.update((n) => n + 1);
    }
  }
}
