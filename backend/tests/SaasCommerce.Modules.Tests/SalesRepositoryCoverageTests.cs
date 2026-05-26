#pragma warning disable CA1707

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Catalog.Domain;
using SaasCommerce.Modules.Customers.Domain.Credits;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Application.DailyClosings;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.Modules.Sales.Infrastructure.Persistence;
using SaasCommerce.Modules.Tenancy.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Tests;

public sealed class SalesRepositoryCoverageTests
{
  private static readonly DateTimeOffset Now = new(2026, 5, 25, 12, 0, 0, TimeSpan.Zero);

  [Fact]
  public async Task ProfitabilityReadRepository_ShouldAggregateSummaryProductsAndBranches()
  {
    await using var dbContext = CreateDbContext();
    var businessId = new BusinessId(Guid.NewGuid());
    var branchId = new BranchId(Guid.NewGuid());
    var otherBranchId = new BranchId(Guid.NewGuid());
    var categoryId = Guid.NewGuid();
    var productId = Guid.NewGuid();
    var missingCostProductId = Guid.NewGuid();
    SeedBranch(dbContext, businessId, branchId, "Principal");
    SeedBranch(dbContext, businessId, otherBranchId, "Secundaria");
    dbContext.Add(new Category(categoryId, businessId, "Bebidas", null, Now));
    dbContext.Add(Product(productId, businessId, "Cafe", "SKU-CAFE", 60, categoryId));
    dbContext.Add(Product(missingCostProductId, businessId, "Servicio", "SKU-SRV", 0, null));
    dbContext.Add(CompletedSale(businessId, branchId, productId, 2, 100, null));
    dbContext.Add(CompletedSale(businessId, otherBranchId, missingCostProductId, 1, 50, null));
    dbContext.Add(ReceivedSale(businessId, branchId, productId));
    dbContext.Add(Expense(businessId, branchId, 30, OperatingExpenseStatus.Paid));
    dbContext.Add(Expense(businessId, otherBranchId, 10, OperatingExpenseStatus.Pending));
    await dbContext.SaveChangesAsync();
    var repository = new EfProfitabilityReadRepository(dbContext);

    var summary = await repository.GetSummaryAsync(businessId, Now.AddDays(-1), Now.AddDays(1), null);
    var branchSummary = await repository.GetSummaryAsync(businessId, Now.AddDays(-1), Now.AddDays(1), branchId.Value);
    var products = await repository.GetProductProfitabilityAsync(businessId, Now.AddDays(-1), Now.AddDays(1), null, categoryId);
    var branches = await repository.GetBranchProfitabilityAsync(businessId, Now.AddDays(-1), Now.AddDays(1));
    var emptyBranches = await repository.GetBranchProfitabilityAsync(new BusinessId(Guid.NewGuid()), Now.AddDays(-1), Now.AddDays(1));

    summary.TotalSales.Should().Be(250);
    summary.TotalCost.Should().Be(120);
    summary.OperatingExpenses.Should().Be(30);
    summary.SalesCount.Should().Be(2);
    summary.WarningCount.Should().Be(1);
    branchSummary.TotalSales.Should().Be(200);
    products.Should().ContainSingle(p => p.ProductId == productId && p.CategoryName == "Bebidas");
    branches.Should().HaveCount(2);
    branches.Should().Contain(b => b.BranchName == "Principal" && b.OperatingExpenses == 30);
    emptyBranches.Should().BeEmpty();
  }

