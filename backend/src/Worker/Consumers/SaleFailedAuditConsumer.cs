using SaasCommerce.BuildingBlocks.Application.Abstractions.Audit;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Identity.Domain;
using SaasCommerce.Modules.Sales.Contracts.Events.V1;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Worker.Consumers;

public sealed class SaleFailedAuditConsumer(
  IAuditLogWriter auditLogWriter,
  IInboxStore inboxStore,
  IClock clock,
  ILogger<SaleFailedAuditConsumer> logger)
  : AuditingConsumer<SaleFailedEventV1>(auditLogWriter, inboxStore, clock, logger)
{
  protected override AuditEntry BuildAuditEntry(SaleFailedEventV1 message) =>
    new(
      new BusinessId(message.BusinessId),
      message.UserId,
      AuditActionType.SaleFailed,
      AuditEntityType.Sale,
      message.SaleId,
      Description: $"Sale failed. Reason: {message.Reason}",
      CorrelationId: message.CorrelationId);
}
