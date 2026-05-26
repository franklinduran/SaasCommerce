using System.Globalization;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Catalog.Application.Abstractions;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Catalog.Application.Export;

public sealed record ExportProductsCsvQuery;

public sealed class ExportProductsCsvHandler(
  ICatalogProductRepository products,
  ICurrentUserService currentUser)
{
  private static readonly IReadOnlyList<string> Headers =
  [
    "SKU", "Nombre", "Tipo", "Precio Venta", "Precio Costo", "Precio Mayorista",
    "ITBIS%", "Unidad Medida", "Margen%", "Control Inventario",
    "Stock Mínimo", "Punto Reorden", "Activo", "Creado"
  ];

  public async Task<Result<byte[]>> Handle(
    ExportProductsCsvQuery query,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(query);

    if (currentUser.BusinessId is not Guid businessId)
    {
      return Result.Failure<byte[]>(new DomainError("EXPORT_USER_CONTEXT_REQUIRED",
        "Se requiere contexto de usuario autenticado para exportar."));
    }

    var items = await products.ExportAllAsync(new BusinessId(businessId), cancellationToken);

    var rows = items.Select(p => (IReadOnlyCollection<string?>)
    [
      p.Sku,
      p.Name,
      p.ProductType.ToString(),
      p.SalePrice.ToString("F2", CultureInfo.InvariantCulture),
      p.CostPrice.ToString("F2", CultureInfo.InvariantCulture),
      p.WholesalePrice?.ToString("F2", CultureInfo.InvariantCulture),
      p.TaxRate.ToString("F2", CultureInfo.InvariantCulture),
      p.UnitOfMeasure.ToString(),
      p.ProfitMargin?.ToString("F2", CultureInfo.InvariantCulture),
      p.TrackInventory ? "Sí" : "No",
      p.MinimumStock?.ToString("F2", CultureInfo.InvariantCulture),
      p.ReorderPoint?.ToString("F2", CultureInfo.InvariantCulture),
      p.IsActive ? "Sí" : "No",
      p.CreatedAt.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)
    ]);

    var csv = CsvBuilder.Build(Headers, rows);
    return Result.Success(csv);
  }
}