  [Fact]
  public async Task DailyClosingReadRepository_ShouldListExportAndLoadDetail()
  {
    await using var dbContext = CreateDbContext();
    var businessId = new BusinessId(Guid.NewGuid());
    var branchId = new BranchId(Guid.NewGuid());
    SeedBranch(dbContext, businessId, branchId, "Principal");
    var closing = DailyClosing(businessId, branchId, new DateOnly(2026, 5, 25));
    closing.AddAlert(DailyClosingAlert.Create(Guid.NewGuid(), closing.Id, DailyClosingAlertType.HighExpenseRatio, "Diferencia", 10));
    closing.Close(Guid.NewGuid(), 515, "Cerrado", Now.AddHours(1));
    dbContext.Add(closing);
    await dbContext.SaveChangesAsync();
    var repository = new EfDailyClosingReadRepository(dbContext);

    var list = await repository.GetListAsync(businessId, branchId.Value, new DateOnly(2026, 5, 1), new DateOnly(2026, 5, 31), 1, 10);
    var export = await repository.ExportAllAsync(businessId, new DateOnly(2026, 5, 1), new DateOnly(2026, 5, 31));
    var detail = await repository.GetDetailAsync(closing.Id, businessId);
    var missing = await repository.GetDetailAsync(Guid.NewGuid(), businessId);
    var empty = await repository.GetListAsync(new BusinessId(Guid.NewGuid()), null, null, null, 1, 10);

    list.TotalCount.Should().Be(1);
    list.Items.Should().ContainSingle(item => item.BranchName == "Principal" && item.AlertCount == 1);
    export.Should().ContainSingle(item => item.Status == "Closed");
    detail.Should().NotBeNull();
    detail!.Alerts.Should().ContainSingle(alert => alert.Message == "Diferencia");
    missing.Should().BeNull();
    empty.Items.Should().BeEmpty();
  }

  [Fact]
  public async Task PreviewDailyClosingHandler_ShouldBuildPreviewAndAlerts()
  {
    await using var dbContext = CreateDbContext();
    var businessId = new BusinessId(Guid.NewGuid());
    var branchId = new BranchId(Guid.NewGuid());
    SeedBranch(dbContext, businessId, branchId, "Principal");
    await dbContext.SaveChangesAsync();
    var data = new DailyClosingData(
      TotalSales: 100,
      CashSales: 40,
      TransferSales: 10,
      CardSales: 10,
      CreditSales: 45,
      SalesCount: 3,
      CashSessionOpeningBalance: 25,
      HasOpenCashSessions: true,
      TotalExpenses: 80,
      TotalCost: 60,
      HasMissingCosts: true,
      NewCreditsAmount: 40,
      NewCreditsCount: 1,
      CreditPaymentsReceived: 15);
    var handler = new PreviewDailyClosingHandler(
      new StubDailyClosingDataGatherer(data),
      TestCurrentUser.Create(businessId.Value),
      dbContext);

    var result = await handler.Handle(new PreviewDailyClosingQuery(new DateOnly(2026, 5, 25), branchId.Value));
    var noContext = await new PreviewDailyClosingHandler(
        new StubDailyClosingDataGatherer(data),
        TestCurrentUser.Create(null),
        dbContext)
      .Handle(new PreviewDailyClosingQuery(new DateOnly(2026, 5, 25), branchId.Value));
    var unknownBranch = await handler.Handle(new PreviewDailyClosingQuery(new DateOnly(2026, 5, 25), Guid.NewGuid()));

    result.IsSuccess.Should().BeTrue();
    result.Value.BranchName.Should().Be("Principal");
    result.Value.CashExpected.Should().Be(65);
    result.Value.Alerts.Select(a => a.AlertType).Should().Contain(["OpenCashSession", "MissingProductCost", "NegativeMargin", "HighExpenseRatio", "CreditSalesHigh"]);
    noContext.Error.Should().Be(DailyClosingErrors.UserContextRequired);
    unknownBranch.Value.BranchName.Should().Be("Sucursal desconocida");
  }

