using SaasCommerce.BuildingBlocks.Contracts.Events;

namespace SaasCommerce.Modules.Sales.Contracts.Events.V1;

public sealed record CashRegisterClosedEventV1(
  Guid EventId,
  Guid CorrelationId,
  Guid CashRegisterId,
  Guid BusinessId,
  Guid BranchId,
  Guid UserId,
  decimal OpeningAmount,
  decimal CashSales,
  decimal CardSales,
  decimal TransferSales,
  decimal CreditSales,
  decimal CashReturns,
  decimal ManualCashIn,
  decimal ManualCashOut,
  decimal ExpectedCashAmount,
  decimal CountedAmount,
  decimal Difference,
  string DifferenceType,
  DateTimeOffset ClosedAt,
  int Version = 1) : IIntegrationEvent
{
  public DateTimeOffset OccurredAt => ClosedAt;
  public DateTimeOffset CreatedAt => ClosedAt;
}
