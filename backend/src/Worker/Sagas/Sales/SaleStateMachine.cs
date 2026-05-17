using MassTransit;
using SaasCommerce.BuildingBlocks.Infrastructure.Messaging.Sagas.Sales;
using SaasCommerce.Modules.Billing.Contracts.Events.V1;
using SaasCommerce.Modules.Inventory.Contracts.Events.V1;
using SaasCommerce.Modules.Payments.Contracts.Events.V1;
using SaasCommerce.Modules.Sales.Contracts.Events.V1;

namespace SaasCommerce.Worker.Sagas.Sales;

public sealed class SaleStateMachine : MassTransitStateMachine<SaleSagaState>
{
  private static readonly Action<ILogger, Guid, Guid, Guid, Guid, Guid, string, Exception?> LogSagaTransition =
    LoggerMessage.Define<Guid, Guid, Guid, Guid, Guid, string>(
      LogLevel.Information,
      new EventId(2400, nameof(LogSagaTransition)),
      "Sale saga transition. CorrelationId={CorrelationId} BusinessId={BusinessId} BranchId={BranchId} UserId={UserId} SaleId={SaleId} SagaState={SagaState}");
  private static readonly Action<ILogger, Guid, Guid, Guid, string, string, Exception?> LogDuplicateEvent =
    LoggerMessage.Define<Guid, Guid, Guid, string, string>(
      LogLevel.Information,
      new EventId(2401, nameof(LogDuplicateEvent)),
      "Duplicate sale saga event ignored. CorrelationId={CorrelationId} BusinessId={BusinessId} SaleId={SaleId} EventName={EventName} SagaState={SagaState}");
  private static readonly Action<ILogger, Guid, Guid, Guid, string, string, Exception?> LogOutOfOrderEvent =
    LoggerMessage.Define<Guid, Guid, Guid, string, string>(
      LogLevel.Warning,
      new EventId(2402, nameof(LogOutOfOrderEvent)),
      "Out-of-order sale saga event ignored. CorrelationId={CorrelationId} BusinessId={BusinessId} SaleId={SaleId} EventName={EventName} SagaState={SagaState}");

  public SaleStateMachine(ILogger<SaleStateMachine> logger)
  {
    InstanceState(state => state.CurrentState);

    Event(() => SaleCreated, configurator => configurator.CorrelateById(context => context.Message.CorrelationId));
    Event(() => StockValidatedEvent, configurator => configurator.CorrelateById(context => context.Message.CorrelationId));
    Event(() => StockValidationFailed, configurator => configurator.CorrelateById(context => context.Message.CorrelationId));
    Event(() => InventoryDeductedEvent, configurator => configurator.CorrelateById(context => context.Message.CorrelationId));
    Event(() => InventoryDeductionFailed, configurator => configurator.CorrelateById(context => context.Message.CorrelationId));
    Event(() => PaymentRegisteredEvent, configurator => configurator.CorrelateById(context => context.Message.CorrelationId));
    Event(() => PaymentFailed, configurator => configurator.CorrelateById(context => context.Message.CorrelationId));
    Event(() => InvoiceGeneratedEvent, configurator => configurator.CorrelateById(context => context.Message.CorrelationId));
    Event(() => InvoiceGenerationFailed, configurator => configurator.CorrelateById(context => context.Message.CorrelationId));
    Event(() => SaleCompleted, configurator => configurator.CorrelateById(context => context.Message.CorrelationId));
    Event(() => SaleFailed, configurator => configurator.CorrelateById(context => context.Message.CorrelationId));

    Initially(
      When(SaleCreated)
        .Then(context =>
        {
          context.Saga.SaleId = context.Message.SaleId;
          context.Saga.BusinessId = context.Message.BusinessId;
          context.Saga.BranchId = context.Message.BranchId;
          context.Saga.UserId = context.Message.UserId;
          context.Saga.CreatedAt = context.Message.OccurredAt;
          context.Saga.UpdatedAt = context.Message.OccurredAt;
          LogTransition(logger, context.Saga, "Initial", "Processing");
        })
        .TransitionTo(Processing));

    During(
      Processing,
      When(StockValidatedEvent)
        .Then(context => MarkUpdated(logger, context.Saga, context.Message.OccurredAt, "StockValidated"))
        .TransitionTo(StockValidated),
      When(InventoryDeductedEvent).Then(context => LogOutOfOrder(logger, context.Saga, nameof(InventoryDeductedEvent))),
      When(PaymentRegisteredEvent).Then(context => LogOutOfOrder(logger, context.Saga, nameof(PaymentRegisteredEvent))),
      When(InvoiceGeneratedEvent).Then(context => LogOutOfOrder(logger, context.Saga, nameof(InvoiceGeneratedEvent))));

    During(
      StockValidated,
      When(InventoryDeductedEvent)
        .Then(context => MarkUpdated(logger, context.Saga, context.Message.OccurredAt, "InventoryDeducted"))
        .TransitionTo(InventoryDeducted),
      When(StockValidatedEvent).Then(context => LogDuplicate(logger, context.Saga, nameof(StockValidatedEvent))));

    During(
      InventoryDeducted,
      When(PaymentRegisteredEvent)
        .Then(context => MarkUpdated(logger, context.Saga, context.Message.OccurredAt, "PaymentRegistered"))
        .TransitionTo(PaymentRegistered),
      When(InventoryDeductedEvent).Then(context => LogDuplicate(logger, context.Saga, nameof(InventoryDeductedEvent))));

    During(
      PaymentRegistered,
      When(InvoiceGeneratedEvent)
        .Then(context => MarkUpdated(logger, context.Saga, context.Message.OccurredAt, "InvoiceGenerated"))
        .TransitionTo(InvoiceGenerated),
      When(PaymentRegisteredEvent).Then(context => LogDuplicate(logger, context.Saga, nameof(PaymentRegisteredEvent))));

    During(
      InvoiceGenerated,
      When(SaleCompleted)
        .Then(context =>
        {
          context.Saga.CompletedAt = context.Message.OccurredAt;
          MarkUpdated(logger, context.Saga, context.Message.OccurredAt, "Completed");
        })
        .TransitionTo(Completed),
      When(InvoiceGeneratedEvent).Then(context => LogDuplicate(logger, context.Saga, nameof(InvoiceGeneratedEvent))));

    DuringAny(
      When(StockValidationFailed)
        .Then(context => MarkFailed(logger, context.Saga, context.Message.OccurredAt, context.Message.Reason))
        .TransitionTo(Failed),
      When(InventoryDeductionFailed)
        .Then(context => MarkFailed(logger, context.Saga, context.Message.OccurredAt, context.Message.Reason))
        .TransitionTo(Failed),
      When(PaymentFailed)
        .Then(context => MarkFailed(logger, context.Saga, context.Message.OccurredAt, context.Message.Reason))
        .TransitionTo(Failed),
      When(InvoiceGenerationFailed)
        .Then(context => MarkFailed(logger, context.Saga, context.Message.OccurredAt, context.Message.Reason))
        .TransitionTo(Failed),
      When(SaleFailed)
        .Then(context => MarkFailed(logger, context.Saga, context.Message.OccurredAt, context.Message.Reason))
        .TransitionTo(Failed));
  }