  [Fact]
  public async Task DailyClosingDataGatherer_ShouldAggregateSalesCashExpensesAndCredits()
  {
    await using var dbContext = CreateDbContext();
    var businessId = new BusinessId(Guid.NewGuid());
    var branchId = new BranchId(Guid.NewGuid());
    var productId = Guid.NewGuid();
    dbContext.Add(Product(productId, businessId, "Cafe", "SKU-CAFE", 0, null));
    dbContext.Add(CompletedSale(businessId, branchId, productId, 1, 100, null, "Cash"));
    dbContext.Add(CompletedSale(businessId, branchId, productId, 1, 80, 30, "Transfer"));
    dbContext.Add(CompletedSale(businessId, branchId, productId, 1, 70, 20, "Card"));
    dbContext.Add(CompletedSale(businessId, branchId, productId, 1, 60, 10, "Credit"));
    dbContext.Add(ReceivedSale(businessId, branchId, productId));
    dbContext.Add(CashSession.Create(Guid.NewGuid(), businessId, branchId, Guid.NewGuid(), 200, null, Now));
    dbContext.Add(Expense(businessId, branchId, 45, OperatingExpenseStatus.Paid));
    dbContext.Add(CreditMovement(businessId, CustomerCreditMovementType.Debit, 60, 0, 60));
    dbContext.Add(CreditMovement(businessId, CustomerCreditMovementType.Payment, 15, 60, 45));
    await dbContext.SaveChangesAsync();

    var data = await new EfDailyClosingDataGatherer(dbContext)
      .GatherAsync(businessId, branchId, new DateOnly(2026, 5, 25));

    data.TotalSales.Should().Be(310);
    data.CashSales.Should().Be(100);
    data.TransferSales.Should().Be(80);
    data.CardSales.Should().Be(70);
    data.CreditSales.Should().Be(60);
    data.SalesCount.Should().Be(4);
    data.CashSessionOpeningBalance.Should().Be(200);
    data.HasOpenCashSessions.Should().BeTrue();
    data.TotalExpenses.Should().Be(45);
    data.TotalCost.Should().Be(60);
    data.HasMissingCosts.Should().BeTrue();
    data.NewCreditsAmount.Should().Be(60);
    data.NewCreditsCount.Should().Be(1);
    data.CreditPaymentsReceived.Should().Be(15);
  }

  [Fact]
  public async Task OperatingExpenseRepository_ShouldFilterListCountSummaryAndGet()
  {
    await using var dbContext = CreateDbContext();
    var businessId = new BusinessId(Guid.NewGuid());
    var branchId = new BranchId(Guid.NewGuid());
    var categoryId = Guid.NewGuid();
    var paid = Expense(businessId, branchId, 100, OperatingExpenseStatus.Paid, categoryId, ExpensePaymentMethod.Cash);
    var pending = Expense(businessId, branchId, 50, OperatingExpenseStatus.Pending, categoryId, ExpensePaymentMethod.Transfer, Now.AddDays(-3));
    dbContext.AddRange(paid, pending, Expense(new BusinessId(Guid.NewGuid()), branchId, 999, OperatingExpenseStatus.Paid));
    await dbContext.SaveChangesAsync();
    var repository = new EfOperatingExpenseRepository(dbContext);
    var criteria = new OperatingExpenseSearchCriteria(
      branchId.Value,
      categoryId,
      "Paid",
      "Cash",
      Now.AddDays(-1),
      Now.AddDays(1),
      1,
      10);

    var count = await repository.CountAsync(businessId, criteria);
    var list = await repository.ListAsync(businessId, criteria);
    var summary = await repository.ListForSummaryAsync(businessId, Now.AddDays(-10), Now.AddDays(1), branchId.Value);
    var loaded = await repository.GetAsync(businessId, paid.Id);
    await repository.AddAsync(Expense(businessId, branchId, 25, OperatingExpenseStatus.Pending));
    await dbContext.SaveChangesAsync();

    count.Should().Be(1);
    list.Should().ContainSingle(e => e.Id == paid.Id);
    summary.Should().HaveCount(2);
    loaded.Should().NotBeNull();
    (await repository.GetAsync(new BusinessId(Guid.NewGuid()), paid.Id)).Should().BeNull();
    (await repository.CountAsync(businessId, new OperatingExpenseSearchCriteria(null, null, "Nope", "Nope", null, null, 1, 10))).Should().Be(3);
  }

