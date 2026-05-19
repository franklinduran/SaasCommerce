#pragma warning disable CA1707

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Billing.Domain;
using SaasCommerce.Modules.Catalog.Domain;
using SaasCommerce.Modules.Customers.Domain;
using SaasCommerce.Modules.Customers.Domain.Credits;
using SaasCommerce.Modules.Inventory.Domain;
using SaasCommerce.Modules.Purchasing.Domain;
using SaasCommerce.Modules.Reporting.Application.Abstractions;
using SaasCommerce.Modules.Reporting.Infrastructure.Persistence;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.Modules.Tenancy.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Tests;

public sealed class ReportsReadRepositoryTests
{
  private static readonly DateTimeOffset Today = new(2026, 5, 19, 10, 0, 0, TimeSpan.Zero);

  private readonly BusinessId businessId = new(Guid.NewGuid());
  private readonly BranchId branchId = new(Guid.NewGuid());
  private readonly Guid userId = Guid.NewGuid();
  private readonly Guid customerId = Guid.NewGuid();
  private readonly Guid supplierId = Guid.NewGuid();
  private readonly Guid productId = Guid.NewGuid();

  // ── GetDashboardSummaryAsync ─────────────────────────────────────────────

  [Fact]
  public async Task GetDashboardSummary_ShouldAggregateAllSections()
  {
    await using var db = CreateDbContext();
    await SeedFullDatasetAsync(db);
    var repo = new EfReportsReadRepository(db);

    var result = await repo.GetDashboardSummaryAsync(businessId, Today);

    result.SalesToday.Count.Should().BeGreaterThan(0);
    result.InvoicesToday.Count.Should().BeGreaterThan(0);
    result.Receivables.CustomerCount.Should().BeGreaterThan(0);
    result.LowStock.ProductCount.Should().BeGreaterThan(0);
    result.RecentSales.Should().NotBeEmpty();
    result.RecentInvoices.Should().NotBeEmpty();
    result.RecentPurchases.Should().NotBeEmpty();
  }

  [Fact]
  public async Task GetDashboardSummary_ShouldReturnZeros_WhenNoData()
  {
    await using var db = CreateDbContext();
    var repo = new EfReportsReadRepository(db);

    var result = await repo.GetDashboardSummaryAsync(businessId, Today);

    result.SalesToday.Count.Should().Be(0);
    result.InvoicesToday.Count.Should().Be(0);
    result.Receivables.CustomerCount.Should().Be(0);
    result.LowStock.ProductCount.Should().Be(0);
    result.RecentSales.Should().BeEmpty();
  }

  // ── GetSalesReportAsync ──────────────────────────────────────────────────

  [Fact]
  public async Task GetSalesReport_ShouldReturnItems_NoFilters()
  {
    await using var db = CreateDbContext();
    await SeedFullDatasetAsync(db);
    var repo = new EfReportsReadRepository(db);

    var result = await repo.GetSalesReportAsync(
      businessId,
      new SalesReportCriteria(null, null, null, null, null, null, 1, 20));

    result.Items.Should().NotBeEmpty();
    result.Summary.TotalCount.Should().BeGreaterThan(0);
    result.TotalItems.Should().BeGreaterThan(0);
  }

  [Fact]
  public async Task GetSalesReport_ShouldApplyAllFilters()
  {
    await using var db = CreateDbContext();
    await SeedFullDatasetAsync(db);
    var repo = new EfReportsReadRepository(db);

    var result = await repo.GetSalesReportAsync(
      businessId,
      new SalesReportCriteria(
        Today.AddDays(-1), Today.AddDays(1), branchId.Value,
        "Completed", "Cash", "0", 1, 20));

    result.Should().NotBeNull();
    result.Page.Should().Be(1);
  }

  // ── GetInvoiceReportAsync ────────────────────────────────────────────────

  [Fact]
  public async Task GetInvoiceReport_ShouldReturnItems_NoFilters()
  {
    await using var db = CreateDbContext();
    await SeedFullDatasetAsync(db);
    var repo = new EfReportsReadRepository(db);

    var result = await repo.GetInvoiceReportAsync(
      businessId,
      new InvoiceReportCriteria(null, null, null, null, null, 1, 20));

    result.Items.Should().NotBeEmpty();
    result.Summary.TotalCount.Should().BeGreaterThan(0);
  }

  [Fact]
  public async Task GetInvoiceReport_ShouldApplyAllFilters()
  {
    await using var db = CreateDbContext();
    await SeedFullDatasetAsync(db);
    var repo = new EfReportsReadRepository(db);

    var result = await repo.GetInvoiceReportAsync(
      businessId,
      new InvoiceReportCriteria(
        Today.AddDays(-1), Today.AddDays(1), "Issued", customerId, "RI", 1, 20));

    result.Should().NotBeNull();
  }

  // ── GetAccountsReceivableReportAsync ─────────────────────────────────────

  [Fact]
  public async Task GetAccountsReceivableReport_ShouldReturnItems_NoFilters()
  {
    await using var db = CreateDbContext();
    await SeedFullDatasetAsync(db);
    var repo = new EfReportsReadRepository(db);

    var result = await repo.GetAccountsReceivableReportAsync(
      businessId,
      new AccountsReceivableCriteria(null, null, null, null, 1, 20));

    result.Items.Should().NotBeEmpty();
    result.Summary.TotalCustomers.Should().BeGreaterThan(0);
  }

