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
using SaasCommerce.Modules.Inventory.Application.Transfers;
using SaasCommerce.Modules.Inventory.Contracts.Events.V1;
using SaasCommerce.Modules.Purchasing.Application.Purchases;
using SaasCommerce.Modules.Purchasing.Contracts.Events.V1;
using SaasCommerce.Modules.Notifications.Application.Handlers;
using SaasCommerce.Modules.Notifications.Domain;
using SaasCommerce.Modules.Sales.Application.CashRegisters;
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

public sealed class SaleReturnNotificationConsumer(
  IInboxStore inboxStore,
  IClock clock,
  IRealtimeNotifier realtime,
  ILogger<SaleReturnNotificationConsumer> logger)
  : IdempotentConsumer<CreditNoteGeneratedEventV1>(inboxStore, clock, logger)
{
  protected override Task ConsumeMessageAsync(ConsumeContext<CreditNoteGeneratedEventV1> context)
  {
    ArgumentNullException.ThrowIfNull(context);

    var message = context.Message;

    return realtime.NotifyBusinessAsync(
      message.BusinessId,
      SaleRealtimeEvents.ReturnChanged,
      new SaleReturnChangedNotificationV1(
        Guid.NewGuid(),
        message.CorrelationId,
        message.BusinessId,
        message.BranchId,
        message.SaleId,
        message.SaleReturnId,
        message.CreditNoteId,
        "Approved",
        message.Total,
        null,
        message.CreatedAt),
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

public sealed class PurchaseInventoryUpdatedRealtimeConsumer(
  IInboxStore inboxStore,
  IClock clock,
  IRealtimeNotifier realtime,
  ILogger<PurchaseInventoryUpdatedRealtimeConsumer> logger)
  : IdempotentConsumer<PurchaseInventoryUpdatedEventV1>(inboxStore, clock, logger)
{
  protected override Task ConsumeMessageAsync(ConsumeContext<PurchaseInventoryUpdatedEventV1> context)
  {
    ArgumentNullException.ThrowIfNull(context);

    return realtime.NotifyBusinessAsync(
      context.Message.BusinessId,
      PurchaseRealtimeEvents.Completed,
      context.Message,
      context.CancellationToken);
  }
}

public sealed class PurchaseFailedRealtimeConsumer(
  IInboxStore inboxStore,
  IClock clock,
  IRealtimeNotifier realtime,
  ILogger<PurchaseFailedRealtimeConsumer> logger)
  : IdempotentConsumer<PurchaseFailedEventV1>(inboxStore, clock, logger)
{
  protected override Task ConsumeMessageAsync(ConsumeContext<PurchaseFailedEventV1> context)
  {
    ArgumentNullException.ThrowIfNull(context);

    return realtime.NotifyBusinessAsync(
      context.Message.BusinessId,
      PurchaseRealtimeEvents.Failed,
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

public sealed class InventoryTransferCompletedRealtimeConsumer(
  IInboxStore inboxStore,
  IClock clock,
  IRealtimeNotifier realtime,
  ILogger<InventoryTransferCompletedRealtimeConsumer> logger)
  : IdempotentConsumer<InventoryTransferCompletedEventV1>(inboxStore, clock, logger)
{
  protected override async Task ConsumeMessageAsync(ConsumeContext<InventoryTransferCompletedEventV1> context)
  {
    ArgumentNullException.ThrowIfNull(context);

    var notification = new InventoryTransferStatusChangedNotificationV1(
      context.Message.TransferId,
      context.Message.BusinessId,
      context.Message.SourceBranchId,
      context.Message.TargetBranchId,
      "Completed",
      null,
      context.Message.CreatedAt);

    await realtime.NotifyBusinessAsync(
      context.Message.BusinessId,
      InventoryTransferRealtimeEvents.Completed,
      notification,
      context.CancellationToken);
    await realtime.NotifyBranchAsync(
      context.Message.SourceBranchId,
      InventoryTransferRealtimeEvents.Completed,
      notification,
      context.CancellationToken);
    await realtime.NotifyBranchAsync(
      context.Message.TargetBranchId,
      InventoryTransferRealtimeEvents.Completed,
      notification,
      context.CancellationToken);
  }
}

public sealed class InventoryTransferFailedRealtimeConsumer(
  IInboxStore inboxStore,
  IClock clock,
  IRealtimeNotifier realtime,
  ILogger<InventoryTransferFailedRealtimeConsumer> logger)
  : IdempotentConsumer<InventoryTransferFailedEventV1>(inboxStore, clock, logger)
{
  protected override Task ConsumeMessageAsync(ConsumeContext<InventoryTransferFailedEventV1> context)
  {
    ArgumentNullException.ThrowIfNull(context);

    return realtime.NotifyBusinessAsync(
      context.Message.BusinessId,
      InventoryTransferRealtimeEvents.Failed,
      new InventoryTransferStatusChangedNotificationV1(
        context.Message.TransferId,
        context.Message.BusinessId,
        context.Message.SourceBranchId,
        context.Message.TargetBranchId,
        "Failed",
        context.Message.Reason,
        context.Message.CreatedAt),
      context.CancellationToken);
  }
}

public sealed class InventoryTransferCancelledRealtimeConsumer(
  IInboxStore inboxStore,
  IClock clock,
  IRealtimeNotifier realtime,
  ILogger<InventoryTransferCancelledRealtimeConsumer> logger)
  : IdempotentConsumer<InventoryTransferCancelledEventV1>(inboxStore, clock, logger)
{
  protected override Task ConsumeMessageAsync(ConsumeContext<InventoryTransferCancelledEventV1> context)
  {
    ArgumentNullException.ThrowIfNull(context);

    return realtime.NotifyBusinessAsync(
      context.Message.BusinessId,
      InventoryTransferRealtimeEvents.Cancelled,
      new InventoryTransferStatusChangedNotificationV1(
        context.Message.TransferId,
        context.Message.BusinessId,
        context.Message.SourceBranchId,
        context.Message.TargetBranchId,
        "Cancelled",
        null,
        context.Message.CreatedAt),
      context.CancellationToken);
  }
}

public sealed class CashSessionOpenedRealtimeConsumer(
  IInboxStore inboxStore,
  IClock clock,
  IRealtimeNotifier realtime,
  ILogger<CashSessionOpenedRealtimeConsumer> logger)
  : IdempotentConsumer<CashSessionOpenedEventV1>(inboxStore, clock, logger)
{
  protected override async Task ConsumeMessageAsync(ConsumeContext<CashSessionOpenedEventV1> context)
  {
    ArgumentNullException.ThrowIfNull(context);

    await realtime.NotifyBusinessAsync(
      context.Message.BusinessId,
      "cash.session.opened",
      context.Message,
      context.CancellationToken);
    await realtime.NotifyBranchAsync(
      context.Message.BranchId,
      "cash.session.opened",
      context.Message,
      context.CancellationToken);
  }
}

public sealed class CashMovementRegisteredRealtimeConsumer(
  IInboxStore inboxStore,
  IClock clock,
  IRealtimeNotifier realtime,
  ILogger<CashMovementRegisteredRealtimeConsumer> logger)
  : IdempotentConsumer<CashMovementRegisteredEventV1>(inboxStore, clock, logger)
{
  protected override async Task ConsumeMessageAsync(ConsumeContext<CashMovementRegisteredEventV1> context)
  {
    ArgumentNullException.ThrowIfNull(context);

    await realtime.NotifyBusinessAsync(
      context.Message.BusinessId,
      "cash.movement.registered",
      context.Message,
      context.CancellationToken);
    await realtime.NotifyBranchAsync(
      context.Message.BranchId,
      "cash.movement.registered",
      context.Message,
      context.CancellationToken);
  }
}

public sealed class CashSessionClosedRealtimeConsumer(
  IInboxStore inboxStore,
  IClock clock,
  IRealtimeNotifier realtime,
  ILogger<CashSessionClosedRealtimeConsumer> logger)
  : IdempotentConsumer<CashSessionClosedEventV1>(inboxStore, clock, logger)
{
  protected override async Task ConsumeMessageAsync(ConsumeContext<CashSessionClosedEventV1> context)
  {
    ArgumentNullException.ThrowIfNull(context);

    await realtime.NotifyBusinessAsync(
      context.Message.BusinessId,
      "cash.session.closed",
      context.Message,
      context.CancellationToken);
    await realtime.NotifyBranchAsync(
      context.Message.BranchId,
      "cash.session.closed",
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

public sealed class OperatingExpenseCreatedRealtimeConsumer(
  IInboxStore inboxStore,
  IClock clock,
  IRealtimeNotifier realtime,
  ILogger<OperatingExpenseCreatedRealtimeConsumer> logger)
  : IdempotentConsumer<OperatingExpenseCreatedEventV1>(inboxStore, clock, logger)
{
  protected override async Task ConsumeMessageAsync(ConsumeContext<OperatingExpenseCreatedEventV1> context)
  {
    ArgumentNullException.ThrowIfNull(context);

    await realtime.NotifyBusinessAsync(
      context.Message.BusinessId,
      "expenses.created",
      context.Message,
      context.CancellationToken);
    await realtime.NotifyBranchAsync(
      context.Message.BranchId,
      "expenses.created",
      context.Message,
      context.CancellationToken);
  }
}

public sealed class OperatingExpensePaidRealtimeConsumer(
  IInboxStore inboxStore,
  IClock clock,
  IRealtimeNotifier realtime,
  ILogger<OperatingExpensePaidRealtimeConsumer> logger)
  : IdempotentConsumer<OperatingExpensePaidEventV1>(inboxStore, clock, logger)
{
  protected override async Task ConsumeMessageAsync(ConsumeContext<OperatingExpensePaidEventV1> context)
  {
    ArgumentNullException.ThrowIfNull(context);

    await realtime.NotifyBusinessAsync(
      context.Message.BusinessId,
      "expenses.paid",
      context.Message,
      context.CancellationToken);
    await realtime.NotifyBranchAsync(
      context.Message.BranchId,
      "expenses.paid",
      context.Message,
      context.CancellationToken);
  }
}

public sealed class OperatingExpenseCancelledRealtimeConsumer(
  IInboxStore inboxStore,
  IClock clock,
  IRealtimeNotifier realtime,
  ILogger<OperatingExpenseCancelledRealtimeConsumer> logger)
  : IdempotentConsumer<OperatingExpenseCancelledEventV1>(inboxStore, clock, logger)
{
  protected override async Task ConsumeMessageAsync(ConsumeContext<OperatingExpenseCancelledEventV1> context)
  {
    ArgumentNullException.ThrowIfNull(context);

    await realtime.NotifyBusinessAsync(
      context.Message.BusinessId,
      "expenses.cancelled",
      context.Message,
      context.CancellationToken);
    await realtime.NotifyBranchAsync(
      context.Message.BranchId,
      "expenses.cancelled",
      context.Message,
      context.CancellationToken);
  }
}

public sealed class CashRegisterOpenedRealtimeConsumer(
  IInboxStore inboxStore,
  IClock clock,
  IRealtimeNotifier realtime,
  ILogger<CashRegisterOpenedRealtimeConsumer> logger)
  : IdempotentConsumer<CashRegisterOpenedEventV1>(inboxStore, clock, logger)
{
  protected override Task ConsumeMessageAsync(ConsumeContext<CashRegisterOpenedEventV1> context)
  {
    ArgumentNullException.ThrowIfNull(context);

    return realtime.NotifyBusinessAsync(
      context.Message.BusinessId,
      "cashRegister.opened",
      context.Message,
      context.CancellationToken);
  }
}

public sealed class CashRegisterMovementRegisteredRealtimeConsumer(
  IInboxStore inboxStore,
  IClock clock,
  IRealtimeNotifier realtime,
  ILogger<CashRegisterMovementRegisteredRealtimeConsumer> logger)
  : IdempotentConsumer<CashRegisterMovementRegisteredEventV1>(inboxStore, clock, logger)
{
  protected override Task ConsumeMessageAsync(ConsumeContext<CashRegisterMovementRegisteredEventV1> context)
  {
    ArgumentNullException.ThrowIfNull(context);

    return realtime.NotifyBusinessAsync(
      context.Message.BusinessId,
      "cashRegister.movementRegistered",
      context.Message,
      context.CancellationToken);
  }
}

public sealed class CashRegisterClosedRealtimeConsumer(
  IInboxStore inboxStore,
  IClock clock,
  IRealtimeNotifier realtime,
  ILogger<CashRegisterClosedRealtimeConsumer> logger)
  : IdempotentConsumer<CashRegisterClosedEventV1>(inboxStore, clock, logger)
{
  protected override Task ConsumeMessageAsync(ConsumeContext<CashRegisterClosedEventV1> context)
  {
    ArgumentNullException.ThrowIfNull(context);

    return realtime.NotifyBusinessAsync(
      context.Message.BusinessId,
      "cashRegister.closed",
      context.Message,
      context.CancellationToken);
  }
}

public sealed class CashRegisterDifferenceNotificationConsumer(
  IInboxStore inboxStore,
  IClock clock,
  CreateNotificationHandler handler,
  IRealtimeNotifier realtime,
  ILogger<CashRegisterDifferenceNotificationConsumer> logger)
  : IdempotentConsumer<CashRegisterDifferenceDetectedEventV1>(inboxStore, clock, logger)
{
  private const decimal SignificantDifferenceThreshold = 50m;

  protected override async Task ConsumeMessageAsync(ConsumeContext<CashRegisterDifferenceDetectedEventV1> context)
  {
    ArgumentNullException.ThrowIfNull(context);

    var msg = context.Message;
    await realtime.NotifyBusinessAsync(
      msg.BusinessId,
      "cashRegister.differenceDetected",
      msg,
      context.CancellationToken);

    if (Math.Abs(msg.Difference) <= SignificantDifferenceThreshold)
    {
      return;
    }

    var severity = Math.Abs(msg.Difference) > 500m
      ? OperationalNotificationSeverity.Critical
      : OperationalNotificationSeverity.Warning;

    var direction = msg.Difference > 0 ? "sobrante" : "faltante";
    var amount = Math.Abs(msg.Difference);

    await handler.Handle(
      new CreateNotificationCommand(
        BusinessId: msg.BusinessId,
        BranchId: msg.BranchId,
        Type: OperationalNotificationType.CashDifference,
        Severity: severity,
        Title: "Diferencia en arqueo de caja",
        Message: $"Se detectó un {direction} de RD${amount:N2} al cerrar el arqueo de caja.",
        RelatedEntityId: msg.CashRegisterId,
        RelatedEntityType: "CashRegister"),
      context.CancellationToken);
  }
}
