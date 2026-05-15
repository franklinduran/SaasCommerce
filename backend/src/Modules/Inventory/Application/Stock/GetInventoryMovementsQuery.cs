namespace SaasCommerce.Modules.Inventory.Application.Stock;

public sealed record GetInventoryMovementsQuery(Guid? ProductId, int Page, int PageSize);
