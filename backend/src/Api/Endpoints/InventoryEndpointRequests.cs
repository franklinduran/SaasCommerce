namespace SaasCommerce.Api.Endpoints;

public sealed record InventoryStockEndpointRequest(
  string? Search,
  bool? LowStockOnly,
  string? ProductType,
  Guid? CategoryId,
  int? Page,
  int? PageSize,
  string? SortBy,
  string? SortDirection);

public sealed record InventoryMovementsEndpointRequest(
  Guid? ProductId,
  string? MovementType,
  DateTimeOffset? DateFrom,
  DateTimeOffset? DateTo,
  int? Page,
  int? PageSize,
  string? SortBy,
  string? SortDirection);
