using FluentAssertions;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Contracts.Events;
using SaasCommerce.BuildingBlocks.Infrastructure.Messaging.Sagas.Sales;
using SaasCommerce.Modules.Billing.Contracts.Events.V1;
using SaasCommerce.Modules.Inventory.Contracts.Events.V1;
using SaasCommerce.Modules.Payments.Contracts.Events.V1;
using SaasCommerce.Modules.Sales.Contracts.Events.V1;
using SaasCommerce.Worker.Sagas.Sales;

namespace SaasCommerce.Worker.Tests;

public sealed class SaleStateMachineTests
{
  [Fact]
  public async Task SaleStateMachineShouldStartSagaWhenSaleCreatedEventIsConsumed()
  {
    await using var provider = CreateProvider();
    var harness = provider.GetRequiredService<ITestHarness>();
    var sagaHarness = harness.GetSagaStateMachineHarness<SaleStateMachine, SaleSagaState>();
    var outbox = provider.GetRequiredService<RecordingOutboxWriter>();
    var created = CreateSaleCreated();

    await harness.Start();

    try
    {
      await harness.Bus.Publish(created);

      (await sagaHarness.Consumed.Any<SaleCreatedEventV1>()).Should().BeTrue();
      (await sagaHarness.Exists(created.CorrelationId, state => state.StockValidationPending))
        .Should()
        .NotBeNull();
      outbox.Events.Should().ContainSingle(@event => @event is StockValidationRequestedEventV1);
    }
    finally
    {
      await harness.Stop();
    }
  }

  [Fact]
  public async Task SaleStateMachineShouldRequestInventoryDeductionWhenStockValidated()
  {
    await using var provider = CreateProvider();
    var harness = provider.GetRequiredService<ITestHarness>();
    var sagaHarness = harness.GetSagaStateMachineHarness<SaleStateMachine, SaleSagaState>();
    var outbox = provider.GetRequiredService<RecordingOutboxWriter>();
    var created = CreateSaleCreated();

    await harness.Start();

    try
    {
      await harness.Bus.Publish(created);
      await harness.Bus.Publish(CreateStockValidated(created));

      (await sagaHarness.Exists(created.CorrelationId, state => state.InventoryDeductionPending))
        .Should()
        .NotBeNull();
      outbox.Events.Should().Contain(@event => @event is InventoryDeductionRequestedEventV1);
    }
    finally
    {
      await harness.Stop();
    }
  }

  [Fact]
  public async Task SaleStateMachineShouldMoveToFailedWhenStockValidationFailed()
  {
    await using var provider = CreateProvider();
    var harness = provider.GetRequiredService<ITestHarness>();
    var sagaHarness = harness.GetSagaStateMachineHarness<SaleStateMachine, SaleSagaState>();
    var outbox = provider.GetRequiredService<RecordingOutboxWriter>();
    var created = CreateSaleCreated();

    await harness.Start();

    try
    {
      await harness.Bus.Publish(created);
      await harness.Bus.Publish(new StockValidationFailedEventV1(
        Guid.NewGuid(),
        created.CorrelationId,
        created.SaleId,
        created.BusinessId,
        created.BranchId,
        created.UserId,
        "stock unavailable",
        DateTimeOffset.UtcNow));

      (await sagaHarness.Exists(created.CorrelationId, state => state.Failed)).Should().NotBeNull();
      outbox.Events.Should().Contain(@event => @event is SaleFailedEventV1);
    }
    finally
    {
      await harness.Stop();
    }
  }

  [Fact]
  public async Task SaleStateMachineShouldRequestPaymentRegistrationWhenInventoryDeducted()
  {
    await using var provider = CreateProvider();
    var harness = provider.GetRequiredService<ITestHarness>();
    var sagaHarness = harness.GetSagaStateMachineHarness<SaleStateMachine, SaleSagaState>();
    var outbox = provider.GetRequiredService<RecordingOutboxWriter>();
    var created = CreateSaleCreated();

    await harness.Start();

    try
    {
      await harness.Bus.Publish(created);
      await harness.Bus.Publish(CreateStockValidated(created));
      await harness.Bus.Publish(CreateInventoryDeducted(created));

      (await sagaHarness.Exists(created.CorrelationId, state => state.PaymentRegistrationPending))
        .Should()
        .NotBeNull();
      outbox.Events.Should().Contain(@event => @event is PaymentRegistrationRequestedEventV1);
    }
    finally
    {
      await harness.Stop();
    }
  }

  [Fact]
  public async Task SaleStateMachineShouldCompleteWhenPaymentRegistered()
  {
    await using var provider = CreateProvider();
    var harness = provider.GetRequiredService<ITestHarness>();
    var sagaHarness = harness.GetSagaStateMachineHarness<SaleStateMachine, SaleSagaState>();
    var outbox = provider.GetRequiredService<RecordingOutboxWriter>();
    var created = CreateSaleCreated();

    await harness.Start();

    try
    {
      await harness.Bus.Publish(created);
      await harness.Bus.Publish(CreateStockValidated(created));
      await harness.Bus.Publish(CreateInventoryDeducted(created));
      await harness.Bus.Publish(CreatePaymentRegistered(created));

      (await sagaHarness.Exists(created.CorrelationId, state => state.Completed)).Should().NotBeNull();
      outbox.Events.Should().Contain(@event => @event is SaleCompletedEventV1);
    }
    finally
    {
      await harness.Stop();
    }
  }