  [Fact]
  public async Task CashSessionRepository_ShouldLoadOpenListExportAndCount()
  {
    await using var dbContext = CreateDbContext();
    var businessId = new BusinessId(Guid.NewGuid());
    var branchId = new BranchId(Guid.NewGuid());
    var open = CashSession.Create(Guid.NewGuid(), businessId, branchId, Guid.NewGuid(), 100, "Inicio", Now);
    open.AddMovement(Guid.NewGuid(), Guid.NewGuid(), CashMovementType.CashIn, 50, "Venta", Now.AddMinutes(5));
    var closed = CashSession.Create(Guid.NewGuid(), businessId, branchId, Guid.NewGuid(), 100, null, Now.AddDays(-2));
    closed.Close(100, Now.AddDays(-1));
    dbContext.AddRange(open, closed, CashSession.Create(Guid.NewGuid(), new BusinessId(Guid.NewGuid()), branchId, Guid.NewGuid(), 10, null, Now));
    await dbContext.SaveChangesAsync();
    var repository = new EfCashSessionRepository(dbContext);
    var criteria = new CashSessionSearchCriteria(branchId.Value, "Open", Now.AddDays(-1), Now.AddDays(1), 1, 10);

    var hasOpen = await repository.HasOpenSessionAsync(businessId, branchId);
    var loadedOpen = await repository.GetOpenSessionAsync(businessId, branchId);
    var loaded = await repository.GetAsync(businessId, open.Id);
    var count = await repository.CountAsync(businessId, criteria);
    var list = await repository.ListAsync(businessId, criteria);
    var export = await repository.ExportAllAsync(businessId, Now.AddDays(-3), Now.AddDays(1));
    await repository.AddAsync(CashSession.Create(Guid.NewGuid(), businessId, new BranchId(Guid.NewGuid()), Guid.NewGuid(), 5, null, Now));
    await dbContext.SaveChangesAsync();

    hasOpen.Should().BeTrue();
    loadedOpen.Should().NotBeNull();
    loaded!.Movements.Should().ContainSingle();
    count.Should().Be(1);
    list.Should().ContainSingle(s => s.Id == open.Id);
    export.Should().HaveCount(2);
    (await repository.GetAsync(new BusinessId(Guid.NewGuid()), open.Id)).Should().BeNull();
    (await repository.CountAsync(businessId, new CashSessionSearchCriteria(null, "Nope", null, null, 1, 10))).Should().Be(3);
  }

  private static AppDbContext CreateDbContext()
  {
    var options = new DbContextOptionsBuilder<AppDbContext>()
      .UseInMemoryDatabase(Guid.NewGuid().ToString("D"))
      .Options;

    return new AppDbContext(options);
  }

  private static void SeedBranch(AppDbContext dbContext, BusinessId businessId, BranchId branchId, string name)
    => dbContext.Add(new Branch(branchId, businessId, name, name[..3], Now, isMain: false));

  private static Product Product(Guid id, BusinessId businessId, string name, string sku, decimal costPrice, Guid? categoryId)
    => new(
      new ProductCreationContext(id, businessId, Now),
      new ProductIdentity(ProductType.Simple, name, null, categoryId, null, UnitOfMeasure.Unit),
      new ProductCodes(sku, null, null, null),
      new ProductPricing(100, costPrice, null, null, TaxCategory.Itbis18, 18, true),
      new ProductInventorySettings(true, 1, 100, 5, false),
      new ProductOptions(true, null, null, null));

  private static Sale CompletedSale(
    BusinessId businessId,
    BranchId branchId,
    Guid productId,
    decimal quantity,
    decimal price,
    decimal? unitCost,
    string paymentMethod = "Cash")
  {
    var sale = Sale.Create(
      Guid.NewGuid(),
      businessId,
      branchId,
      Guid.NewGuid(),
      [new SaleLine(productId, quantity, price, unitCost)],
      paymentMethod,
      Now);
    sale.MarkAsProcessing(Now.AddMinutes(1));
    sale.Complete(Now.AddMinutes(2));
    return sale;
  }

