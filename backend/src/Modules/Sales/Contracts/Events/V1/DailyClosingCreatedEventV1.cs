using SaasCommerce.BuildingBlocks.Contracts.Events;

namespace SaasCommerce.Modules.Sales.Contracts.Events.V1;

public sealed record DailyClosingCreatedEventV1(
  Guid EventId,
  Guid CorrelationId,
  Guid ClosingId,
  Guid BusinessId,
  Guid BranchId,
  Guid CreatedByUserId,
  string ClosingDate,
  decimal TotalSales,
  int AlertCount,
  DateTimeOffset OccurredAt,
  int Version = 1) : IIntegrationEvent;
