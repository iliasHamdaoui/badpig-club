import { Routes } from '@angular/router';
import { memberGuard } from '../../shared/infrastructure/guards';

// The shop of the members: guards only hide the pages, the API decides who may order.
export const ordersRoutes: Routes = [
  { path: 'shop', canActivate: [memberGuard], loadComponent: () => import('./features/catalogue/catalogue.page').then((m) => m.CataloguePage) },
  { path: 'shop/orders', canActivate: [memberGuard], loadComponent: () => import('./features/my-orders/my-orders.page').then((m) => m.MyOrdersPage) },
  { path: 'shop/orders/:id', canActivate: [memberGuard], loadComponent: () => import('./features/order-detail/order-detail.page').then((m) => m.OrderDetailPage) },
];
