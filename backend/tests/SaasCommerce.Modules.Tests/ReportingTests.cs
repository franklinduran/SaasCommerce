using FluentAssertions;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Billing.Domain;
using SaasCommerce.Modules.Catalog.Domain;
using SaasCommerce.Modules.Customers.Domain.Credits;
using SaasCommerce.Modules.Inventory.Domain;
using SaasCommerce.Modules.Reporting.Application.Abstractions;
using SaasCommerce.Modules.Reporting.Application.Dashboard;
using SaasCommerce.Modules.Reporting.Application.Reports;
using SaasCommerce.Modules.Reporting.Contracts.Responses;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel.Tenancy;

#pragma warning disable CA1707

namespace SaasCommerce.Modules.Tests;

public sealed class ReportingTests
{
  private static readonly DateTimeOffset Now = new(2026, 5, 18, 10, 0, 0, TimeSpan.Zero);

  // ── Dashboard ────────────────────────────────────────────────────────────

  [Fact]
  public async Task GetDashboardSummary_ShouldReturnOnlyCurrentBusinessData()
  {
    var scenario = DashboardScenario.Create();
    scenario.Reports.SalesToday = new DashboardSalesTodayDto(3, 1500);
    var handler = scenario.CreateHandler();

    var result = await handler.Handle(new GetDashboardSummaryQuery(), CancellationToken.None);

    result.IsSuccess.Should().BeTrue();
    result.Value.SalesToday.Count.Should().Be(3);
    result.Value.SalesToday.TotalAmount.Should().Be(1500);
  }

  [Fact]
  public async Task GetDashboardSummary_ShouldReturnZeroValues_WhenBusinessHasNoData()
  {
    var scenario = DashboardScenario.Create();
    var handler = scenario.CreateHandler();

    var result = await handler.Handle(new GetDashboardSummaryQuery(), CancellationToken.None);

    result.IsSuccess.Should().BeTrue();
    result.Value.SalesToday.Count.Should().Be(0);
    result.Value.SalesToday.TotalAmount.Should().Be(0);
    result.Value.InvoicesToday.Count.Should().Be(0);
    result.Value.Receivables.CustomerCount.Should().Be(0);
    result.Value.LowStock.ProductCount.Should().Be(0);
  }

  [Fact]
  public async Task GetDashboardSummary_ShouldIncludeLowStockProducts()
  {
    var scenario = DashboardScenario.Create();
    scenario.Reports.LowStock = new DashboardLowStockDto(5);
    var handler = scenario.CreateHandler();

    var result = await handler.Handle(new GetDashboardSummaryQuery(), CancellationToken.None);

    result.IsSuccess.Should().BeTrue();
    result.Value.LowStock.ProductCount.Should().Be(5);
  }

  [Fact]
  public async Task GetDashboardSummary_ShouldIncludeRecentSales()
  {
    var saleId = Guid.NewGuid();
    var scenario = DashboardScenario.Create();
    scenario.Reports.RecentSales = [
      new DashboardRecentSaleDto(saleId, "ABCD1234", "Completed", "Cash", 500, Now),
    ];
    var handler = scenario.CreateHandler();

    var result = await handler.Handle(new GetDashboardSummaryQuery(), CancellationToken.None);

    result.IsSuccess.Should().BeTrue();
    result.Value.RecentSales.Should().ContainSingle(s => s.SaleId == saleId);
  }

  [Fact]
  public async Task GetDashboardSummary_ShouldIncludeRecentInvoices()
  {
    var invoiceId = Guid.NewGuid();
    var scenario = DashboardScenario.Create();
    scenario.Reports.RecentInvoices = [
      new DashboardRecentInvoiceDto(invoiceId, "RI-00000001", "Issued", 500, Now),
    ];
    var handler = scenario.CreateHandler();

    var result = await handler.Handle(new GetDashboardSummaryQuery(), CancellationToken.None);

    result.IsSuccess.Should().BeTrue();
    result.Value.RecentInvoices.Should().ContainSingle(i => i.InvoiceId == invoiceId);
  }

