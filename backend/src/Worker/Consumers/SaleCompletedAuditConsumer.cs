using SaasCommerce.BuildingBlocks.Application.Abstractions.Audit;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Identity.Domain;
using SaasCommerce.Modules.Sales.Contracts.Events.V1;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Worker.Consumers;

public sealed class SaleCompletedAuditConsumer(
  IAuditLogWriter auditLogWriter,
  IInboxStore inboxStore,
  IClock clock,
  ILogger<SaleCompletedAuditConsumer> logger)
  : AuditingConsumer<SaleCompletedEventV1>(auditLogWriter, inboxStore, clock, logger)
{
  protected override AuditEntry BuildAuditEntry(SaleCompletedEventV1 message) =>
    new(
      new BusinessId(message.BusinessId),
      message.UserId,
      AuditActionType.SaleCompleted,
      AuditEntityType.Sale,
      message.SaleId,
      Description: $"Sale completed. Total: {message.Total:C2}",
      CorrelationId: message.CorrelationId);
}
