#pragma warning disable CA1707

using FluentAssertions;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Application.Profitability;
using SaasCommerce.Modules.Sales.Contracts.Responses;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Tests;

public sealed class ProfitabilityUseCaseTests
{
  private static readonly DateTimeOffset From = new(2026, 5, 1, 0, 0, 0, TimeSpan.Zero);
  private static readonly DateTimeOffset To = new(2026, 5, 31, 23, 59, 59, TimeSpan.Zero);

  // ── GetProfitabilitySummaryHandler ───────────────────────────────────────

  [Fact]
  public async Task GetSummary_ShouldFail_WhenNotAuthenticated()
  {
    var handler = new GetProfitabilitySummaryHandler(new FakeProfitabilityRepo(), Anonymous());

    var result = await handler.Handle(new GetProfitabilitySummaryQuery(From, To, null));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(ProfitabilityErrors.UserContextRequired);
  }

  [Fact]
  public async Task GetSummary_ShouldSucceed_WithValidBusinessContext()
  {
    var expectedSummary = BuildSummary();
    var repo = new FakeProfitabilityRepo { Summary = expectedSummary };
    var handler = new GetProfitabilitySummaryHandler(repo, Authed());

    var result = await handler.Handle(new GetProfitabilitySummaryQuery(From, To, null));

    result.IsSuccess.Should().BeTrue();
    result.Value.TotalSales.Should().Be(10_000m);
    result.Value.GrossProfit.Should().Be(4_000m);
    result.Value.SalesCount.Should().Be(25);
  }

  [Fact]
  public async Task GetSummary_ShouldPassBranchFilterToRepository()
  {
    var branchId = Guid.NewGuid();
    var repo = new FakeProfitabilityRepo { Summary = BuildSummary() };
    var handler = new GetProfitabilitySummaryHandler(repo, Authed());

    await handler.Handle(new GetProfitabilitySummaryQuery(From, To, branchId));

    repo.LastBranchId.Should().Be(branchId);
  }

  // ── GetProductProfitabilityHandler ───────────────────────────────────────

  [Fact]
  public async Task GetProductProfitability_ShouldFail_WhenNotAuthenticated()
  {
    var handler = new GetProductProfitabilityHandler(new FakeProfitabilityRepo(), Anonymous());

    var result = await handler.Handle(new GetProductProfitabilityQuery(From, To, null, null));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(ProfitabilityErrors.UserContextRequired);
  }

  [Fact]
  public async Task GetProductProfitability_ShouldReturnEmptyList_WhenNoSales()
  {
    var handler = new GetProductProfitabilityHandler(new FakeProfitabilityRepo(), Authed());

    var result = await handler.Handle(new GetProductProfitabilityQuery(From, To, null, null));

    result.IsSuccess.Should().BeTrue();
    result.Value.Should().BeEmpty();
  }

  [Fact]
  public async Task GetProductProfitability_ShouldReturnProducts_WhenSalesExist()
  {
    var products = new[]
    {
      new ProductProfitabilityResponse(
        ProductId: Guid.NewGuid(),
        ProductName: "Producto A",
        Sku: "SKU-001",
        TotalQuantity: 10,
        TotalSales: 1_000m,
        TotalCost: 600m,
        GrossProfit: 400m,
        MarginPercent: 40m,
        HasMissingCost: false,
        CategoryId: null,
        CategoryName: null)
    };
    var repo = new FakeProfitabilityRepo { Products = products };
    var handler = new GetProductProfitabilityHandler(repo, Authed());

    var result = await handler.Handle(new GetProductProfitabilityQuery(From, To, null, null));

    result.IsSuccess.Should().BeTrue();
    result.Value.Should().HaveCount(1);
    result.Value.First().ProductName.Should().Be("Producto A");
    result.Value.First().MarginPercent.Should().Be(40m);
  }

  [Fact]
  public async Task GetProductProfitability_ShouldPassCategoryFilterToRepository()
  {
    var categoryId = Guid.NewGuid();
    var repo = new FakeProfitabilityRepo();
    var handler = new GetProductProfitabilityHandler(repo, Authed());

    await handler.Handle(new GetProductProfitabilityQuery(From, To, null, categoryId));

    repo.LastCategoryId.Should().Be(categoryId);
  }

  // ── GetBranchProfitabilityHandler ────────────────────────────────────────

  [Fact]
  public async Task GetBranchProfitability_ShouldFail_WhenNotAuthenticated()
  {
    var handler = new GetBranchProfitabilityHandler(new FakeProfitabilityRepo(), Anonymous());

    var result = await handler.Handle(new GetBranchProfitabilityQuery(From, To));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(ProfitabilityErrors.UserContextRequired);
  }

  [Fact]
  public async Task GetBranchProfitability_ShouldReturnEmptyList_WhenNoBranchSales()
  {
    var handler = new GetBranchProfitabilityHandler(new FakeProfitabilityRepo(), Authed());

    var result = await handler.Handle(new GetBranchProfitabilityQuery(From, To));

    result.IsSuccess.Should().BeTrue();
    result.Value.Should().BeEmpty();
  }

