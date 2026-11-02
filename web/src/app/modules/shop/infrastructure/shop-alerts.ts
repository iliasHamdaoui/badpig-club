import { Injectable, inject } from '@angular/core';
import { ShopApi } from './shop.api';

/**
 * What the shop needs the board to do. It is the only part of the module the host may call,
 * so that the administration home can show the same kind of banner as for the memberships.
 */
@Injectable({ providedIn: 'root' })
export class ShopAlerts {
  private readonly api = inject(ShopApi);

  /** Orders paid and waiting to be handed over; 0 when the count cannot be read. */
  async ordersToHandOver(): Promise<number> {
    try {
      // Only the count is needed: one item is asked for, the API returns the total.
      const page = await this.api.orders({ status: 'paid', page: 1, pageSize: 1 });
      return page.totalCount;
    } catch {
      return 0;
    }
  }
}