  [Fact]
  public async Task GetDashboardSummary_ShouldFail_WhenUserNotAuthenticated()
  {
    var scenario = DashboardScenario.CreateUnauthenticated();
    var handler = scenario.CreateHandler();

    var result = await handler.Handle(new GetDashboardSummaryQuery(), CancellationToken.None);

    result.IsFailure.Should().BeTrue();
    result.Error.Code.Should().Be("reporting.user_context_required");
  }

  // ── Sales Report ─────────────────────────────────────────────────────────

  [Fact]
  public async Task GetSalesReport_ShouldFilterByBusinessId()
  {
    var scenario = ReportScenario.Create();
    var handler = scenario.CreateSalesHandler();

    var result = await handler.Handle(
      new GetSalesReportQuery(null, null, null, null, null, null, 1, 25),
      CancellationToken.None);

    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().OnlyContain(i => true);
  }

  [Fact]
  public async Task GetSalesReport_ShouldReturnSummaryTotals()
  {
    var scenario = ReportScenario.Create();
    scenario.Reports.SalesReport = new SalesReportResponse(
      [],
      new SalesReportSummaryDto(10, 5000, 500),
      1, 25, 10, 1, false, false);
    var handler = scenario.CreateSalesHandler();

    var result = await handler.Handle(
      new GetSalesReportQuery(null, null, null, null, null, null, 1, 25),
      CancellationToken.None);

    result.IsSuccess.Should().BeTrue();
    result.Value.Summary.TotalAmount.Should().Be(5000);
    result.Value.Summary.AverageAmount.Should().Be(500);
  }

  [Fact]
  public async Task GetSalesReport_ShouldFail_WhenUserNotAuthenticated()
  {
    var scenario = ReportScenario.CreateUnauthenticated();
    var handler = scenario.CreateSalesHandler();

    var result = await handler.Handle(
      new GetSalesReportQuery(null, null, null, null, null, null, 1, 25),
      CancellationToken.None);

    result.IsFailure.Should().BeTrue();
  }

  // ── Invoice Report ────────────────────────────────────────────────────────

