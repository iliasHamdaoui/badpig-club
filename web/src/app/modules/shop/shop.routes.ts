import { Routes } from '@angular/router';
import { boardGuard } from '../../shared/infrastructure/guards';

// Shop management of the board: guards only hide the pages, the API decides who may act.
export const shopRoutes: Routes = [
  { path: 'admin/shop', canActivate: [boardGuard], loadComponent: () => import('./features/manage-products/manage-products.page').then((m) => m.ManageProductsPage) },
  { path: 'admin/shop/orders', canActivate: [boardGuard], loadComponent: () => import('./features/shop-orders/shop-orders.page').then((m) => m.ShopOrdersPage) },
  { path: 'admin/shop/audit-log', canActivate: [boardGuard], loadComponent: () => import('./features/shop-audit/shop-audit.page').then((m) => m.ShopAuditPage) },
  { path: 'admin/shop/variants/:id/history', canActivate: [boardGuard], loadComponent: () => import('./features/stock-history/stock-history.page').then((m) => m.StockHistoryPage) },
];