  public State Received { get; private set; } = null!;

  public State Processing { get; private set; } = null!;

  public State StockValidated { get; private set; } = null!;

  public State InventoryDeducted { get; private set; } = null!;

  public State PaymentRegistered { get; private set; } = null!;

  public State InvoiceGenerated { get; private set; } = null!;

  public State Completed { get; private set; } = null!;

  public State Failed { get; private set; } = null!;

  public State Cancelled { get; private set; } = null!;

  public Event<SaleCreatedIntegrationEventV1> SaleCreated { get; private set; } = null!;

  public Event<StockValidatedIntegrationEventV1> StockValidatedEvent { get; private set; } = null!;

  public Event<StockValidationFailedIntegrationEventV1> StockValidationFailed { get; private set; } = null!;

  public Event<InventoryDeductedIntegrationEventV1> InventoryDeductedEvent { get; private set; } = null!;

  public Event<InventoryDeductionFailedIntegrationEventV1> InventoryDeductionFailed { get; private set; } = null!;

  public Event<PaymentRegisteredIntegrationEventV1> PaymentRegisteredEvent { get; private set; } = null!;

  public Event<PaymentFailedIntegrationEventV1> PaymentFailed { get; private set; } = null!;

  public Event<InvoiceGeneratedIntegrationEventV1> InvoiceGeneratedEvent { get; private set; } = null!;

  public Event<InvoiceGenerationFailedIntegrationEventV1> InvoiceGenerationFailed { get; private set; } = null!;

  public Event<SaleCompletedIntegrationEventV1> SaleCompleted { get; private set; } = null!;

  public Event<SaleFailedIntegrationEventV1> SaleFailed { get; private set; } = null!;

  private static void MarkUpdated(
    ILogger logger,
    SaleSagaState state,
    DateTimeOffset occurredAt,
    string nextState)
  {
    LogTransition(logger, state, state.CurrentState, nextState);
    state.UpdatedAt = occurredAt;
  }

  private static void MarkFailed(
    ILogger logger,
    SaleSagaState state,
    DateTimeOffset occurredAt,
    string reason)
  {
    state.FailedAt = occurredAt;
    state.FailureReason = reason;
    MarkUpdated(logger, state, occurredAt, "Failed");
  }

  private static void LogTransition(
    ILogger logger,
    SaleSagaState state,
    string previousState,
    string nextState)
  {
    LogSagaTransition(
      logger,
      state.CorrelationId,
      state.BusinessId,
      state.BranchId,
      state.UserId,
      state.SaleId,
      $"{previousState}->{nextState}",
      null);
  }

  private static void LogDuplicate(
    ILogger logger,
    SaleSagaState state,
    string eventName)
  {
    LogDuplicateEvent(
      logger,
      state.CorrelationId,
      state.BusinessId,
      state.SaleId,
      eventName,
      state.CurrentState,
      null);
  }

  private static void LogOutOfOrder(
    ILogger logger,
    SaleSagaState state,
    string eventName)
  {
    LogOutOfOrderEvent(
      logger,
      state.CorrelationId,
      state.BusinessId,
      state.SaleId,
      eventName,
      state.CurrentState,
      null);
  }
}
