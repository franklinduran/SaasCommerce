# Modules

Every business feature must live inside one module. API and Worker hosts can call modules, but they must not contain business rules.

Initial module ownership:

- Tenancy: businesses, branches, BusinessId, tenant configuration.
- Identity: users, roles, permissions, authentication and refresh tokens.
- Catalog: products, categories, brands and base prices.
- Inventory: stock, movements, adjustments, entries and exits.
- Sales: POS, sales, sale items, statuses, cancellations and returns.
- Customers: customers, credit accounts, receivables and customer payments.
- Billing: receipts, invoices, fiscal sequences and future tax documents.
- Payments: payment methods, sale payments, cash movements and payment validation.
- Purchasing: suppliers, purchases, purchase reception and replenishment cost events.
- Reporting: dashboards, metrics, reports and read models.

Rules:

- Do not create commands, queries, handlers, entities or business services in a global Application folder.
- Module domain entities are private to their owner module.
- Modules communicate through public contracts, integration events or read models.
- Critical consumers must use inbox/idempotency before executing business work.