  private static CustomerCreditMovement CreditMovement(
    BusinessId businessId,
    CustomerCreditMovementType type,
    decimal amount,
    decimal previousBalance,
    decimal newBalance)
    => new(
      new CreditMovementIdentifiers(Guid.NewGuid(), businessId, Guid.NewGuid(), new CreditMovementSource(SaleId: Guid.NewGuid())),
      type,
      amount,
      new CreditMovementBalance(previousBalance, newBalance),
      null,
      Now,
      Guid.NewGuid());

  private static Sale ReceivedSale(BusinessId businessId, BranchId branchId, Guid productId)
    => Sale.Create(
      Guid.NewGuid(),
      businessId,
      branchId,
      Guid.NewGuid(),
      [new SaleLine(productId, 1, 999, 999)],
      "Cash",
      Now);

  private static OperatingExpense Expense(
    BusinessId businessId,
    BranchId branchId,
    decimal amount,
    OperatingExpenseStatus status,
    Guid? categoryId = null,
    ExpensePaymentMethod paymentMethod = ExpensePaymentMethod.Cash,
    DateTimeOffset? expenseDate = null)
  {
    var draft = new OperatingExpenseDraft
    {
      Id = Guid.NewGuid(),
      BusinessId = businessId,
      BranchId = branchId,
      UserId = Guid.NewGuid(),
      CategoryId = categoryId ?? Guid.NewGuid(),
      Description = $"Gasto {amount}",
      Amount = amount,
      PaymentMethod = paymentMethod,
      ExpenseDate = expenseDate ?? Now,
      Notes = "Nota",
      CreatedAt = expenseDate ?? Now
    };

    return status switch
    {
      OperatingExpenseStatus.Paid => OperatingExpense.CreatePaid(draft),
      OperatingExpenseStatus.Cancelled => CancelledExpense(draft),
      _ => OperatingExpense.CreatePending(draft)
    };
  }

  private static OperatingExpense CancelledExpense(OperatingExpenseDraft draft)
  {
    var expense = OperatingExpense.CreatePending(draft);
    expense.Cancel(draft.CreatedAt.AddMinutes(1));
    return expense;
  }

  private static DailyClosing DailyClosing(BusinessId businessId, BranchId branchId, DateOnly closingDate)
    => SaasCommerce.Modules.Sales.Domain.DailyClosing.Create(new DailyClosingDraft
    {
      Id = Guid.NewGuid(),
      BusinessId = businessId,
      BranchId = branchId,
      CreatedByUserId = Guid.NewGuid(),
      ClosingDate = closingDate,
      TotalSales = 500,
      CashSales = 300,
      TransferSales = 100,
      CardSales = 50,
      CreditSales = 50,
      SalesCount = 5,
      CashExpected = 500,
      TotalExpenses = 75,
      TotalCost = 250,
      GrossProfit = 250,
      EstimatedNetProfit = 175,
      GrossMarginPercent = 50,
      NetMarginPercent = 35,
      NewCreditsAmount = 50,
      NewCreditsCount = 1,
      CreditPaymentsReceived = 10,
      Notes = "Draft",
      CreatedAt = Now
    });

  private sealed class StubDailyClosingDataGatherer(DailyClosingData data) : IDailyClosingDataGatherer
  {
    public Task<DailyClosingData> GatherAsync(
      BusinessId businessId,
      BranchId branchId,
      DateOnly closingDate,
      CancellationToken cancellationToken = default)
      => Task.FromResult(data);
  }

  private sealed record TestCurrentUser : ICurrentUserService
  {
    public Guid? UserId { get; init; }

    public Guid? BusinessId { get; init; }

    public Guid? BranchId { get; init; }

    public IReadOnlyCollection<string> Roles { get; init; } = [];

    public bool IsAuthenticated { get; init; }

    public static TestCurrentUser Create(Guid? businessId)
      => new()
      {
        UserId = Guid.NewGuid(),
        BusinessId = businessId,
        BranchId = Guid.NewGuid(),
        Roles = ["Admin"],
        IsAuthenticated = true
      };
  }
}

#pragma warning restore CA1707
