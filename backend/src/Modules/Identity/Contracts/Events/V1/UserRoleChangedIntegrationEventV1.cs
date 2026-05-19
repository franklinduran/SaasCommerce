using SaasCommerce.BuildingBlocks.Contracts.Events;

namespace SaasCommerce.Modules.Identity.Contracts.Events.V1;

public sealed record UserRoleChangedIntegrationEventV1(
  Guid EventId,
  Guid CorrelationId,
  Guid BusinessId,
  Guid UserId,
  string PreviousRole,
  string NewRole,
  DateTimeOffset OccurredAt,
  int Version = 1) : IIntegrationEvent;
