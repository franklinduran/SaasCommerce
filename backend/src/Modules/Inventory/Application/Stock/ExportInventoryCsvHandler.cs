using System.Globalization;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Inventory.Application.Abstractions;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Inventory.Application.Stock;

public sealed record ExportInventoryCsvQuery;

public sealed class ExportInventoryCsvHandler(
  IInventoryReadRepository inventory,
  ICurrentUserService currentUser)
{
  private static readonly IReadOnlyList<string> Headers =
  [
    "SKU", "Producto", "Sucursal", "Cantidad", "Unidad Medida",
    "Stock Mínimo", "Punto Reorden", "Estado", "Última Actualización"
  ];

  public async Task<Result<byte[]>> Handle(
    ExportInventoryCsvQuery query,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(query);

    if (currentUser.BusinessId is not Guid businessId)
    {
      return Result.Failure<byte[]>(new DomainError("EXPORT_USER_CONTEXT_REQUIRED",
        "Se requiere contexto de usuario autenticado para exportar."));
    }

    var items = await inventory.ExportAllAsync(new BusinessId(businessId), cancellationToken);

    var rows = items.Select(s => (IReadOnlyCollection<string?>)
    [
      s.Sku,
      s.ProductName,
      s.BranchName,
      s.Quantity.ToString("F2", CultureInfo.InvariantCulture),
      s.UnitOfMeasure,
      s.MinimumStock?.ToString("F2", CultureInfo.InvariantCulture),
      s.ReorderPoint?.ToString("F2", CultureInfo.InvariantCulture),
      s.Status,
      s.LastUpdatedAt?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)
    ]);

    var csv = CsvBuilder.Build(Headers, rows);
    return Result.Success(csv);
  }
}
