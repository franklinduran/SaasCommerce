namespace SaasCommerce.Modules.Catalog.Contracts.Inventory;

public interface IInventoryProductLookupReader
{
  Task<IReadOnlyCollection<InventoryProductLookup>> SearchAsync(
    InventoryProductLookupQuery query,
    CancellationToken cancellationToken = default);
}

public sealed record InventoryProductLookupQuery(
  Guid BusinessId,
  string? Search,
  string? ProductType,
  Guid? CategoryId);