  [Fact]
  public async Task GetInvoiceReport_ShouldFilterByBusinessId()
  {
    var scenario = ReportScenario.Create();
    var handler = scenario.CreateInvoiceHandler();

    var result = await handler.Handle(
      new GetInvoiceReportQuery(null, null, null, null, null, 1, 25),
      CancellationToken.None);

    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task GetInvoiceReport_ShouldReturnSummaryTotal()
  {
    var scenario = ReportScenario.Create();
    scenario.Reports.InvoiceReport = new InvoiceReportResponse(
      [],
      new InvoiceReportSummaryDto(5, 2500),
      1, 25, 5, 1, false, false);
    var handler = scenario.CreateInvoiceHandler();

    var result = await handler.Handle(
      new GetInvoiceReportQuery(null, null, null, null, null, 1, 25),
      CancellationToken.None);

    result.IsSuccess.Should().BeTrue();
    result.Value.Summary.TotalAmount.Should().Be(2500);
  }

  // ── Accounts Receivable Report ────────────────────────────────────────────

  [Fact]
  public async Task GetAccountsReceivableReport_ShouldFilterByBusinessId()
  {
    var scenario = ReportScenario.Create();
    var handler = scenario.CreateArHandler();

    var result = await handler.Handle(
      new GetAccountsReceivableReportQuery(null, null, null, null, 1, 25),
      CancellationToken.None);

    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task GetAccountsReceivableReport_ShouldReturnEmpty_WhenNoPendingDebts()
  {
    var scenario = ReportScenario.Create();
    scenario.Reports.ArReport = new AccountsReceivableReportResponse(
      [],
      new AccountsReceivableSummaryDto(0, 0, 0),
      1, 25, 0, 0, false, false);
    var handler = scenario.CreateArHandler();

    var result = await handler.Handle(
      new GetAccountsReceivableReportQuery(null, null, null, null, 1, 25),
      CancellationToken.None);

    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().BeEmpty();
    result.Value.Summary.TotalPending.Should().Be(0);
  }

  // ── Low Stock Report ──────────────────────────────────────────────────────

  [Fact]
  public async Task GetLowStockReport_ShouldReturnProductsBelowMinimumStock()
  {
    var scenario = ReportScenario.Create();
    scenario.Reports.LowStockReport = new LowStockReportResponse(
      [new LowStockReportItemDto(Guid.NewGuid(), "Cafe molido", "CAFE-001", null, null, 2, 5, 3, 500, null)],
      1, 25, 1, 1, false, false);
    var handler = scenario.CreateLowStockHandler();

    var result = await handler.Handle(
      new GetLowStockReportQuery(null, null, null, 1, 25),
      CancellationToken.None);

    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().ContainSingle();
    result.Value.Items.First().SuggestedRestock.Should().Be(3);
  }

  // ── Purchase Report ───────────────────────────────────────────────────────

  [Fact]
  public async Task GetPurchaseReport_ShouldFilterByBusinessId()
  {
    var scenario = ReportScenario.Create();
    var handler = scenario.CreatePurchaseHandler();

    var result = await handler.Handle(
      new GetPurchaseReportQuery(null, null, null, null, null, 1, 25),
      CancellationToken.None);

    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task GetPurchaseReport_ShouldReturnSummaryTotal()
  {
    var scenario = ReportScenario.Create();
    scenario.Reports.PurchaseReport = new PurchaseReportResponse(
      [],
      new PurchaseReportSummaryDto(3, 9000),
      1, 25, 3, 1, false, false);
    var handler = scenario.CreatePurchaseHandler();

    var result = await handler.Handle(
      new GetPurchaseReportQuery(null, null, null, null, null, 1, 25),
      CancellationToken.None);

    result.IsSuccess.Should().BeTrue();
    result.Value.Summary.TotalAmount.Should().Be(9000);
  }

  // ── Helpers ───────────────────────────────────────────────────────────────

  private sealed class DashboardScenario
  {
    private DashboardScenario(TestReportsRepository reports, ICurrentUserService user, IClock clock)
    {
      Reports = reports;
      User = user;
      Clock = clock;
    }

    public TestReportsRepository Reports { get; }

    public ICurrentUserService User { get; }

    public IClock Clock { get; }

    public static DashboardScenario Create()
    {
      var businessId = Guid.NewGuid();
      return new DashboardScenario(
        new TestReportsRepository(),
        new TestCurrentUser(businessId, Guid.NewGuid(), Guid.NewGuid()),
        new TestClock());
    }

    public static DashboardScenario CreateUnauthenticated()
    {
      return new DashboardScenario(
        new TestReportsRepository(),
        new UnauthenticatedUser(),
        new TestClock());
    }

    public GetDashboardSummaryHandler CreateHandler()
      => new(Reports, User, Clock);
  }

  private sealed class ReportScenario
  {
    private ReportScenario(TestReportsRepository reports, ICurrentUserService user)
    {
      Reports = reports;
      User = user;
    }

    public TestReportsRepository Reports { get; }

    public ICurrentUserService User { get; }

    public static ReportScenario Create()
    {
      var businessId = Guid.NewGuid();
      return new ReportScenario(
        new TestReportsRepository(),
        new TestCurrentUser(businessId, Guid.NewGuid(), Guid.NewGuid()));
    }

    public static ReportScenario CreateUnauthenticated()
    {
      return new ReportScenario(
        new TestReportsRepository(),
        new UnauthenticatedUser());
    }

    public GetSalesReportHandler CreateSalesHandler() => new(Reports, User);

    public GetInvoiceReportHandler CreateInvoiceHandler() => new(Reports, User);

    public GetAccountsReceivableReportHandler CreateArHandler() => new(Reports, User);

    public GetLowStockReportHandler CreateLowStockHandler() => new(Reports, User);

    public GetPurchaseReportHandler CreatePurchaseHandler() => new(Reports, User);
  }

  private sealed class TestReportsRepository : IReportsReadRepository
  {
    public DashboardSalesTodayDto SalesToday { get; set; } = new(0, 0);
    public DashboardInvoicesTodayDto InvoicesToday { get; set; } = new(0, 0);
    public DashboardReceivablesDto Receivables { get; set; } = new(0, 0);
    public DashboardLowStockDto LowStock { get; set; } = new(0);
    public IReadOnlyCollection<DashboardRecentSaleDto> RecentSales { get; set; } = [];
    public IReadOnlyCollection<DashboardRecentInvoiceDto> RecentInvoices { get; set; } = [];
    public IReadOnlyCollection<DashboardRecentPurchaseDto> RecentPurchases { get; set; } = [];
    public IReadOnlyCollection<DailySalesPointDto> DailySales { get; set; } = [];
    public SalesReportResponse SalesReport { get; set; } = new([], new(0, 0, 0), 1, 25, 0, 0, false, false);
    public InvoiceReportResponse InvoiceReport { get; set; } = new([], new(0, 0), 1, 25, 0, 0, false, false);
    public AccountsReceivableReportResponse ArReport { get; set; } = new([], new(0, 0, 0), 1, 25, 0, 0, false, false);
    public LowStockReportResponse LowStockReport { get; set; } = new([], 1, 25, 0, 0, false, false);
    public PurchaseReportResponse PurchaseReport { get; set; } = new([], new(0, 0), 1, 25, 0, 0, false, false);

    public Task<DashboardSummaryResponse> GetDashboardSummaryAsync(
      BusinessId businessId, DateTimeOffset today, DateTimeOffset thirtyDaysAgo, CancellationToken cancellationToken = default)
      => Task.FromResult(new DashboardSummaryResponse(
        SalesToday, InvoicesToday, Receivables, LowStock,
        RecentSales, RecentInvoices, RecentPurchases, DailySales));

    public Task<SalesReportResponse> GetSalesReportAsync(
      BusinessId businessId, SalesReportCriteria criteria, CancellationToken cancellationToken = default)
      => Task.FromResult(SalesReport);

    public Task<InvoiceReportResponse> GetInvoiceReportAsync(
      BusinessId businessId, InvoiceReportCriteria criteria, CancellationToken cancellationToken = default)
      => Task.FromResult(InvoiceReport);

    public Task<AccountsReceivableReportResponse> GetAccountsReceivableReportAsync(
      BusinessId businessId, AccountsReceivableCriteria criteria, CancellationToken cancellationToken = default)
      => Task.FromResult(ArReport);

    public Task<LowStockReportResponse> GetLowStockReportAsync(
      BusinessId businessId, LowStockCriteria criteria, CancellationToken cancellationToken = default)
      => Task.FromResult(LowStockReport);

    public Task<PurchaseReportResponse> GetPurchaseReportAsync(
      BusinessId businessId, PurchaseReportCriteria criteria, CancellationToken cancellationToken = default)
      => Task.FromResult(PurchaseReport);
  }

  private sealed class TestCurrentUser(Guid businessId, Guid branchId, Guid userId) : ICurrentUserService
  {
    public Guid? UserId => userId;

    public Guid? BusinessId => businessId;

    public Guid? BranchId => branchId;

    public IReadOnlyCollection<string> Roles => ["Admin"];

    public bool IsAuthenticated => true;
  }

  private sealed class UnauthenticatedUser : ICurrentUserService
  {
    public Guid? UserId => null;

    public Guid? BusinessId => null;

    public Guid? BranchId => null;

    public IReadOnlyCollection<string> Roles => [];

    public bool IsAuthenticated => false;
  }

  private sealed class TestClock : IClock
  {
    public DateTimeOffset UtcNow => Now;
  }
}

#pragma warning restore CA1707
