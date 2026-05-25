using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Application.Sales;

public sealed record ExportSalesCsvQuery(
  DateTimeOffset? DateFrom,
  DateTimeOffset? DateTo);

public sealed class ExportSalesCsvHandler(
  ISaleReadRepository sales,
  ICurrentUserService currentUser)
{
  private static readonly IReadOnlyList<string> Headers =
  [
    "Código", "Fecha", "Sucursal", "Cliente", "Método Pago",
    "Total", "Estado", "Productos"
  ];

  public async Task<Result<byte[]>> Handle(
    ExportSalesCsvQuery query,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(query);

    if (currentUser.BusinessId is not Guid businessId)
    {
      return Result.Failure<byte[]>(new DomainError("EXPORT_USER_CONTEXT_REQUIRED",
        "Se requiere contexto de usuario autenticado para exportar."));
    }

    var items = await sales.ExportAllAsync(
      new BusinessId(businessId),
      query.DateFrom,
      query.DateTo,
      cancellationToken);

    var rows = items.Select(s => (IReadOnlyCollection<string?>)
    [
      s.Code,
      s.CreatedAt.ToString("yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture),
      s.BranchName,
      s.CustomerName,
      s.PaymentMethod,
      s.Total.ToString("F2"),
      s.Status,
      s.Items.Count.ToString()
    ]);

    var csv = CsvBuilder.Build(Headers, rows);
    return Result.Success(csv);
  }
}