  [Fact]
  public async Task SaleStateMachineShouldIgnoreInvoiceGeneratedAfterCompletion()
  {
    await using var provider = CreateProvider();
    var harness = provider.GetRequiredService<ITestHarness>();
    var sagaHarness = harness.GetSagaStateMachineHarness<SaleStateMachine, SaleSagaState>();
    var outbox = provider.GetRequiredService<RecordingOutboxWriter>();
    var created = CreateSaleCreated();
    var paymentRegistered = CreatePaymentRegistered(created);

    await harness.Start();

    try
    {
      await harness.Bus.Publish(created);
      await harness.Bus.Publish(CreateStockValidated(created));
      await harness.Bus.Publish(CreateInventoryDeducted(created));
      await harness.Bus.Publish(paymentRegistered);
      await harness.Bus.Publish(CreateInvoiceGenerated(created, paymentRegistered.PaymentId));

      (await sagaHarness.Exists(created.CorrelationId, state => state.Completed)).Should().NotBeNull();
      outbox.Events.OfType<SaleCompletedEventV1>().Should().ContainSingle();
    }
    finally
    {
      await harness.Stop();
    }
  }

  [Fact]
  public async Task SaleStateMachineShouldIgnoreInvoiceFailedAfterCompletion()
  {
    await using var provider = CreateProvider();
    var harness = provider.GetRequiredService<ITestHarness>();
    var sagaHarness = harness.GetSagaStateMachineHarness<SaleStateMachine, SaleSagaState>();
    var created = CreateSaleCreated();
    var paymentRegistered = CreatePaymentRegistered(created);

    await harness.Start();

    try
    {
      await harness.Bus.Publish(created);
      await harness.Bus.Publish(CreateStockValidated(created));
      await harness.Bus.Publish(CreateInventoryDeducted(created));
      await harness.Bus.Publish(paymentRegistered);
      await harness.Bus.Publish(new InvoiceFailedEventV1(
        Guid.NewGuid(),
        created.CorrelationId,
        created.SaleId,
        created.BusinessId,
        created.BranchId,
        created.UserId,
        paymentRegistered.PaymentId,
        "invoice failed",
        DateTimeOffset.UtcNow));

      (await sagaHarness.Exists(created.CorrelationId, state => state.Completed)).Should().NotBeNull();
    }
    finally
    {
      await harness.Stop();
    }
  }

  [Fact]
  public async Task SaleStateMachineShouldIgnoreDuplicateEventsSafely()
  {
    await using var provider = CreateProvider();
    var harness = provider.GetRequiredService<ITestHarness>();
    var sagaHarness = harness.GetSagaStateMachineHarness<SaleStateMachine, SaleSagaState>();
    var outbox = provider.GetRequiredService<RecordingOutboxWriter>();
    var created = CreateSaleCreated();
    var stockValidated = CreateStockValidated(created);

    await harness.Start();

    try
    {
      await harness.Bus.Publish(created);
      await harness.Bus.Publish(stockValidated);
      await harness.Bus.Publish(stockValidated);

      (await sagaHarness.Exists(created.CorrelationId, state => state.InventoryDeductionPending))
        .Should()
        .NotBeNull();
      outbox.Events.OfType<InventoryDeductionRequestedEventV1>().Should().ContainSingle();
    }
    finally
    {
      await harness.Stop();
    }
  }

  private static ServiceProvider CreateProvider()
    => new ServiceCollection()
      .AddLogging()
      .AddSingleton<RecordingOutboxWriter>()
      .AddSingleton<IOutboxWriter>(provider => provider.GetRequiredService<RecordingOutboxWriter>())
      .AddMassTransitTestHarness(configurator =>
      {
        configurator.AddSagaStateMachine<SaleStateMachine, SaleSagaState>()
          .InMemoryRepository();
      })
      .BuildServiceProvider(true);

  private static SaleCreatedEventV1 CreateSaleCreated()
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
      DateTimeOffset.UtcNow);

  private static StockValidatedEventV1 CreateStockValidated(SaleCreatedEventV1 created)
    => new(
      Guid.NewGuid(),
      created.CorrelationId,
      created.SaleId,
      created.BusinessId,
      created.BranchId,
      created.UserId,
      created.Items,
      created.Total,
      created.PaymentMethod,
      DateTimeOffset.UtcNow);

  private static InventoryDeductedEventV1 CreateInventoryDeducted(SaleCreatedEventV1 created)
    => new(
      Guid.NewGuid(),
      created.CorrelationId,
      created.SaleId,
      created.BusinessId,
      created.BranchId,
      created.UserId,
      created.Items,
      created.Total,
      created.PaymentMethod,
      DateTimeOffset.UtcNow);

  private static PaymentRegisteredEventV1 CreatePaymentRegistered(SaleCreatedEventV1 created)
    => new(
      Guid.NewGuid(),
      created.CorrelationId,
      created.SaleId,
      created.BusinessId,
      created.BranchId,
      created.UserId,
      Guid.NewGuid(),
      created.Total,
      created.PaymentMethod,
      DateTimeOffset.UtcNow);

  private static InvoiceGeneratedEventV1 CreateInvoiceGenerated(
    SaleCreatedEventV1 created,
    Guid paymentId)
    => new(
      Guid.NewGuid(),
      created.CorrelationId,
      created.SaleId,
      created.BusinessId,
      created.BranchId,
      created.UserId,
      paymentId,
      Guid.NewGuid(),
      created.Total,
      DateTimeOffset.UtcNow);

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
}
