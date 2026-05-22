using SaasCommerce.Modules.Inventory.Contracts.Responses;
using SaasCommerce.Modules.Inventory.Domain;

namespace SaasCommerce.Modules.Inventory.Application.Transfers;

public static class InventoryTransferResponseMapper
{
  public static InventoryTransferResponse ToResponse(InventoryTransfer transfer)
  {
    ArgumentNullException.ThrowIfNull(transfer);

    return new InventoryTransferResponse(
      transfer.Id,
      transfer.BusinessId.Value,
      transfer.SourceBranchId.Value,
      transfer.TargetBranchId.Value,
      transfer.Status.ToString(),
      transfer.Note,
      transfer.FailureReason,
      transfer.Items
        .Select(item => new InventoryTransferItemResponse(item.ProductId, item.Quantity))
        .ToArray(),
      transfer.CreatedAt,
      transfer.UpdatedAt);
  }
}
