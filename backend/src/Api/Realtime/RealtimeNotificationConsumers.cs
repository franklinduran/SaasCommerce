using MassTransit;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Realtime;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Infrastructure.Messaging;
using SaasCommerce.Modules.Billing.Application.Invoices;
using SaasCommerce.Modules.Billing.Contracts.Events.V1;
using SaasCommerce.Modules.Customers.Application.Credits;
using SaasCommerce.Modules.Customers.Contracts.Events.V1;
using SaasCommerce.Modules.Inventory.Application.Stock;
using SaasCommerce.Modules.Inventory.Contracts.Events.V1;
using SaasCommerce.Modules.Purchasing.Application.Purchases;
using SaasCommerce.Modules.Purchasing.Contracts.Events.V1;
using SaasCommerce.Modules.Sales.Application.Sales;
using SaasCommerce.Modules.Sales.Contracts.Events.V1;

namespace SaasCommerce.Api.Realtime;

public sealed class SaleCompletedRealtimeConsumer(
  IInboxStore inboxStore,
  IClock clock,
  IRealtimeNotifier realtime,
  ILogger<SaleCompletedRealtimeConsumer> logger)
  : IdempotentConsumer<SaleCompletedEventV1>(inboxStore, clock, logger)
{
  protected override Task ConsumeMessageAsync(ConsumeContext<SaleCompletedEventV1> context)
  {
    ArgumentNullException.ThrowIfNull(context);

    return realtime.NotifyBusinessAsync(
      context.Message.BusinessId,
      SaleRealtimeEvents.StatusChanged,
      new SaleStatusChangedNotificationV1(
        Guid.NewGuid(),
        context.Message.CorrelationId,
        context.Message.SaleId,
        context.Message.BusinessId,
        context.Message.BranchId,
        context.Message.UserId,
        "Completed",
        null,
        context.Message.CreatedAt),
      context.CancellationToken);
  }
}

public sealed class SaleFailedRealtimeConsumer(
  IInboxStore inboxStore,
  IClock clock,
  IRealtimeNotifier realtime,
  ILogger<SaleFailedRealtimeConsumer> logger)
  : IdempotentConsumer<SaleFailedEventV1>(inboxStore, clock, logger)
{
  protected override Task ConsumeMessageAsync(ConsumeContext<SaleFailedEventV1> context)
  {
    ArgumentNullException.ThrowIfNull(context);

    return realtime.NotifyBusinessAsync(
      context.Message.BusinessId,
      SaleRealtimeEvents.StatusChanged,
      new SaleStatusChangedNotificationV1(
        Guid.NewGuid(),
        context.Message.CorrelationId,
        context.Message.SaleId,
        context.Message.BusinessId,
        context.Message.BranchId,
        context.Message.UserId,
        "Failed",
        context.Message.Reason,
        context.Message.CreatedAt),
      context.CancellationToken);
  }
}

public sealed class CustomerCreditDebitedRealtimeConsumer(
  IInboxStore inboxStore,
  IClock clock,
  IRealtimeNotifier realtime,
  ILogger<CustomerCreditDebitedRealtimeConsumer> logger)
  : IdempotentConsumer<CustomerCreditDebitedEventV1>(inboxStore, clock, logger)
{
  protected override Task ConsumeMessageAsync(ConsumeContext<CustomerCreditDebitedEventV1> context)
  {
    ArgumentNullException.ThrowIfNull(context);

    return realtime.NotifyBusinessAsync(
      context.Message.BusinessId,
      CustomerCreditRealtimeEvents.CreditDebited,
      new CustomerCreditDebitedNotificationV1(
        Guid.NewGuid(),
        context.Message.CorrelationId,
        context.Message.BusinessId,
        context.Message.CustomerId,
        context.Message.SaleId,
        context.Message.Amount,
        context.Message.NewBalance,
        context.Message.CreatedAt),
      context.CancellationToken);
  }
}

