namespace SaasCommerce.Modules.Catalog.Contracts.Purchasing;

public interface IProductPurchaseReader
{
  Task<ProductPurchaseInfo?> GetAsync(
    Guid businessId,
    Guid productId,
    CancellationToken cancellationToken = default);

  Task<IReadOnlyDictionary<Guid, ProductPurchaseInfo>> ListAsync(
    Guid businessId,
    IReadOnlyCollection<Guid> productIds,
    CancellationToken cancellationToken = default);

  Task<ProductCostUpdate?> UpdateAverageCostAsync(
    Guid businessId,
    Guid productId,
    decimal currentStock,
    decimal purchasedQuantity,
    decimal unitCost,
    DateTimeOffset updatedAt,
    CancellationToken cancellationToken = default);
}

public sealed record ProductPurchaseInfo(
  Guid ProductId,
  Guid BusinessId,
  string Name,
  string Sku,
  bool TrackInventory,
  bool AllowNegativeStock,
  decimal CostPrice,
  bool IsActive);

public sealed record ProductCostUpdate(
  Guid ProductId,
  decimal PreviousCost,
  decimal NewCost);
