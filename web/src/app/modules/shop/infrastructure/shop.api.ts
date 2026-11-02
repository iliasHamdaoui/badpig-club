import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { API } from '../../../shared/infrastructure/api';
import { ListQuery, Page, listParams } from '../../../shared/core/page';
import { ShopAuditEntry, ShopOrderItem, ShopProduct, StockMovement } from '../core/shop';

/** Calls of the board's shop management (articles, stock, orders received). */
@Injectable({ providedIn: 'root' })
export class ShopApi {
  private readonly http = inject(HttpClient);
  private readonly base = `${API}/admin/shop`;

  products(query: ListQuery & { lowStock?: boolean }): Promise<Page<ShopProduct>> {
    return firstValueFrom(this.http.get<Page<ShopProduct>>(`${this.base}/products`, { params: listParams(query) }));
  }

  createProduct(name: string, description: string): Promise<{ id: string }> {
    return firstValueFrom(this.http.post<{ id: string }>(`${this.base}/products`, { name, description }));
  }

  updateProduct(id: string, name: string, description: string, isActive: boolean): Promise<void> {
    return firstValueFrom(this.http.put<void>(`${this.base}/products/${id}`, { name, description, isActive }));
  }

  deleteProduct(id: string): Promise<void> {
    return firstValueFrom(this.http.delete<void>(`${this.base}/products/${id}`));
  }

  setImage(productId: string, file: File): Promise<void> {
    const form = new FormData();
    form.append('file', file, file.name);
    return firstValueFrom(this.http.put<void>(`${this.base}/products/${productId}/image`, form));
  }

  removeImage(productId: string): Promise<void> {
    return firstValueFrom(this.http.delete<void>(`${this.base}/products/${productId}/image`));
  }

  createVariant(productId: string, label: string, priceCents: number, quantity: number, lowStockThreshold: number): Promise<{ id: string }> {
    return firstValueFrom(this.http.post<{ id: string }>(`${this.base}/products/${productId}/variants`, { label, priceCents, quantity, lowStockThreshold }));
  }

  updateVariant(id: string, label: string, priceCents: number, lowStockThreshold: number, isActive: boolean): Promise<void> {
    return firstValueFrom(this.http.put<void>(`${this.base}/variants/${id}`, { label, priceCents, lowStockThreshold, isActive }));
  }

  deleteVariant(id: string): Promise<void> {
    return firstValueFrom(this.http.delete<void>(`${this.base}/variants/${id}`));
  }

  restock(id: string, quantity: number, note: string): Promise<void> {
    return firstValueFrom(this.http.post<void>(`${this.base}/variants/${id}/restock`, { quantity, note: note || null }));
  }

  correctStock(id: string, quantity: number, note: string): Promise<void> {
    return firstValueFrom(this.http.post<void>(`${this.base}/variants/${id}/stock-correction`, { quantity, note }));
  }

  movements(id: string, query: ListQuery): Promise<Page<StockMovement>> {
    return firstValueFrom(this.http.get<Page<StockMovement>>(`${this.base}/variants/${id}/movements`, { params: listParams(query) }));
  }

  orders(query: ListQuery & { status?: string }): Promise<Page<ShopOrderItem>> {
    return firstValueFrom(this.http.get<Page<ShopOrderItem>>(`${this.base}/orders`, { params: listParams(query) }));
  }

  auditLog(query: ListQuery): Promise<Page<ShopAuditEntry>> {
    return firstValueFrom(this.http.get<Page<ShopAuditEntry>>(`${this.base}/audit-log`, { params: listParams(query) }));
  }

  deliver(orderId: string): Promise<void> {
    return firstValueFrom(this.http.post<void>(`${this.base}/orders/${orderId}/deliver`, {}));
  }
}
