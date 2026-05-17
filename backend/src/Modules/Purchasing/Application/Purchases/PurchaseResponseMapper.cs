using SaasCommerce.Modules.Catalog.Contracts.Purchasing;
using SaasCommerce.Modules.Purchasing.Contracts.Responses;
using SaasCommerce.Modules.Purchasing.Domain;

namespace SaasCommerce.Modules.Purchasing.Application.Purchases;

internal static class PurchaseResponseMapper
{
  public static PurchaseResponse ToResponse(
    Purchase purchase,
    string? supplierName,
    IReadOnlyDictionary<Guid, ProductPurchaseInfo> products,
    IReadOnlyCollection<PurchaseInventoryMovementResponse>? movements = null)
  {
    ArgumentNullException.ThrowIfNull(purchase);
    ArgumentNullException.ThrowIfNull(products);

    return new PurchaseResponse(
      purchase.Id,
      purchase.BusinessId.Value,
      purchase.BranchId.Value,
      purchase.SupplierId,
      supplierName ?? "Proveedor no disponible",
      purchase.UserId,
      purchase.Status.ToString(),
      purchase.SupplierInvoiceNumber,
      purchase.PurchaseDate,
      purchase.Notes,
      purchase.Total,
      purchase.Items.Select(item => ToItemResponse(item, products)).ToArray(),
      movements ?? [],
      purchase.CreatedAt,
      purchase.UpdatedAt,
      purchase.ReceivedAt,
      purchase.CancelledAt);
  }

  private static PurchaseItemResponse ToItemResponse(
    PurchaseItem item,
    IReadOnlyDictionary<Guid, ProductPurchaseInfo> products)
  {
    products.TryGetValue(item.ProductId, out var product);

    return new PurchaseItemResponse(
      item.Id,
      item.ProductId,
      product?.Name ?? "Producto no disponible",
      product?.Sku,
      item.Quantity,
      item.UnitCost,
      item.Subtotal);
  }
}
