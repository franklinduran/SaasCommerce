using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Application.DailyClosings;

public sealed record ExportDailyClosingsCsvQuery(
  DateOnly? DateFrom,
  DateOnly? DateTo);

public sealed class ExportDailyClosingsCsvHandler(
  IDailyClosingReadRepository dailyClosings,
  ICurrentUserService currentUser)
{
  private static readonly IReadOnlyList<string> Headers =
  [
    "Fecha", "Sucursal", "Estado", "Total Ventas",
    "Utilidad Estimada", "Margen%", "Alertas"
  ];

  public async Task<Result<byte[]>> Handle(
    ExportDailyClosingsCsvQuery query,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(query);

    if (currentUser.BusinessId is not Guid businessId)
    {
      return Result.Failure<byte[]>(new DomainError("EXPORT_USER_CONTEXT_REQUIRED",
        "Se requiere contexto de usuario autenticado para exportar."));
    }

    var items = await dailyClosings.ExportAllAsync(
      new BusinessId(businessId),
      query.DateFrom,
      query.DateTo,
      cancellationToken);

    var rows = items.Select(dc => (IReadOnlyCollection<string?>)
    [
      dc.ClosingDate,
      dc.BranchName,
      dc.Status,
      dc.TotalSales.ToString("F2"),
      dc.EstimatedNetProfit.ToString("F2"),
      dc.NetMarginPercent.ToString("F2"),
      dc.AlertCount.ToString()
    ]);

    var csv = CsvBuilder.Build(Headers, rows);
    return Result.Success(csv);
  }
}
