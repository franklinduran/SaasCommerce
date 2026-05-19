using SaasCommerce.BuildingBlocks.Contracts.Events;

namespace SaasCommerce.Modules.Identity.Contracts.Events.V1;

public sealed record UserCreatedIntegrationEventV1(
  Guid EventId,
  Guid CorrelationId,
  Guid BusinessId,
  Guid UserId,
  string FullName,
  string Email,
  string Role,
  DateTimeOffset OccurredAt,
  int Version = 1) : IIntegrationEvent;
