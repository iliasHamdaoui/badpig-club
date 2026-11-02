import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { API } from '../../../shared/infrastructure/api';
import { ListQuery, Page, listParams } from '../../../shared/core/page';
import { CatalogProduct, MyOrder, MyOrderItem } from '../core/orders';

@Injectable({ providedIn: 'root' })
export class OrdersApi {
  private readonly http = inject(HttpClient);

  catalog(query: ListQuery): Promise<Page<CatalogProduct>> {
    return firstValueFrom(this.http.get<Page<CatalogProduct>>(`${API}/shop/products`, { params: listParams(query) }));
  }

  /** Makes an order: the stock is held for 35 minutes. Returns the order identifier. */
  createOrder(lines: { variantId: string; quantity: number }[]): Promise<{ id: string }> {
    return firstValueFrom(this.http.post<{ id: string }>(`${API}/shop/orders`, { lines }));
  }

  myOrders(query: ListQuery): Promise<Page<MyOrderItem>> {
    return firstValueFrom(this.http.get<Page<MyOrderItem>>(`${API}/shop/orders`, { params: listParams(query) }));
  }

  /** Reading a pending order also asks the payment provider, so the answer is up to date. */
  order(id: string): Promise<MyOrder> {
    return firstValueFrom(this.http.get<MyOrder>(`${API}/shop/orders/${id}`));
  }

  cancel(id: string): Promise<void> {
    return firstValueFrom(this.http.post<void>(`${API}/shop/orders/${id}/cancel`, {}));
  }
}
