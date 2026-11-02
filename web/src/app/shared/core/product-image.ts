import { API } from '../infrastructure/api';

/** Address of the picture of a shop article; the version changes when the picture is replaced, so the browser never shows an old one. */
export function productImageUrl(productId: string, version: number): string {
  return `${API}/shop/products/${productId}/image?v=${version}`;
}
