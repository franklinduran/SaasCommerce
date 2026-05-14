import { createBrowserRouter } from 'react-router-dom'
import { AppShell } from '@/app/AppShell'
import { AuthPage } from '@/modules/auth/AuthPage'
import { CustomersPage } from '@/modules/customers/CustomersPage'
import { DashboardPage } from '@/modules/dashboard/DashboardPage'
import { InventoryPage } from '@/modules/inventory/InventoryPage'
import { InvoicesPage } from '@/modules/invoices/InvoicesPage'
import { ProductsPage } from '@/modules/products/ProductsPage'
import { PurchasesPage } from '@/modules/purchases/PurchasesPage'
import { ReportsPage } from '@/modules/reports/ReportsPage'
import { SalesPage } from '@/modules/sales/SalesPage'

export const router = createBrowserRouter([
  {
    element: <AppShell />,
    children: [
      { index: true, element: <DashboardPage /> },
      { path: 'auth', element: <AuthPage /> },
      { path: 'sales', element: <SalesPage /> },
      { path: 'products', element: <ProductsPage /> },
      { path: 'inventory', element: <InventoryPage /> },
      { path: 'customers', element: <CustomersPage /> },
      { path: 'purchases', element: <PurchasesPage /> },
      { path: 'invoices', element: <InvoicesPage /> },
      { path: 'reports', element: <ReportsPage /> },
    ],
  },
])