  [Fact]
  public async Task GetAccountsReceivableReport_ShouldApplyAllFilters()
  {
    await using var db = CreateDbContext();
    await SeedFullDatasetAsync(db);
    var repo = new EfReportsReadRepository(db);

    var result = await repo.GetAccountsReceivableReportAsync(
      businessId,
      new AccountsReceivableCriteria(
        customerId, "Active", Today.AddDays(-1), Today.AddDays(1), 1, 20));

    result.Should().NotBeNull();
  }

  // ── GetLowStockReportAsync ───────────────────────────────────────────────

  [Fact]
  public async Task GetLowStockReport_ShouldReturnItems_NoFilters()
  {
    await using var db = CreateDbContext();
    await SeedFullDatasetAsync(db);
    var repo = new EfReportsReadRepository(db);

    var result = await repo.GetLowStockReportAsync(
      businessId,
      new LowStockCriteria(null, null, null, 1, 20));

    result.Items.Should().NotBeEmpty();
  }

  [Fact]
  public async Task GetLowStockReport_ShouldApplyAllFilters()
  {
    await using var db = CreateDbContext();
    await SeedFullDatasetAsync(db);
    var repo = new EfReportsReadRepository(db);

    var result = await repo.GetLowStockReportAsync(
      businessId,
      new LowStockCriteria(branchId.Value, Guid.NewGuid(), "Producto", 1, 20));

    result.Should().NotBeNull();
  }

  // ── GetPurchaseReportAsync ───────────────────────────────────────────────

  [Fact]
  public async Task GetPurchaseReport_ShouldReturnItems_NoFilters()
  {
    await using var db = CreateDbContext();
    await SeedFullDatasetAsync(db);
    var repo = new EfReportsReadRepository(db);

    var result = await repo.GetPurchaseReportAsync(
      businessId,
      new PurchaseReportCriteria(null, null, null, null, null, 1, 20));

    result.Items.Should().NotBeEmpty();
    result.Summary.TotalCount.Should().BeGreaterThan(0);
  }

  [Fact]
  public async Task GetPurchaseReport_ShouldApplyAllFilters()
  {
    await using var db = CreateDbContext();
    await SeedFullDatasetAsync(db);
    var repo = new EfReportsReadRepository(db);

    var result = await repo.GetPurchaseReportAsync(
      businessId,
      new PurchaseReportCriteria(
        Today.AddDays(-1), Today.AddDays(1), supplierId, "Draft", branchId.Value, 1, 20));

    result.Should().NotBeNull();
  }

  // ── Seeding ──────────────────────────────────────────────────────────────

  private async Task SeedFullDatasetAsync(AppDbContext db)
  {
    var business = new Business(businessId, "Test Business", Today);
    business.AddBranch(branchId, "Main", Today, isMain: true);
    db.Add(business);

    var customer = new Customer(customerId, businessId, "Cliente Test", "8095550101", "cliente@test.com", Today);
    db.Add(customer);

    var supplier = new Supplier(
      supplierId, businessId, "Proveedor Test",
      new SupplierContactInfo(null, null, null, null), Today);
    db.Add(supplier);

    var product = new Product(
      new ProductCreationContext(productId, businessId, Today),
      new ProductIdentity(ProductType.Simple, "Producto Test", null, null, null, UnitOfMeasure.Unit),
      new ProductCodes("SKU-001", null, null, null),
      new ProductPricing(100, 50, null, null, TaxCategory.Itbis18, 18, true),
      new ProductInventorySettings(true, 10, null, null, false),
      new ProductOptions(true, null, null, null));
    db.Add(product);

    // Low stock: quantity 2 <= minimum 10
    var stockItem = new StockItem(Guid.NewGuid(), businessId, branchId, productId, Today);
    stockItem.ApplyAdjustment(2, InventoryMovementReason.InitialStock, userId, false, Today);
    db.Add(stockItem);
    foreach (var m in db.ChangeTracker.Entries<InventoryMovement>()) { _ = m; }

    var movement = new InventoryMovement(
      Guid.NewGuid(),
      new InventoryMovementSnapshot(
        businessId, branchId, productId, 0, 2, 2, InventoryMovementReason.InitialStock),
      userId, Today);
    db.Add(movement);

    // Completed sale today
    var sale = Sale.Create(
      Guid.NewGuid(), businessId, branchId, userId,
      [new SaleLine(productId, 2, 100)], "Cash", Today);
    sale.MarkAsProcessing(Today);
    sale.Complete(Today);
    db.Add(sale);

    // Issued invoice today
    var invoice = Invoice.Issue(
      Guid.NewGuid(), sale.Id, 1,
      new InvoiceContext(businessId, branchId, customerId),
      new InvoiceFinancials(200, 0, 36, 236), Today);
    db.Add(invoice);

    // Credit account with positive balance
    var account = new CustomerCreditAccount(Guid.NewGuid(), businessId, customerId, 1000, Today);
    account.ApplyDebit(Guid.NewGuid(), Guid.NewGuid(), 250, null, userId, Today);
    db.Add(account);

    // Purchase
    var purchase = Purchase.Create(
      new PurchaseCreationData(
        Guid.NewGuid(), businessId, branchId, supplierId, userId,
        "FAC-001", Today, null, Today),
      [new PurchaseLine(productId, 5, 40)]);
    db.Add(purchase);

    await db.SaveChangesAsync();
  }

  private static AppDbContext CreateDbContext()
  {
    var options = new DbContextOptionsBuilder<AppDbContext>()
      .UseInMemoryDatabase(Guid.NewGuid().ToString("D"))
      .Options;

    return new AppDbContext(options);
  }
}

#pragma warning restore CA1707
