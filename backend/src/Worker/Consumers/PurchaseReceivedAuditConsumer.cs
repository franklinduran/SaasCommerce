using SaasCommerce.BuildingBlocks.Application.Abstractions.Audit;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Identity.Domain;
using SaasCommerce.Modules.Purchasing.Contracts.Events.V1;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Worker.Consumers;

public sealed class PurchaseReceivedAuditConsumer(
  IAuditLogWriter auditLogWriter,
  IInboxStore inboxStore,
  IClock clock,
  ILogger<PurchaseReceivedAuditConsumer> logger)
  : AuditingConsumer<PurchaseReceivedEventV1>(auditLogWriter, inboxStore, clock, logger)
{
  protected override AuditEntry BuildAuditEntry(PurchaseReceivedEventV1 message) =>
    new(
      new BusinessId(message.BusinessId),
      message.UserId,
      AuditActionType.PurchaseReceived,
      AuditEntityType.Purchase,
      message.PurchaseId,
      Description: $"Purchase received from supplier {message.SupplierId}. Total: {message.Total:C2}",
      CorrelationId: message.CorrelationId);
}