  [Fact]
  public async Task GetBranchProfitability_ShouldReturnBranches_OrderedByNetProfit()
  {
    var branchA = Guid.NewGuid();
    var branchB = Guid.NewGuid();
    var branches = new[]
    {
      new BranchProfitabilityResponse(
        BranchId: branchA, BranchName: "Sucursal A",
        TotalSales: 5_000m, TotalCost: 3_000m, GrossProfit: 2_000m,
        OperatingExpenses: 500m, EstimatedNetProfit: 1_500m, NetMarginPercent: 30m, SalesCount: 50),
      new BranchProfitabilityResponse(
        BranchId: branchB, BranchName: "Sucursal B",
        TotalSales: 2_000m, TotalCost: 1_500m, GrossProfit: 500m,
        OperatingExpenses: 300m, EstimatedNetProfit: 200m, NetMarginPercent: 10m, SalesCount: 20)
    };
    var repo = new FakeProfitabilityRepo { Branches = branches };
    var handler = new GetBranchProfitabilityHandler(repo, Authed());

    var result = await handler.Handle(new GetBranchProfitabilityQuery(From, To));

    result.IsSuccess.Should().BeTrue();
    result.Value.Should().HaveCount(2);
    result.Value.First().BranchName.Should().Be("Sucursal A");
  }

  // ── GetProfitabilityAlertsHandler ────────────────────────────────────────

  [Fact]
  public async Task GetAlerts_ShouldFail_WhenNotAuthenticated()
  {
    var handler = new GetProfitabilityAlertsHandler(new FakeProfitabilityRepo(), Anonymous());

    var result = await handler.Handle(new GetProfitabilityAlertsQuery(From, To, null));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(ProfitabilityErrors.UserContextRequired);
  }

  [Fact]
  public async Task GetAlerts_ShouldReturnEmpty_WhenNoProblems()
  {
    var handler = new GetProfitabilityAlertsHandler(new FakeProfitabilityRepo(), Authed());

    var result = await handler.Handle(new GetProfitabilityAlertsQuery(From, To, null));

    result.IsSuccess.Should().BeTrue();
    result.Value.Should().BeEmpty();
  }

  [Fact]
  public async Task GetAlerts_ShouldGenerateMissingCostAlert()
  {
    var products = new[]
    {
      new ProductProfitabilityResponse(
        ProductId: Guid.NewGuid(), ProductName: "Sin costo", Sku: "X",
        TotalQuantity: 5, TotalSales: 500m, TotalCost: 0m, GrossProfit: 500m,
        MarginPercent: 100m, HasMissingCost: true, CategoryId: null, CategoryName: null)
    };
    var repo = new FakeProfitabilityRepo { Products = products };
    var handler = new GetProfitabilityAlertsHandler(repo, Authed());

    var result = await handler.Handle(new GetProfitabilityAlertsQuery(From, To, null));

    result.IsSuccess.Should().BeTrue();
    result.Value.Should().ContainSingle(a => a.AlertType == "MissingCost");
    result.Value.First().ProductName.Should().Be("Sin costo");
  }

  [Fact]
  public async Task GetAlerts_ShouldGenerateNegativeMarginAlert()
  {
    var products = new[]
    {
      new ProductProfitabilityResponse(
        ProductId: Guid.NewGuid(), ProductName: "Pérdida", Sku: "Y",
        TotalQuantity: 10, TotalSales: 500m, TotalCost: 700m, GrossProfit: -200m,
        MarginPercent: -40m, HasMissingCost: false, CategoryId: null, CategoryName: null)
    };
    var repo = new FakeProfitabilityRepo { Products = products };
    var handler = new GetProfitabilityAlertsHandler(repo, Authed());

    var result = await handler.Handle(new GetProfitabilityAlertsQuery(From, To, null));

    result.IsSuccess.Should().BeTrue();
    result.Value.Should().ContainSingle(a => a.AlertType == "NegativeMargin");
  }

  [Fact]
  public async Task GetAlerts_ShouldGenerateHighExpensesAlert()
  {
    var branches = new[]
    {
      new BranchProfitabilityResponse(
        BranchId: Guid.NewGuid(), BranchName: "Sucursal costosa",
        TotalSales: 1_000m, TotalCost: 400m, GrossProfit: 600m,
        OperatingExpenses: 800m, EstimatedNetProfit: -200m, NetMarginPercent: -20m, SalesCount: 10)
    };
    var repo = new FakeProfitabilityRepo { Branches = branches };
    var handler = new GetProfitabilityAlertsHandler(repo, Authed());

    var result = await handler.Handle(new GetProfitabilityAlertsQuery(From, To, null));

    result.IsSuccess.Should().BeTrue();
    result.Value.Should().ContainSingle(a => a.AlertType == "HighExpenses");
    result.Value.First().BranchName.Should().Be("Sucursal costosa");
  }

