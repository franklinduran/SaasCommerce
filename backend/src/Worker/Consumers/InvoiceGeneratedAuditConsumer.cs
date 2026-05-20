using SaasCommerce.BuildingBlocks.Application.Abstractions.Audit;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Billing.Contracts.Events.V1;
using SaasCommerce.Modules.Identity.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Worker.Consumers;

public sealed class InvoiceGeneratedAuditConsumer(
  IAuditLogWriter auditLogWriter,
  IInboxStore inboxStore,
  IClock clock,
  ILogger<InvoiceGeneratedAuditConsumer> logger)
  : AuditingConsumer<InvoiceGeneratedIntegrationEventV1>(auditLogWriter, inboxStore, clock, logger)
{
  protected override AuditEntry BuildAuditEntry(InvoiceGeneratedIntegrationEventV1 message) =>
    new(
      new BusinessId(message.BusinessId),
      message.UserId,
      AuditActionType.InvoiceGenerated,
      AuditEntityType.Invoice,
      message.InvoiceId,
      Description: $"Invoice generated for sale {message.SaleId}",
      CorrelationId: message.CorrelationId);
}
