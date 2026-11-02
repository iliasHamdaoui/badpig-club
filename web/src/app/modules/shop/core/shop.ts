/** A variant of an article as the board sees it (mirror of the API). */
export interface ShopVariant {
  id: string;
  label: string;
  priceCents: number;
  quantity: number;
  reserved: number;
  available: number;
  lowStockThreshold: number;
  isLowStock: boolean;
  isActive: boolean;
}

export interface ShopProduct {
  id: string;
  name: string;
  description: string;
  isActive: boolean;
  hasLowStock: boolean;
  createdAt: string;
  /** Version of the picture, null when the article has none. */
  imageVersion: number | null;
  variants: ShopVariant[];
}

export type StockReason = 'restock' | 'correction' | 'sale';

export const STOCK_REASON_LABELS: Record<StockReason, string> = {
  restock: 'Réapprovisionnement',
  correction: 'Correction',
  sale: 'Vente',
};

export interface StockMovement {
  id: string;
  change: number;
  reason: StockReason;
  note: string | null;
  authorName: string | null;
  orderId: string | null;
  createdAt: string;
}

export type ShopOrderStatus = 'pending' | 'paid' | 'delivered' | 'cancelled' | 'expired';

export const SHOP_ORDER_STATUS_LABELS: Record<ShopOrderStatus, string> = {
  pending: 'En attente de paiement',
  paid: 'À remettre',
  delivered: 'Remise',
  cancelled: 'Annulée',
  expired: 'Expirée',
};

export interface ShopOrderLine {
  productName: string;
  variantLabel: string;
  unitPriceCents: number;
  quantity: number;
}

export interface ShopOrderItem {
  id: string;
  buyerName: string;
  status: ShopOrderStatus;
  totalCents: number;
  createdAt: string;
  paidAt: string | null;
  deliveredAt: string | null;
  deliveredByName: string | null;
  lines: ShopOrderLine[];
}

/** Turns what was typed ("18,50", "20") into euro cents; null if it is not an amount. */
export function parseEuros(text: string): number | null {
  const normalized = text.trim().replace(',', '.');
  if (!/^\d+(\.\d{1,2})?$/.test(normalized)) {
    return null;
  }
  return Math.round(parseFloat(normalized) * 100);
}

/** Cents written for an input field: 1850 → "18,50". */
export function centsToInput(cents: number): string {
  return (cents / 100).toFixed(2).replace('.', ',');
}

/** One line of the shop audit log (mirror of the API). */
export interface ShopAuditEntry {
  id: number;
  occurredAt: string;
  actor: string;
  action: string;
  targetId: string | null;
  subject: string | null;
  details: string | null;
}

export const SHOP_AUDIT_ACTION_LABELS: Record<string, string> = {
  'product.created': 'Article créé',
  'product.updated': 'Article modifié',
  'product.deleted': 'Article supprimé',
  'product.image_set': 'Image ajoutée ou changée',
  'product.image_removed': 'Image retirée',
  'variant.created': 'Variante ajoutée',
  'variant.updated': 'Variante modifiée',
  'variant.deleted': 'Variante supprimée',
  'variant.restocked': 'Réapprovisionnement',
  'variant.stock_corrected': 'Correction du stock',
  'order.delivered': 'Commande remise',
};

/** The label of an action code; an unknown code is shown as it is. */
export function shopAuditActionLabel(action: string): string {
  return SHOP_AUDIT_ACTION_LABELS[action] ?? action;
}