  [Fact]
  public async Task GetAlerts_ShouldGenerateHighVolumeLowMarginAlert()
  {
    // Product with top volume (top 20%) and low margin (<10%)
    var products = new[]
    {
      new ProductProfitabilityResponse(
        ProductId: Guid.NewGuid(), ProductName: "Alto volumen bajo margen", Sku: "Z",
        TotalQuantity: 500, TotalSales: 5_000m, TotalCost: 4_600m, GrossProfit: 400m,
        MarginPercent: 8m, HasMissingCost: false, CategoryId: null, CategoryName: null)
    };
    var repo = new FakeProfitabilityRepo { Products = products };
    var handler = new GetProfitabilityAlertsHandler(repo, Authed());

    var result = await handler.Handle(new GetProfitabilityAlertsQuery(From, To, null));

    result.IsSuccess.Should().BeTrue();
    result.Value.Should().ContainSingle(a => a.AlertType == "HighVolumeLowMargin");
  }

  [Fact]
  public async Task GetAlerts_ShouldSortNegativeMarginFirst()
  {
    var branchId = Guid.NewGuid();
    var products = new[]
    {
      new ProductProfitabilityResponse(
        ProductId: Guid.NewGuid(), ProductName: "Sin costo", Sku: "A",
        TotalQuantity: 5, TotalSales: 200m, TotalCost: 0m, GrossProfit: 200m,
        MarginPercent: 100m, HasMissingCost: true, CategoryId: null, CategoryName: null),
      new ProductProfitabilityResponse(
        ProductId: Guid.NewGuid(), ProductName: "Negativo", Sku: "B",
        TotalQuantity: 10, TotalSales: 300m, TotalCost: 500m, GrossProfit: -200m,
        MarginPercent: -67m, HasMissingCost: false, CategoryId: null, CategoryName: null)
    };
    var repo = new FakeProfitabilityRepo { Products = products };
    var handler = new GetProfitabilityAlertsHandler(repo, Authed());

    var result = await handler.Handle(new GetProfitabilityAlertsQuery(From, To, null));

    result.IsSuccess.Should().BeTrue();
    // NegativeMargin should come before MissingCost
    result.Value.First().AlertType.Should().Be("NegativeMargin");
  }

  // ── Helpers ──────────────────────────────────────────────────────────────

  private static FakeUser Authed()
    => new() { BusinessId = Guid.NewGuid(), IsAuthenticated = true };

  private static FakeUser Anonymous()
    => new() { IsAuthenticated = false };

  private static ProfitabilitySummaryResponse BuildSummary()
    => new(
      DateFrom: From,
      DateTo: To,
      TotalSales: 10_000m,
      TotalCost: 6_000m,
      GrossProfit: 4_000m,
      OperatingExpenses: 1_000m,
      EstimatedNetProfit: 3_000m,
      GrossMarginPercent: 40m,
      NetMarginPercent: 30m,
      SalesCount: 25,
      WarningCount: 0);

  // ── Fakes ────────────────────────────────────────────────────────────────

  private sealed class FakeUser : ICurrentUserService
  {
    public Guid? UserId { get; init; }
    public Guid? BusinessId { get; init; }
    public Guid? BranchId { get; init; }
    public IReadOnlyCollection<string> Roles { get; init; } = [];
    public bool IsAuthenticated { get; init; }
  }

  private sealed class FakeProfitabilityRepo : IProfitabilityReadRepository
  {
    public ProfitabilitySummaryResponse? Summary { get; set; }
    public IReadOnlyCollection<ProductProfitabilityResponse> Products { get; set; } = [];
    public IReadOnlyCollection<BranchProfitabilityResponse> Branches { get; set; } = [];

    public Guid? LastBranchId { get; private set; }
    public Guid? LastCategoryId { get; private set; }

    public Task<ProfitabilitySummaryResponse> GetSummaryAsync(
      BusinessId businessId,
      DateTimeOffset dateFrom,
      DateTimeOffset dateTo,
      Guid? branchId,
      CancellationToken cancellationToken = default)
    {
      LastBranchId = branchId;
      return Task.FromResult(Summary ?? BuildSummary());
    }

    public Task<IReadOnlyCollection<ProductProfitabilityResponse>> GetProductProfitabilityAsync(
      BusinessId businessId,
      DateTimeOffset dateFrom,
      DateTimeOffset dateTo,
      Guid? branchId,
      Guid? categoryId,
      CancellationToken cancellationToken = default)
    {
      LastBranchId = branchId;
      LastCategoryId = categoryId;
      return Task.FromResult(Products);
    }

    public Task<IReadOnlyCollection<BranchProfitabilityResponse>> GetBranchProfitabilityAsync(
      BusinessId businessId,
      DateTimeOffset dateFrom,
      DateTimeOffset dateTo,
      CancellationToken cancellationToken = default)
      => Task.FromResult(Branches);
  }
}

#pragma warning restore CA1707
