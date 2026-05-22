#pragma warning disable CA1707

using System.Text;
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
using SaasCommerce.Modules.Reporting.Infrastructure.Export;
using SaasCommerce.Modules.Reporting.Infrastructure.Persistence;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.Modules.Tenancy.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Tests;

public sealed class CsvReportExportServiceTests
{
  private static readonly DateTimeOffset Today = new(2026, 5, 19, 10, 0, 0, TimeSpan.Zero);

  private readonly BusinessId businessId = new(Guid.NewGuid());
  private readonly BranchId branchId = new(Guid.NewGuid());
  private readonly Guid userId = Guid.NewGuid();
  private readonly Guid customerId = Guid.NewGuid();
  private readonly Guid supplierId = Guid.NewGuid();
  private readonly Guid productId = Guid.NewGuid();

  [Fact]
  public async Task ExportSales_ShouldProduceCsvWithHeaderAndRows()
  {
    await using var db = CreateDbContext();
    await SeedAsync(db);
    var service = new CsvReportExportService(new EfReportsReadRepository(db));

    var bytes = await service.ExportSalesAsync(
      businessId.Value,
      new SalesReportCriteria(null, null, null, null, null, null, 1, 10));

    var csv = Encoding.UTF8.GetString(bytes);
    csv.Should().Contain("Venta ID,Codigo,Cliente");
    csv.Trim().Split('\n').Length.Should().BeGreaterThan(1);
  }

  [Fact]
  public async Task ExportInvoices_ShouldProduceCsvWithHeaderAndRows()
  {
    await using var db = CreateDbContext();
    await SeedAsync(db);
    var service = new CsvReportExportService(new EfReportsReadRepository(db));

    var bytes = await service.ExportInvoicesAsync(
      businessId.Value,
      new InvoiceReportCriteria(null, null, null, null, null, 1, 10));

    var csv = Encoding.UTF8.GetString(bytes);
    csv.Should().Contain("Factura ID,Numero");
    csv.Trim().Split('\n').Length.Should().BeGreaterThan(1);
  }

  [Fact]
  public async Task ExportAccountsReceivable_ShouldProduceCsvWithHeaderAndRows()
  {
    await using var db = CreateDbContext();
    await SeedAsync(db);
    var service = new CsvReportExportService(new EfReportsReadRepository(db));

    var bytes = await service.ExportAccountsReceivableAsync(
      businessId.Value,
      new AccountsReceivableCriteria(null, null, null, null, 1, 10));

    var csv = Encoding.UTF8.GetString(bytes);
    csv.Should().Contain("Cliente ID,Nombre,Telefono");
    csv.Trim().Split('\n').Length.Should().BeGreaterThan(1);
  }

  [Fact]
  public async Task ExportLowStock_ShouldProduceCsvWithHeaderAndRows()
  {
    await using var db = CreateDbContext();
    await SeedAsync(db);
    var service = new CsvReportExportService(new EfReportsReadRepository(db));

    var bytes = await service.ExportLowStockAsync(
      businessId.Value,
      new LowStockCriteria(null, null, null, 1, 10));

    var csv = Encoding.UTF8.GetString(bytes);
    csv.Should().Contain("Producto ID,Nombre,SKU");
    csv.Trim().Split('\n').Length.Should().BeGreaterThan(1);
  }

  [Fact]
  public async Task ExportPurchases_ShouldProduceCsvWithHeaderAndRows()
  {
    await using var db = CreateDbContext();
    await SeedAsync(db);
    var service = new CsvReportExportService(new EfReportsReadRepository(db));

    var bytes = await service.ExportPurchasesAsync(
      businessId.Value,
      new PurchaseReportCriteria(null, null, null, null, null, 1, 10));

    var csv = Encoding.UTF8.GetString(bytes);
    csv.Should().Contain("Compra ID,Proveedor");
    csv.Trim().Split('\n').Length.Should().BeGreaterThan(1);
  }

  [Fact]
  public async Task ExportSales_ShouldQuoteAndEscapeValues()
  {
    await using var db = CreateDbContext();
    // Customer with embedded quote to exercise the escaping branch
    var business = new Business(businessId, "Biz", Today);
    business.AddBranch(branchId, "Main", "MAIN", Today, isMain: true);
    db.Add(business);
    db.Add(new Customer(customerId, businessId, "Cliente \"Especial\"", "8095550199", "c@t.com", Today));
    var sale = Sale.Create(Guid.NewGuid(), businessId, branchId, userId,
      [new SaleLine(productId, 1, 100)], "Cash", Today);
    sale.AssignCustomer(customerId);
    sale.MarkAsProcessing(Today);
    sale.Complete(Today);
    db.Add(sale);
    await db.SaveChangesAsync();
    var service = new CsvReportExportService(new EfReportsReadRepository(db));

    var bytes = await service.ExportSalesAsync(
      businessId.Value,
      new SalesReportCriteria(null, null, null, null, null, null, 1, 10));

    var csv = Encoding.UTF8.GetString(bytes);
    csv.Should().Contain("\"\"Especial\"\"");
  }

  private async Task SeedAsync(AppDbContext db)
  {
    var business = new Business(businessId, "Test Business", Today);
    business.AddBranch(branchId, "Main", "MAIN", Today, isMain: true);
    db.Add(business);

    db.Add(new Customer(customerId, businessId, "Cliente Test", "8095550101", "c@test.com", Today));

    db.Add(new Supplier(supplierId, businessId, "Proveedor Test",
      new SupplierContactInfo(null, null, null, null), Today));

    db.Add(new Product(
      new ProductCreationContext(productId, businessId, Today),
      new ProductIdentity(ProductType.Simple, "Producto Test", null, null, null, UnitOfMeasure.Unit),
      new ProductCodes("SKU-001", null, null, null),
      new ProductPricing(100, 50, null, null, TaxCategory.Itbis18, 18, true),
      new ProductInventorySettings(true, 10, null, null, false),
      new ProductOptions(true, null, null, null)));

    var stock = new StockItem(Guid.NewGuid(), businessId, branchId, productId, Today);
    stock.ApplyAdjustment(2, InventoryMovementReason.InitialStock, userId, false, Today);
    db.Add(stock);

    var sale = Sale.Create(Guid.NewGuid(), businessId, branchId, userId,
      [new SaleLine(productId, 2, 100)], "Cash", Today);
    sale.MarkAsProcessing(Today);
    sale.Complete(Today);
    db.Add(sale);

    db.Add(Invoice.Issue(Guid.NewGuid(), sale.Id, 1,
      new InvoiceContext(businessId, branchId, customerId),
      new InvoiceFinancials(200, 0, 36, 236), Today));

    var account = new CustomerCreditAccount(Guid.NewGuid(), businessId, customerId, 1000, Today);
    account.ApplyDebit(Guid.NewGuid(), Guid.NewGuid(), 250, null, userId, Today);
    db.Add(account);

    db.Add(Purchase.Create(
      new PurchaseCreationData(Guid.NewGuid(), businessId, branchId, supplierId, userId,
        "FAC-001", Today, null, Today),
      [new PurchaseLine(productId, 5, 40)]));

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
