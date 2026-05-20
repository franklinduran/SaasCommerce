using SaasCommerce.BuildingBlocks.Application.Abstractions.Audit;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Identity.Domain;
using SaasCommerce.Modules.Inventory.Contracts.Events.V1;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Worker.Consumers;

public sealed class InventoryAdjustedAuditConsumer(
  IAuditLogWriter auditLogWriter,
  IInboxStore inboxStore,
  IClock clock,
  ILogger<InventoryAdjustedAuditConsumer> logger)
  : AuditingConsumer<InventoryAdjustedEventV1>(auditLogWriter, inboxStore, clock, logger)
{
  protected override AuditEntry BuildAuditEntry(InventoryAdjustedEventV1 message) =>
    new(
      new BusinessId(message.BusinessId),
      null,
      AuditActionType.InventoryAdjusted,
      AuditEntityType.Inventory,
      message.ProductId,
      Description: $"Inventory adjusted. Stock: {message.PreviousStock} → {message.NewStock}",
      CorrelationId: message.CorrelationId);
}
