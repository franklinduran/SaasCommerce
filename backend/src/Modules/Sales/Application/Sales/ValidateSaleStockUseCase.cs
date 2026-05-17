using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Realtime;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Catalog.Contracts.Sales;
using SaasCommerce.Modules.Inventory.Contracts.Availability;
using SaasCommerce.Modules.Inventory.Contracts.Events.V1;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Contracts.Events.V1;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Application.Sales;

public sealed class ValidateSaleStockUseCase(
  ISaleRepository sales,
  IProductSalesPolicyReader productPolicies,
  IInventoryAvailabilityService inventoryAvailability,
  IOutboxWriter outbox,
  IRealtimeNotifier realtime,
  IClock clock,
  IUnitOfWork unitOfWork) : IValidateSaleStockUseCase
{
  public async Task<Result> ExecuteAsync(
    StockValidationRequestedEventV1 stockValidationRequested,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(stockValidationRequested);

    var sale = await sales.GetAsync(
      new BusinessId(stockValidationRequested.BusinessId),
      stockValidationRequested.SaleId,
      cancellationToken);

    if (sale is null)
    {
      await PublishFailedAsync(stockValidationRequested, "Sale was not found.", cancellationToken);
      return Result.Success();
    }

    if (sale.Status == SaleStatus.Received)
    {
      sale.MarkAsProcessing(clock.UtcNow);
      await realtime.NotifyBusinessAsync(
        stockValidationRequested.BusinessId,
        SaleRealtimeEvents.StatusChanged,
        new SaleStatusChangedNotificationV1(
          Guid.NewGuid(),
          stockValidationRequested.CorrelationId,
          stockValidationRequested.SaleId,
          stockValidationRequested.BusinessId,
          stockValidationRequested.BranchId,
          stockValidationRequested.UserId,
          SaleStatus.Processing.ToString(),
          null,
          clock.UtcNow),
        cancellationToken);
    }
    else if (sale.Status != SaleStatus.Processing)
    {
      await PublishFailedAsync(stockValidationRequested, "Sale is not in a processable state.", cancellationToken);
      return Result.Success();
    }

    var reason = await GetStockValidationFailureReasonAsync(
      stockValidationRequested,
      cancellationToken);

    if (reason is not null)
    {
      await PublishFailedAsync(stockValidationRequested, reason, cancellationToken);
      return Result.Success();
    }

    await outbox.AddAsync(
      new StockValidatedEventV1(
        Guid.NewGuid(),
        stockValidationRequested.CorrelationId,
        stockValidationRequested.SaleId,
        stockValidationRequested.BusinessId,
        stockValidationRequested.BranchId,
        stockValidationRequested.UserId,
        stockValidationRequested.Items,
        stockValidationRequested.Total,
        stockValidationRequested.PaymentMethod,
        clock.UtcNow),
      cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success();
  }

  private async Task<string?> GetStockValidationFailureReasonAsync(
    StockValidationRequestedEventV1 request,
    CancellationToken cancellationToken)
  {
    if (request.Items.Count == 0)
    {
      return "Sale has no items.";
    }

    foreach (var item in request.Items)
    {
      if (item.Quantity <= 0)
      {
        return $"Product {item.ProductId} has an invalid quantity.";
      }

      var productPolicy = await productPolicies.GetSalesPolicyAsync(
        request.BusinessId,
        item.ProductId,
        cancellationToken);

      if (productPolicy is null)
      {
        return $"Product {item.ProductId} was not found.";
      }

      if (!productPolicy.CanBeSold)
      {
        return productPolicy.ReasonIfCannotBeSold ?? $"Product {item.ProductId} cannot be sold.";
      }

      var availability = await inventoryAvailability.ValidateStockAsync(
        new InventoryAvailabilityRequest(
          request.BusinessId,
          request.BranchId,
          item.ProductId,
          item.Quantity,
          productPolicy.TrackInventory,
          productPolicy.AllowNegativeStock),
        cancellationToken);

      if (!availability.IsAvailable)
      {
        return availability.ReasonIfUnavailable ?? $"Product {item.ProductId} does not have enough stock.";
      }
    }

    return null;
  }

  private async Task PublishFailedAsync(
    StockValidationRequestedEventV1 request,
    string reason,
    CancellationToken cancellationToken)
  {
    await outbox.AddAsync(
      new StockValidationFailedEventV1(
        Guid.NewGuid(),
        request.CorrelationId,
        request.SaleId,
        request.BusinessId,
        request.BranchId,
        request.UserId,
        reason,
        clock.UtcNow),
      cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken);
  }
}
