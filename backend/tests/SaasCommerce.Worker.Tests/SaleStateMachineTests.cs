using FluentAssertions;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
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
  public async Task SaleCreatedShouldStartSagaInProcessingState()
  {
    await using var provider = CreateProvider();
    var harness = provider.GetRequiredService<ITestHarness>();
    var sagaHarness = harness.GetSagaStateMachineHarness<SaleStateMachine, SaleSagaState>();
    var created = CreateSaleCreated();

    await harness.Start();

    try
    {
      await harness.Bus.Publish(created);

      (await sagaHarness.Consumed.Any<SaleCreatedIntegrationEventV1>()).Should().BeTrue();
      (await sagaHarness.Exists(created.CorrelationId, state => state.Processing)).Should().NotBeNull();
    }
    finally
    {
      await harness.Stop();
    }
  }

  [Fact]
  public async Task SagaShouldReachCompletedState()
  {
    await using var provider = CreateProvider();
    var harness = provider.GetRequiredService<ITestHarness>();
    var sagaHarness = harness.GetSagaStateMachineHarness<SaleStateMachine, SaleSagaState>();
    var created = CreateSaleCreated();

    await harness.Start();

    try
    {
      await harness.Bus.Publish(created);
      await harness.Bus.Publish(new StockValidatedIntegrationEventV1(
        Guid.NewGuid(),
        created.CorrelationId,
        created.BusinessId,
        created.SaleId,
        created.BranchId,
        created.UserId,
        DateTimeOffset.UtcNow));
      await harness.Bus.Publish(new InventoryDeductedIntegrationEventV1(
        Guid.NewGuid(),
        created.CorrelationId,
        created.BusinessId,
        created.SaleId,
        created.BranchId,
        created.UserId,
        DateTimeOffset.UtcNow));
      await harness.Bus.Publish(new PaymentRegisteredIntegrationEventV1(
        Guid.NewGuid(),
        created.CorrelationId,
        created.BusinessId,
        created.SaleId,
        created.BranchId,
        created.UserId,
        Guid.NewGuid(),
        created.Total,
        DateTimeOffset.UtcNow));
      await harness.Bus.Publish(new InvoiceGeneratedIntegrationEventV1(
        Guid.NewGuid(),
        created.CorrelationId,
        created.BusinessId,
        created.SaleId,
        created.BranchId,
        created.UserId,
        Guid.NewGuid(),
        DateTimeOffset.UtcNow));
      await harness.Bus.Publish(new SaleCompletedIntegrationEventV1(
        Guid.NewGuid(),
        created.CorrelationId,
        created.BusinessId,
        created.SaleId,
        created.BranchId,
        created.UserId,
        created.Total,
        DateTimeOffset.UtcNow));

      (await sagaHarness.Exists(created.CorrelationId, state => state.Completed)).Should().NotBeNull();
    }
    finally
    {
      await harness.Stop();
    }
  }

  [Fact]
  public async Task FailureEventsShouldMoveSagaToFailedState()
  {
    await using var provider = CreateProvider();
    var harness = provider.GetRequiredService<ITestHarness>();
    var sagaHarness = harness.GetSagaStateMachineHarness<SaleStateMachine, SaleSagaState>();
    var created = CreateSaleCreated();

    await harness.Start();

    try
    {
      await harness.Bus.Publish(created);
      await harness.Bus.Publish(new StockValidationFailedIntegrationEventV1(
        Guid.NewGuid(),
        created.CorrelationId,
        created.BusinessId,
        created.SaleId,
        created.BranchId,
        created.UserId,
        "stock unavailable",
        DateTimeOffset.UtcNow));

      (await sagaHarness.Exists(created.CorrelationId, state => state.Failed)).Should().NotBeNull();
    }
    finally
    {
      await harness.Stop();
    }
  }

  private static ServiceProvider CreateProvider()
    => new ServiceCollection()
      .AddLogging()
      .AddMassTransitTestHarness(configurator =>
      {
        configurator.AddSagaStateMachine<SaleStateMachine, SaleSagaState>()
          .InMemoryRepository();
      })
      .BuildServiceProvider(true);

  private static SaleCreatedIntegrationEventV1 CreateSaleCreated()
    => new(
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      175.50m,
      DateTimeOffset.UtcNow);
}
