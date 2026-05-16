namespace SaasCommerce.Modules.Inventory.Application.Stock;

public sealed record GetInventoryMovementsQuery(
  Guid? ProductId,
  string? MovementType,
  DateTimeOffset? DateFrom,
  DateTimeOffset? DateTo,
  int Page,
  int PageSize,
  string? SortBy,
  string? SortDirection);