public sealed class CustomerPaymentRegisteredRealtimeConsumer(
  IInboxStore inboxStore,
  IClock clock,
  IRealtimeNotifier realtime,
  ILogger<CustomerPaymentRegisteredRealtimeConsumer> logger)
  : IdempotentConsumer<CustomerPaymentRegisteredEventV1>(inboxStore, clock, logger)
{
  protected override Task ConsumeMessageAsync(ConsumeContext<CustomerPaymentRegisteredEventV1> context)
  {
    ArgumentNullException.ThrowIfNull(context);

    return realtime.NotifyBusinessAsync(
      context.Message.BusinessId,
      CustomerCreditRealtimeEvents.PaymentRegistered,
      new CustomerPaymentRegisteredNotificationV1(
        Guid.NewGuid(),
        context.Message.CorrelationId,
        context.Message.BusinessId,
        context.Message.CustomerId,
        context.Message.PaymentId,
        context.Message.Amount,
        context.Message.NewBalance,
        context.Message.CreatedAt),
      context.CancellationToken);
  }
}

public sealed class InvoiceGeneratedRealtimeConsumer(
  IInboxStore inboxStore,
  IClock clock,
  IRealtimeNotifier realtime,
  ILogger<InvoiceGeneratedRealtimeConsumer> logger)
  : IdempotentConsumer<InvoiceGeneratedEventV1>(inboxStore, clock, logger)
{
  protected override Task ConsumeMessageAsync(ConsumeContext<InvoiceGeneratedEventV1> context)
  {
    ArgumentNullException.ThrowIfNull(context);

    return realtime.NotifyBusinessAsync(
      context.Message.BusinessId,
      InvoiceRealtimeEvents.Generated,
      context.Message,
      context.CancellationToken);
  }
}

public sealed class InvoiceCancelledRealtimeConsumer(
  IInboxStore inboxStore,
  IClock clock,
  IRealtimeNotifier realtime,
  ILogger<InvoiceCancelledRealtimeConsumer> logger)
  : IdempotentConsumer<InvoiceCancelledEventV1>(inboxStore, clock, logger)
{
  protected override Task ConsumeMessageAsync(ConsumeContext<InvoiceCancelledEventV1> context)
  {
    ArgumentNullException.ThrowIfNull(context);

    return realtime.NotifyBusinessAsync(
      context.Message.BusinessId,
      InvoiceRealtimeEvents.Cancelled,
      context.Message,
      context.CancellationToken);
  }
}

public sealed class InventoryAdjustedRealtimeConsumer(
  IInboxStore inboxStore,
  IClock clock,
  IRealtimeNotifier realtime,
  ILogger<InventoryAdjustedRealtimeConsumer> logger)
  : IdempotentConsumer<InventoryAdjustedEventV1>(inboxStore, clock, logger)
{
  protected override async Task ConsumeMessageAsync(ConsumeContext<InventoryAdjustedEventV1> context)
  {
    ArgumentNullException.ThrowIfNull(context);

    await realtime.NotifyBusinessAsync(
      context.Message.BusinessId,
      InventoryRealtimeEvents.Adjusted,
      context.Message,
      context.CancellationToken);
    await realtime.NotifyBusinessAsync(
      context.Message.BusinessId,
      InventoryRealtimeEvents.StockChanged,
      new InventoryStockChangedNotificationV1(
        Guid.NewGuid(),
        context.Message.CorrelationId,
        context.Message.BusinessId,
        context.Message.BranchId,
        context.Message.ProductId,
        null,
        "ManualAdjustment",
        context.Message.CreatedAt),
      context.CancellationToken);
  }
}

public sealed class InventoryDeductedRealtimeConsumer(
  IInboxStore inboxStore,
  IClock clock,
  IRealtimeNotifier realtime,
  ILogger<InventoryDeductedRealtimeConsumer> logger)
  : IdempotentConsumer<InventoryDeductedEventV1>(inboxStore, clock, logger)
{
  protected override Task ConsumeMessageAsync(ConsumeContext<InventoryDeductedEventV1> context)
  {
    ArgumentNullException.ThrowIfNull(context);

    return realtime.NotifyBusinessAsync(
      context.Message.BusinessId,
      InventoryRealtimeEvents.StockChanged,
      new InventoryStockChangedNotificationV1(
        Guid.NewGuid(),
        context.Message.CorrelationId,
        context.Message.BusinessId,
        context.Message.BranchId,
        null,
        context.Message.SaleId,
        "SaleDeduction",
        context.Message.CreatedAt),
      context.CancellationToken);
  }
}

