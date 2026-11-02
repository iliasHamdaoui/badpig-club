/** An offered variant of the catalogue (mirror of the API). */
export interface CatalogVariant {
  id: string;
  label: string;
  priceCents: number;
  available: number;
  inStock: boolean;
}

export interface CatalogProduct {
  id: string;
  name: string;
  description: string;
  /** Version of the picture, null when the article has none. */
  imageVersion: number | null;
  variants: CatalogVariant[];
}

export type OrderStatus = 'pending' | 'paid' | 'delivered' | 'cancelled' | 'expired';

export interface OrderLine {
  productName: string;
  variantLabel: string;
  unitPriceCents: number;
  quantity: number;
}

export interface MyOrderItem {
  id: string;
  status: OrderStatus;
  totalCents: number;
  createdAt: string;
  itemCount: number;
}

export interface MyOrder {
  id: string;
  status: OrderStatus;
  totalCents: number;
  createdAt: string;
  expiresAt: string;
  paidAt: string | null;
  deliveredAt: string | null;
  /** Address of the payment page; only while the order waits for its payment. */
  checkoutUrl: string | null;
  lines: OrderLine[];
}

/** What a member sees for each state of an order. */
export const ORDER_STATUS_LABELS: Record<OrderStatus, string> = {
  pending: 'En attente de paiement',
  paid: 'Payée',
  delivered: 'Remise',
  cancelled: 'Annulée',
  expired: 'Expirée',
};

/** Most units of one article in an order (same limit as the API). */
export const MAX_QUANTITY_PER_LINE = 10;

/** One line of the basket being built on the catalogue page. */
export interface BasketLine {
  variantId: string;
  productName: string;
  variantLabel: string;
  unitPriceCents: number;
  quantity: number;
}

export function basketTotal(lines: BasketLine[]): number {
  return lines.reduce((sum, l) => sum + l.unitPriceCents * l.quantity, 0);
}
