using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Contracts.Responses;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Application.Profitability;

public sealed record GetProfitabilityAlertsQuery(
  DateTimeOffset DateFrom,
  DateTimeOffset DateTo,
  Guid? BranchId);

public sealed class GetProfitabilityAlertsHandler(
  IProfitabilityReadRepository repository,
  ICurrentUserService currentUser)
{
  private const decimal LowMarginThreshold = 10m;
  private const decimal HighVolumePercentile = 0.80m; // top 20% by quantity

  public async Task<Result<IReadOnlyCollection<ProfitabilityAlertResponse>>> Handle(
    GetProfitabilityAlertsQuery query,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(query);

    if (currentUser.BusinessId is not Guid businessId)
    {
      return Result.Failure<IReadOnlyCollection<ProfitabilityAlertResponse>>(
        ProfitabilityErrors.UserContextRequired);
    }

    var bId = new BusinessId(businessId);

    var products = await repository.GetProductProfitabilityAsync(
      bId, query.DateFrom, query.DateTo, query.BranchId, null, cancellationToken);

    var branches = await repository.GetBranchProfitabilityAsync(
      bId, query.DateFrom, query.DateTo, cancellationToken);

    var alerts = new List<ProfitabilityAlertResponse>();

    AddMissingCostAlerts(products, alerts);
    AddNegativeMarginAlerts(products, alerts);
    AddHighVolumeLowMarginAlerts(products, alerts);
    AddHighExpenseAlerts(branches, alerts);

    var sorted = alerts
      .OrderBy(a => SortRank(a.AlertType))
      .ThenByDescending(a => Math.Abs(a.EstimatedImpact ?? 0))
      .ToArray();

    return Result.Success<IReadOnlyCollection<ProfitabilityAlertResponse>>(sorted);
  }

  private static void AddMissingCostAlerts(
    IReadOnlyCollection<ProductProfitabilityResponse> products,
    List<ProfitabilityAlertResponse> alerts)
  {
    foreach (var product in products.Where(p => p.HasMissingCost))
    {
      alerts.Add(new ProfitabilityAlertResponse(
        AlertType: "MissingCost",
        Message: $"El producto '{product.ProductName}' no tiene costo registrado. Los cálculos de ganancia son incorrectos.",
        BranchId: null,
        BranchName: null,
        ProductId: product.ProductId,
        ProductName: product.ProductName,
        EstimatedImpact: product.TotalSales));
    }
  }

  private static void AddNegativeMarginAlerts(
    IReadOnlyCollection<ProductProfitabilityResponse> products,
    List<ProfitabilityAlertResponse> alerts)
  {
    foreach (var product in products.Where(p => !p.HasMissingCost && p.MarginPercent < 0))
    {
      alerts.Add(new ProfitabilityAlertResponse(
        AlertType: "NegativeMargin",
        Message: $"El producto '{product.ProductName}' tiene margen negativo ({product.MarginPercent:F1}%). Se está vendiendo por debajo del costo.",
        BranchId: null,
        BranchName: null,
        ProductId: product.ProductId,
        ProductName: product.ProductName,
        EstimatedImpact: product.GrossProfit)); // negative number
    }
  }

  private static void AddHighVolumeLowMarginAlerts(
    IReadOnlyCollection<ProductProfitabilityResponse> products,
    List<ProfitabilityAlertResponse> alerts)
  {
    if (products.Count == 0)
    {
      return;
    }

    var sortedByQuantity = products
      .Where(p => !p.HasMissingCost && p.MarginPercent >= 0)
      .OrderByDescending(p => p.TotalQuantity)
      .ToArray();

    var topVolumeCount = Math.Max(1, (int)Math.Ceiling(sortedByQuantity.Length * (1 - HighVolumePercentile)));

    foreach (var product in sortedByQuantity
      .Take(topVolumeCount)
      .Where(product => ProfitabilityCalculator.IsLowMargin(product.MarginPercent, LowMarginThreshold)))
    {
      alerts.Add(new ProfitabilityAlertResponse(
        AlertType: "HighVolumeLowMargin",
        Message: $"El producto '{product.ProductName}' tiene alto volumen de ventas pero margen bajo ({product.MarginPercent:F1}%). Considera revisar el precio.",
        BranchId: null,
        BranchName: null,
        ProductId: product.ProductId,
        ProductName: product.ProductName,
        EstimatedImpact: product.TotalSales));
    }
  }

  private static void AddHighExpenseAlerts(
    IReadOnlyCollection<BranchProfitabilityResponse> branches,
    List<ProfitabilityAlertResponse> alerts)
  {
    foreach (var branch in branches.Where(b => b.OperatingExpenses > b.GrossProfit && b.GrossProfit > 0))
    {
      alerts.Add(new ProfitabilityAlertResponse(
        AlertType: "HighExpenses",
        Message: $"La sucursal '{branch.BranchName}' tiene gastos operativos (RD${branch.OperatingExpenses:N0}) mayores que la ganancia bruta (RD${branch.GrossProfit:N0}).",
        BranchId: branch.BranchId,
        BranchName: branch.BranchName,
        ProductId: null,
        ProductName: null,
        EstimatedImpact: branch.OperatingExpenses - branch.GrossProfit));
    }
  }

  private static int SortRank(string alertType) => alertType switch
  {
    "NegativeMargin" => 0,
    "MissingCost" => 1,
    "HighExpenses" => 2,
    _ => 3
  };
}
