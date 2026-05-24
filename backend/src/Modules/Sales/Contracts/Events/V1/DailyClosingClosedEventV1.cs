using SaasCommerce.BuildingBlocks.Contracts.Events;

namespace SaasCommerce.Modules.Sales.Contracts.Events.V1;

public sealed record DailyClosingClosedEventV1(
  Guid EventId,
  Guid CorrelationId,
  Guid ClosingId,
  Guid BusinessId,
  Guid BranchId,
  Guid ClosedByUserId,
  string ClosingDate,
  decimal TotalSales,
  decimal EstimatedNetProfit,
  decimal? CashDifference,
  DateTimeOffset OccurredAt,
  int Version = 1) : IIntegrationEvent;