public sealed class LowStockDetectedRealtimeConsumer(
  IInboxStore inboxStore,
  IClock clock,
  IRealtimeNotifier realtime,
  ILogger<LowStockDetectedRealtimeConsumer> logger)
  : IdempotentConsumer<LowStockDetectedEventV1>(inboxStore, clock, logger)
{
  protected override Task ConsumeMessageAsync(ConsumeContext<LowStockDetectedEventV1> context)
  {
    ArgumentNullException.ThrowIfNull(context);

    return realtime.NotifyBusinessAsync(
      context.Message.BusinessId,
      InventoryRealtimeEvents.LowStockDetected,
      new LowStockDetectedNotificationV1(
        Guid.NewGuid(),
        context.Message.CorrelationId,
        context.Message.BusinessId,
        context.Message.BranchId,
        context.Message.ProductId,
        context.Message.ProductName,
        context.Message.CurrentStock,
        context.Message.MinimumStock,
        context.Message.CreatedAt),
      context.CancellationToken);
  }
}

public sealed class PurchaseReceivedRealtimeConsumer(
  IInboxStore inboxStore,
  IClock clock,
  IRealtimeNotifier realtime,
  ILogger<PurchaseReceivedRealtimeConsumer> logger)
  : IdempotentConsumer<PurchaseReceivedEventV1>(inboxStore, clock, logger)
{
  protected override async Task ConsumeMessageAsync(ConsumeContext<PurchaseReceivedEventV1> context)
  {
    ArgumentNullException.ThrowIfNull(context);

    await realtime.NotifyBusinessAsync(
      context.Message.BusinessId,
      PurchaseRealtimeEvents.Received,
      context.Message,
      context.CancellationToken);
    await realtime.NotifyBranchAsync(
      context.Message.BranchId,
      PurchaseRealtimeEvents.Received,
      context.Message,
      context.CancellationToken);
  }
}

public sealed class InventoryIncreasedRealtimeConsumer(
  IInboxStore inboxStore,
  IClock clock,
  IRealtimeNotifier realtime,
  ILogger<InventoryIncreasedRealtimeConsumer> logger)
  : IdempotentConsumer<InventoryIncreasedEventV1>(inboxStore, clock, logger)
{
  protected override async Task ConsumeMessageAsync(ConsumeContext<InventoryIncreasedEventV1> context)
  {
    ArgumentNullException.ThrowIfNull(context);

    await realtime.NotifyBusinessAsync(
      context.Message.BusinessId,
      PurchaseRealtimeEvents.InventoryUpdated,
      context.Message,
      context.CancellationToken);
    await realtime.NotifyBranchAsync(
      context.Message.BranchId,
      PurchaseRealtimeEvents.InventoryUpdated,
      context.Message,
      context.CancellationToken);
  }
}

public sealed class ProductCostUpdatedRealtimeConsumer(
  IInboxStore inboxStore,
  IClock clock,
  IRealtimeNotifier realtime,
  ILogger<ProductCostUpdatedRealtimeConsumer> logger)
  : IdempotentConsumer<ProductCostUpdatedEventV1>(inboxStore, clock, logger)
{
  protected override async Task ConsumeMessageAsync(ConsumeContext<ProductCostUpdatedEventV1> context)
  {
    ArgumentNullException.ThrowIfNull(context);

    await realtime.NotifyBusinessAsync(
      context.Message.BusinessId,
      PurchaseRealtimeEvents.ProductCostUpdated,
      context.Message,
      context.CancellationToken);
    await realtime.NotifyBranchAsync(
      context.Message.BranchId,
      PurchaseRealtimeEvents.ProductCostUpdated,
      context.Message,
      context.CancellationToken);
  }
}
