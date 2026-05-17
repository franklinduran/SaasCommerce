using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Realtime;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Sales.Contracts.Events.V1;
using SaasCommerce.Modules.Sales.Domain;

namespace SaasCommerce.Modules.Sales.Application.Sales;

public sealed class SaleEventWriter(
  IOutboxWriter outbox,
  IRealtimeNotifier realtime,
  IClock clock) : ISaleEventWriter
{
  public Task AddAsync(
    SaleCreatedEventV1 saleCreated,
    CancellationToken cancellationToken = default)
    => outbox.AddAsync(saleCreated, cancellationToken);

  public async Task<SaleCreatedEventV1> AddSaleCreatedAsync(
    Sale sale,
    Guid businessId,
    Guid branchId,
    Guid userId,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(sale);

    var occurredAt = clock.UtcNow;
    var saleCreated = new SaleCreatedEventV1(
      Guid.NewGuid(),
      Guid.NewGuid(),
      sale.Id,
      businessId,
      branchId,
      userId,
      sale.Items
        .Select(item => new SaleItemV1(item.ProductId, item.Quantity, item.UnitPrice))
        .ToArray(),
      sale.Total,
      sale.PaymentMethod,
      occurredAt);

    await AddAsync(saleCreated, cancellationToken);

    return saleCreated;
  }

  public Task NotifyStatusChangedAsync(
    SaleCreatedEventV1 saleCreated,
    SaleStatus status,
    string? reason,
    CancellationToken cancellationToken = default)
    => realtime.NotifyBusinessAsync(
      saleCreated.BusinessId,
      SaleRealtimeEvents.StatusChanged,
      new SaleStatusChangedNotificationV1(
        Guid.NewGuid(),
        saleCreated.CorrelationId,
        saleCreated.SaleId,
        saleCreated.BusinessId,
        saleCreated.BranchId,
        saleCreated.UserId,
        status.ToString(),
        reason,
        clock.UtcNow),
      cancellationToken);
}
