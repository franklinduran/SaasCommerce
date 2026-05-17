using SaasCommerce.BuildingBlocks.Contracts.Events;

namespace SaasCommerce.Modules.Sales.Contracts.Events.V1;

public sealed record SaleCompletedIntegrationEventV1(
  Guid EventId,
  Guid CorrelationId,
  Guid BusinessId,
  Guid SaleId,
  Guid BranchId,
  Guid UserId,
  decimal Total,
  DateTimeOffset OccurredAt,
  int Version = 1) : IIntegrationEvent;
