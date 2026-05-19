using SaasCommerce.BuildingBlocks.Contracts.Events;

namespace SaasCommerce.Modules.Identity.Contracts.Events.V1;

public sealed record UserActivatedIntegrationEventV1(
  Guid EventId,
  Guid CorrelationId,
  Guid BusinessId,
  Guid UserId,
  DateTimeOffset OccurredAt,
  int Version = 1) : IIntegrationEvent;
