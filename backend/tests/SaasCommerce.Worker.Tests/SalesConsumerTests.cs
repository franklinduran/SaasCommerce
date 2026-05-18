#pragma warning disable CA1707

using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Realtime;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Contracts.Events;
using SaasCommerce.Modules.Billing.Application.Invoices;
using SaasCommerce.Modules.Billing.Contracts.Events.V1;
using SaasCommerce.Modules.Billing.Contracts.Responses;
using SaasCommerce.Modules.Customers.Application.Credits;
using SaasCommerce.Modules.Customers.Contracts.Events.V1;
using SaasCommerce.Modules.Inventory.Contracts.Events.V1;
using SaasCommerce.Modules.Payments.Contracts.Events.V1;
using SaasCommerce.Modules.Sales.Application.Sales;
using SaasCommerce.Modules.Sales.Contracts.Events.V1;
using SaasCommerce.SharedKernel;
using SaasCommerce.Worker.Consumers;

namespace SaasCommerce.Worker.Tests;

public sealed class SalesConsumerTests
{
  private static readonly DateTimeOffset Now = new(2026, 5, 16, 12, 0, 0, TimeSpan.Zero);

  [Fact]
  public async Task ValidateStockConsumer_ShouldConsumeStockValidationRequestedEvent()
  {
    var message = StockValidationRequested();
    var useCase = Substitute.For<IValidateSaleStockUseCase>();
    var inboxStore = InboxStore(message.EventId, nameof(ValidateStockConsumer), alreadyProcessed: false);
    var clock = Clock();
    var consumer = new ValidateStockConsumer(
      NullLogger<ValidateStockConsumer>.Instance,
      inboxStore,
      clock,
      useCase);

    useCase.ExecuteAsync(message, Arg.Any<CancellationToken>())
      .Returns(Task.FromResult(Result.Success()));

    await consumer.Consume(Context(message));

    await useCase.Received(1).ExecuteAsync(message, Arg.Any<CancellationToken>());
    await inboxStore.Received(1).MarkProcessedAsync(
      message.EventId,
      nameof(ValidateStockConsumer),
      message.BusinessId,
      message.CorrelationId,
      Now,
      Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task DeductInventoryConsumer_ShouldIgnoreDuplicateMessage()
  {
    var message = InventoryDeductionRequested();
    var useCase = Substitute.For<IDeductSaleInventoryUseCase>();
    var inboxStore = InboxStore(message.EventId, nameof(DeductInventoryConsumer), alreadyProcessed: true);
    var consumer = new DeductInventoryConsumer(
      NullLogger<DeductInventoryConsumer>.Instance,
      inboxStore,
      Clock(),
      useCase);

    await consumer.Consume(Context(message));

    await useCase.DidNotReceive().ExecuteAsync(Arg.Any<InventoryDeductionRequestedEventV1>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task RegisterPaymentConsumer_ShouldPreserveCorrelationId()
  {
    var message = new PaymentRegistrationRequestedEventV1(
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      200,
      "Cash",
      Now);
    var useCase = Substitute.For<IRegisterSalePaymentUseCase>();
    var inboxStore = InboxStore(message.EventId, nameof(RegisterPaymentConsumer), alreadyProcessed: false);
    PaymentRegistrationRequestedEventV1? captured = null;
    var consumer = new RegisterPaymentConsumer(
      NullLogger<RegisterPaymentConsumer>.Instance,
      inboxStore,
      Clock(),
      useCase);

    useCase
      .ExecuteAsync(Arg.Do<PaymentRegistrationRequestedEventV1>(received => captured = received), Arg.Any<CancellationToken>())
      .Returns(Task.FromResult(Result.Success()));

    await consumer.Consume(Context(message));

    captured.Should().NotBeNull();
    captured!.CorrelationId.Should().Be(message.CorrelationId);
  }

  [Fact]
  public async Task GenerateInvoiceConsumer_ShouldDelegateToGenerateInvoiceUseCase()
  {
    var message = new InvoiceGenerationRequestedEventV1(
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.Empty,
      200,
      Now);
    var useCase = Substitute.For<IGenerateInvoiceUseCase>();
    var inboxStore = InboxStore(message.EventId, nameof(GenerateInvoiceConsumer), alreadyProcessed: false);
    GenerateInvoiceCommand? captured = null;
    var consumer = new GenerateInvoiceConsumer(
      NullLogger<GenerateInvoiceConsumer>.Instance,
      inboxStore,
      Clock(),
      useCase);

    useCase
      .Handle(Arg.Do<GenerateInvoiceCommand>(command => captured = command), Arg.Any<CancellationToken>())
      .Returns(Task.FromResult(Result.Success(InvoiceResponse(message))));

    await consumer.Consume(Context(message));

    captured.Should().NotBeNull();
    captured!.BusinessId.Should().Be(message.BusinessId);
    captured.SaleId.Should().Be(message.SaleId);
  }

  [Fact]
  public async Task SaleCompletedConsumer_ShouldDelegateToCompleteUseCase()
  {
    var message = new SaleCompletedEventV1(
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      200,
      Now);
    var useCase = Substitute.For<ICompleteSaleUseCase>();
    var generateInvoice = Substitute.For<IGenerateInvoiceUseCase>();
    var consumer = new SaleCompletedConsumer(
      NullLogger<SaleCompletedConsumer>.Instance,
      InboxStore(message.EventId, nameof(SaleCompletedConsumer), alreadyProcessed: false),
      Clock(),
      useCase,
      generateInvoice);

    useCase.ExecuteAsync(message, Arg.Any<CancellationToken>())
      .Returns(Task.FromResult(Result.Success()));
    generateInvoice
      .Handle(Arg.Any<GenerateInvoiceCommand>(), Arg.Any<CancellationToken>())
      .Returns(Task.FromResult(Result.Success(InvoiceResponse(message))));

    await consumer.Consume(Context(message));

    await useCase.Received(1).ExecuteAsync(message, Arg.Any<CancellationToken>());
    await generateInvoice.Received(1).Handle(
      Arg.Is<GenerateInvoiceCommand>(command => command.SaleId == message.SaleId),
      Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task SaleFailedConsumer_ShouldDelegateToFailUseCase()
  {
    var message = new SaleFailedEventV1(
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      "stock failed",
      Now);
    var useCase = Substitute.For<IFailSaleUseCase>();
    var consumer = new SaleFailedConsumer(
      NullLogger<SaleFailedConsumer>.Instance,
      InboxStore(message.EventId, nameof(SaleFailedConsumer), alreadyProcessed: false),
      Clock(),
      useCase);

    useCase.ExecuteAsync(message, Arg.Any<CancellationToken>())
      .Returns(Task.FromResult(Result.Success()));

    await consumer.Consume(Context(message));

    await useCase.Received(1).ExecuteAsync(message, Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task InventoryDeductedConsumer_ShouldProcessEvent_WhenMessageIsValid()
  {
    var message = InventoryDeducted();
    var realtime = Substitute.For<IRealtimeNotifier>();
    var consumer = new InventoryDeductedConsumer(
      InboxStore(message.EventId, nameof(InventoryDeductedConsumer), alreadyProcessed: false),
      Clock(),
      realtime,
      NullLogger<InventoryDeductedConsumer>.Instance);

    await consumer.Consume(Context(message));

    await realtime.Received(1).NotifyBusinessAsync(
      message.BusinessId,
      "inventory.stockChanged",
      Arg.Any<object>(),
      Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task InventoryAdjustedConsumer_ShouldNotifyAdjustedAndStockChanged()
  {
    var message = new InventoryAdjustedEventV1(
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      3,
      7,
      Now);
    var realtime = Substitute.For<IRealtimeNotifier>();
    var consumer = new InventoryAdjustedConsumer(
      InboxStore(message.EventId, nameof(InventoryAdjustedConsumer), alreadyProcessed: false),
      Clock(),
      realtime,
      NullLogger<InventoryAdjustedConsumer>.Instance);

    await consumer.Consume(Context(message));

    await realtime.Received(1).NotifyBusinessAsync(
      message.BusinessId,
      "inventory.adjusted",
      message,
      Arg.Any<CancellationToken>());
    await realtime.Received(1).NotifyBusinessAsync(
      message.BusinessId,
      "inventory.stockChanged",
      Arg.Any<object>(),
      Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task InventoryDeductedConsumer_ShouldNotDeductTwice_WhenMessageIsDuplicated()
  {
    var message = InventoryDeducted();
    var realtime = Substitute.For<IRealtimeNotifier>();
    var consumer = new InventoryDeductedConsumer(
      InboxStore(message.EventId, nameof(InventoryDeductedConsumer), alreadyProcessed: true),
      Clock(),
      realtime,
      NullLogger<InventoryDeductedConsumer>.Instance);

    await consumer.Consume(Context(message));

    await realtime.DidNotReceive().NotifyBusinessAsync(
      Arg.Any<Guid>(),
      Arg.Any<string>(),
      Arg.Any<object>(),
      Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task LowStockDetectedConsumer_ShouldNotifyBusinessGroup()
  {
    var message = new LowStockDetectedEventV1(
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      "Cafe",
      2,
      5,
      Now);
    var realtime = Substitute.For<IRealtimeNotifier>();
    var consumer = new LowStockDetectedConsumer(
      InboxStore(message.EventId, nameof(LowStockDetectedConsumer), alreadyProcessed: false),
      Clock(),
      realtime,
      NullLogger<LowStockDetectedConsumer>.Instance);

    await consumer.Consume(Context(message));

    await realtime.Received(1).NotifyBusinessAsync(
      message.BusinessId,
      "inventory.lowStockDetected",
      Arg.Is<LowStockDetectedNotificationV1>(notification =>
        notification.BusinessId == message.BusinessId &&
        notification.ProductId == message.ProductId &&
        notification.CurrentStock == 2),
      Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task CustomerCreditDebitedNotificationConsumer_ShouldNotifyBusinessGroup()
  {
    var message = new CustomerCreditDebitedEventV1(
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      800,
      2500,
      Now);
    var realtime = Substitute.For<IRealtimeNotifier>();
    var consumer = new CustomerCreditDebitedNotificationConsumer(
      InboxStore(message.EventId, nameof(CustomerCreditDebitedNotificationConsumer), alreadyProcessed: false),
      Clock(),
      realtime,
      NullLogger<CustomerCreditDebitedNotificationConsumer>.Instance);

    await consumer.Consume(Context(message));

    await realtime.Received(1).NotifyBusinessAsync(
      message.BusinessId,
      CustomerCreditRealtimeEvents.CreditDebited,
      Arg.Is<CustomerCreditDebitedNotificationV1>(notification =>
        notification.BusinessId == message.BusinessId &&
        notification.CustomerId == message.CustomerId &&
        notification.NewBalance == 2500),
      Arg.Any<CancellationToken>());
  }

  private static StockValidationRequestedEventV1 StockValidationRequested()
    => new(
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      [new SaleItemV1(Guid.NewGuid(), 2, 100)],
      200,
      "Cash",
      Now);

  private static InventoryDeductionRequestedEventV1 InventoryDeductionRequested()
    => new(
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      [new SaleItemV1(Guid.NewGuid(), 2, 100)],
      200,
      "Cash",
      Now);

  private static InventoryDeductedEventV1 InventoryDeducted()
    => new(
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      [new SaleItemV1(Guid.NewGuid(), 2, 100)],
      200,
      "Cash",
      Now);

  private static InvoiceResponse InvoiceResponse(InvoiceGenerationRequestedEventV1 message)
    => new(
      Guid.NewGuid(),
      message.BusinessId,
      message.BranchId,
      message.SaleId,
      null,
      "RI-00000001",
      message.Total,
      0,
      0,
      message.Total,
      "Issued",
      Now,
      Now,
      null);

  private static InvoiceResponse InvoiceResponse(SaleCompletedEventV1 message)
    => new(
      Guid.NewGuid(),
      message.BusinessId,
      message.BranchId,
      message.SaleId,
      null,
      "RI-00000001",
      message.Total,
      0,
      0,
      message.Total,
      "Issued",
      Now,
      Now,
      null);

  private static ConsumeContext<TMessage> Context<TMessage>(TMessage message)
    where TMessage : class
  {
    var context = Substitute.For<ConsumeContext<TMessage>>();
    context.Message.Returns(message);
    context.CancellationToken.Returns(CancellationToken.None);

    return context;
  }

  private static IInboxStore InboxStore(Guid eventId, string consumerName, bool alreadyProcessed)
  {
    var inboxStore = Substitute.For<IInboxStore>();
    inboxStore
      .HasProcessedAsync(eventId, consumerName, Arg.Any<CancellationToken>())
      .Returns(alreadyProcessed);
    inboxStore
      .MarkProcessedAsync(
        eventId,
        consumerName,
        Arg.Any<Guid>(),
        Arg.Any<Guid>(),
        Arg.Any<DateTimeOffset>(),
        Arg.Any<CancellationToken>())
      .Returns(Task.CompletedTask);

    return inboxStore;
  }

  private static IClock Clock()
  {
    var clock = Substitute.For<IClock>();
    clock.UtcNow.Returns(Now);
    return clock;
  }

  private sealed class RecordingOutboxWriter : IOutboxWriter
  {
    public List<IIntegrationEvent> Events { get; } = [];

    public Task AddAsync<TEvent>(
      TEvent integrationEvent,
      CancellationToken cancellationToken = default)
      where TEvent : class, IIntegrationEvent
    {
      Events.Add(integrationEvent);
      return Task.CompletedTask;
    }
  }

  private sealed class NoopUnitOfWork : IUnitOfWork
  {
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
      => Task.FromResult(1);
  }
}

#pragma warning restore CA1707
