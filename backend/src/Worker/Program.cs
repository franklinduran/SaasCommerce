using System.Globalization;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using SaasCommerce.BuildingBlocks;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.BuildingBlocks.Infrastructure.Messaging.Outbox;
using SaasCommerce.BuildingBlocks.Infrastructure.Messaging.Sagas.Sales;
using SaasCommerce.Modules;
using SaasCommerce.Worker;
using SaasCommerce.Worker.Consumers;
using SaasCommerce.Worker.Sagas.Sales;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSerilog((_, loggerConfiguration) =>
  loggerConfiguration
    .Enrich.FromLogContext()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture));
builder.Services.AddModules();
builder.Services.AddBuildingBlocks(
  builder.Configuration,
  massTransit =>
  {
    massTransit.AddConsumer<TechnicalPingConsumer>();
    massTransit.AddConsumer<SaleCreatedConsumer>();
    massTransit.AddConsumer<StockValidationRequestedConsumer>();
    massTransit.AddConsumer<InventoryDeductionRequestedConsumer>();
    massTransit.AddConsumer<PaymentRegistrationRequestedConsumer>();
    massTransit.AddConsumer<InvoiceGenerationRequestedConsumer>();
    massTransit.AddConsumer<SaleStatusChangedConsumer>();
    massTransit.AddConsumer<PurchaseReceivedConsumer>();
    massTransit.AddConsumer<RegisterCreditSaleConsumer>();
    massTransit.AddConsumer<SaleReturnRequestedConsumer>();
    massTransit.AddConsumer<RestoreInventoryFromSaleReturnConsumer>();
    massTransit.AddConsumer<GenerateCreditNoteForReturnConsumer>();
    massTransit.AddConsumer<CustomerCreditDebitedNotificationConsumer>();
    massTransit.AddConsumer<CustomerPaymentRegisteredNotificationConsumer>();
    massTransit.AddConsumer<ValidateStockConsumer>();
    massTransit.AddConsumer<DeductInventoryConsumer>();
    massTransit.AddConsumer<RegisterPaymentConsumer>();
    massTransit.AddConsumer<GenerateInvoiceConsumer>();
    massTransit.AddConsumer<SaleCompletedConsumer>();
    massTransit.AddConsumer<SaleFailedConsumer>();
    massTransit.AddConsumer<InventoryAdjustedConsumer>();
    massTransit.AddConsumer<InventoryDeductedConsumer>();
    massTransit.AddConsumer<LowStockDetectedConsumer>();
    massTransit.AddConsumer<ProcessInventoryTransferConsumer>();
    massTransit.AddConsumer<SaleCompletedAuditConsumer>();
    massTransit.AddConsumer<SaleFailedAuditConsumer>();
    massTransit.AddConsumer<PaymentRegisteredAuditConsumer>();
    massTransit.AddConsumer<InvoiceGeneratedAuditConsumer>();
    massTransit.AddConsumer<PurchaseReceivedAuditConsumer>();
    massTransit.AddConsumer<InventoryAdjustedAuditConsumer>();
    massTransit.AddConsumer<DailyClosingCreatedConsumer>();
    massTransit.AddConsumer<DailyClosingClosedConsumer>();
    massTransit.AddConsumer<CashRegisterClosedConsumer>();
    massTransit.AddConsumer<CashRegisterNotificationConsumer>();
    // Notification consumers (persist to DB)
    massTransit.AddConsumer<LowStockNotificationConsumer>();
    massTransit.AddConsumer<SaleFailedNotificationConsumer>();
    massTransit.AddConsumer<InvoiceFailedNotificationConsumer>();
    massTransit.AddConsumer<CashSessionClosedNotificationConsumer>();
    massTransit.AddConsumer<DailyClosingClosedNotificationConsumer>();
    massTransit.AddSagaStateMachine<SaleStateMachine, SaleSagaState>()
      .EntityFrameworkRepository(repository =>
      {
        repository.ConcurrencyMode = ConcurrencyMode.Pessimistic;
        repository.ExistingDbContext<AppDbContext>();
        // serializable:false → uses RepeatableRead instead of Serializable.
        // With SELECT FOR UPDATE (pessimistic lock), Serializable adds no protection
        // but causes 40001 errors when multiple saga events arrive simultaneously.
        repository.LockStatementProvider = new PostgresRepeatableReadLockProvider();
      });
  });
builder.Services.AddHostedService<OutboxNotificationHostedService>();
builder.Services.AddHostedService<OutboxPublisherHostedService>();
builder.Services.AddHostedService<StuckSaleWatchdogService>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
await host.RunAsync();

public partial class Program
{
  protected Program()
  {
  }
}

// Saga uses SELECT FOR UPDATE (pessimistic lock), so Serializable isolation is redundant
// and causes 40001 errors when multiple saga events arrive concurrently.
// RepeatableRead + FOR UPDATE is sufficient and avoids serialization conflicts.
internal sealed class PostgresRepeatableReadLockProvider : SqlLockStatementProvider
{
  public PostgresRepeatableReadLockProvider()
    : base(string.Empty, new PostgresLockStatementFormatter(), false)
  {
  }
}
