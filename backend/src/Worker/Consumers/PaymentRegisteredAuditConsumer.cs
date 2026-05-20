using SaasCommerce.BuildingBlocks.Application.Abstractions.Audit;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Identity.Domain;
using SaasCommerce.Modules.Payments.Contracts.Events.V1;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Worker.Consumers;

public sealed class PaymentRegisteredAuditConsumer(
  IAuditLogWriter auditLogWriter,
  IInboxStore inboxStore,
  IClock clock,
  ILogger<PaymentRegisteredAuditConsumer> logger)
  : AuditingConsumer<PaymentRegisteredEventV1>(auditLogWriter, inboxStore, clock, logger)
{
  protected override AuditEntry BuildAuditEntry(PaymentRegisteredEventV1 message) =>
    new(
      new BusinessId(message.BusinessId),
      message.UserId,
      AuditActionType.PaymentRegistered,
      AuditEntityType.Payment,
      message.PaymentId,
      Description: $"Payment registered. Amount: {message.Amount:C2} via {message.PaymentMethod}",
      CorrelationId: message.CorrelationId);
}
