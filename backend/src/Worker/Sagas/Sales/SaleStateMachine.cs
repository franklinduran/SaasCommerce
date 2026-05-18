using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Contracts.Events;
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
    Event(() => InvoiceFailed, configurator => configurator.CorrelateById(context => context.Message.CorrelationId));

    Initially(
      When(SaleCreated)
        .Then(context =>
        {
          context.Saga.SaleId = context.Message.SaleId;
          context.Saga.BusinessId = context.Message.BusinessId;
          context.Saga.BranchId = context.Message.BranchId;
          context.Saga.UserId = context.Message.UserId;
          context.Saga.CreatedAt = context.Message.CreatedAt;
          context.Saga.UpdatedAt = context.Message.CreatedAt;
          context.Saga.Version = 1;
          LogTransition(logger, context.Saga, "Initial", nameof(StockValidationPending));
        })
        .ThenAsync(context => AddOutboxAsync(
          context,
          new StockValidationRequestedEventV1(
            Guid.NewGuid(),
            context.Message.CorrelationId,
            context.Message.SaleId,
            context.Message.BusinessId,
            context.Message.BranchId,
            context.Message.UserId,
            context.Message.Items,
            context.Message.Total,
            context.Message.PaymentMethod,
            DateTimeOffset.UtcNow)))
        .TransitionTo(StockValidationPending));

    During(
      StockValidationPending,
      When(StockValidatedEvent)
        .Then(context => MarkUpdated(logger, context.Saga, context.Message.CreatedAt, nameof(InventoryDeductionPending)))
        .ThenAsync(context => AddOutboxAsync(
          context,
          new InventoryDeductionRequestedEventV1(
            Guid.NewGuid(),
            context.Message.CorrelationId,
            context.Message.SaleId,
            context.Message.BusinessId,
            context.Message.BranchId,
            context.Message.UserId,
            context.Message.Items,
            context.Message.Total,
            context.Message.PaymentMethod,
            DateTimeOffset.UtcNow)))
        .TransitionTo(InventoryDeductionPending),
      When(InventoryDeductedEvent).Then(context => LogOutOfOrder(logger, context.Saga, nameof(InventoryDeductedEvent))),
      When(PaymentRegisteredEvent).Then(context => LogOutOfOrder(logger, context.Saga, nameof(PaymentRegisteredEvent))),
      When(InvoiceGeneratedEvent).Then(context => LogOutOfOrder(logger, context.Saga, nameof(InvoiceGeneratedEvent))));

    During(
      InventoryDeductionPending,
      When(InventoryDeductedEvent)
        .Then(context => MarkUpdated(logger, context.Saga, context.Message.CreatedAt, nameof(PaymentRegistrationPending)))
        .ThenAsync(context => AddOutboxAsync(
          context,
          new PaymentRegistrationRequestedEventV1(
            Guid.NewGuid(),
            context.Message.CorrelationId,
            context.Message.SaleId,
            context.Message.BusinessId,
            context.Message.BranchId,
            context.Message.UserId,
            context.Message.Total,
            context.Message.PaymentMethod,
            DateTimeOffset.UtcNow)))
        .TransitionTo(PaymentRegistrationPending),
      When(StockValidatedEvent).Then(context => LogDuplicate(logger, context.Saga, nameof(StockValidatedEvent))),
      When(PaymentRegisteredEvent).Then(context => LogOutOfOrder(logger, context.Saga, nameof(PaymentRegisteredEvent))));

    During(
      PaymentRegistrationPending,
      When(PaymentRegisteredEvent)
        .Then(context =>
        {
          context.Saga.CompletedAt = context.Message.CreatedAt;
          MarkUpdated(logger, context.Saga, context.Message.CreatedAt, nameof(Completed));
        })
        .ThenAsync(context => AddOutboxAsync(
          context,
          new SaleCompletedEventV1(
            Guid.NewGuid(),
            context.Message.CorrelationId,
            context.Message.SaleId,
            context.Message.BusinessId,
            context.Message.BranchId,
            context.Message.UserId,
            context.Message.PaymentId,
            Guid.Empty,
            context.Message.Amount,
            DateTimeOffset.UtcNow)))
        .TransitionTo(Completed),
      When(InventoryDeductedEvent).Then(context => LogDuplicate(logger, context.Saga, nameof(InventoryDeductedEvent))),
      When(InvoiceGeneratedEvent).Then(context => LogOutOfOrder(logger, context.Saga, nameof(InvoiceGeneratedEvent))));

    During(
      InvoiceGenerationPending,
      When(InvoiceGeneratedEvent)
        .Then(context =>
        {
          context.Saga.CompletedAt = context.Message.CreatedAt;
          MarkUpdated(logger, context.Saga, context.Message.CreatedAt, nameof(Completed));
        })
        .ThenAsync(context => AddOutboxAsync(
          context,
          new SaleCompletedEventV1(
            Guid.NewGuid(),
            context.Message.CorrelationId,
            context.Message.SaleId,
            context.Message.BusinessId,
            context.Message.BranchId,
            context.Message.UserId,
            context.Message.PaymentId,
            context.Message.InvoiceId,
            context.Message.Total,
            DateTimeOffset.UtcNow)))
        .TransitionTo(Completed),
      When(PaymentRegisteredEvent).Then(context => LogDuplicate(logger, context.Saga, nameof(PaymentRegisteredEvent))));

    DuringAny(
      When(StockValidationFailed)
        .Then(context => MarkFailed(logger, context.Saga, context.Message.CreatedAt, context.Message.Reason))
        .ThenAsync(context => AddSaleFailedAsync(context, context.Message.Reason))
        .TransitionTo(Failed),
      When(InventoryDeductionFailed)
        .Then(context => MarkFailed(logger, context.Saga, context.Message.CreatedAt, context.Message.Reason))
        .ThenAsync(context => AddSaleFailedAsync(context, context.Message.Reason))
        .TransitionTo(Failed),
      When(PaymentFailed)
        .Then(context => MarkFailed(logger, context.Saga, context.Message.CreatedAt, context.Message.Reason))
        .ThenAsync(context => AddSaleFailedAsync(context, context.Message.Reason))
        .TransitionTo(Failed),
      When(InvoiceFailed)
        .Then(context => MarkFailed(logger, context.Saga, context.Message.CreatedAt, context.Message.Reason))
        .ThenAsync(context => AddSaleFailedAsync(context, context.Message.Reason))
        .TransitionTo(Failed));

    During(
      Completed,
      Ignore(StockValidatedEvent),
      Ignore(InventoryDeductedEvent),
      Ignore(PaymentRegisteredEvent),
      Ignore(InvoiceGeneratedEvent),
      Ignore(InvoiceFailed));

    During(
      Failed,
      Ignore(StockValidatedEvent),
      Ignore(InventoryDeductedEvent),
      Ignore(PaymentRegisteredEvent),
      Ignore(InvoiceGeneratedEvent),
      Ignore(StockValidationFailed),
      Ignore(InventoryDeductionFailed),
      Ignore(PaymentFailed),
      Ignore(InvoiceFailed));
  }

  public State Processing { get; private set; } = null!;

  public State StockValidationPending { get; private set; } = null!;

  public State InventoryDeductionPending { get; private set; } = null!;

  public State PaymentRegistrationPending { get; private set; } = null!;

  public State InvoiceGenerationPending { get; private set; } = null!;

  public State Completed { get; private set; } = null!;

  public State Failed { get; private set; } = null!;

  public Event<SaleCreatedEventV1> SaleCreated { get; private set; } = null!;

  public Event<StockValidatedEventV1> StockValidatedEvent { get; private set; } = null!;

  public Event<StockValidationFailedEventV1> StockValidationFailed { get; private set; } = null!;

  public Event<InventoryDeductedEventV1> InventoryDeductedEvent { get; private set; } = null!;

  public Event<InventoryDeductionFailedEventV1> InventoryDeductionFailed { get; private set; } = null!;

  public Event<PaymentRegisteredEventV1> PaymentRegisteredEvent { get; private set; } = null!;

  public Event<PaymentFailedEventV1> PaymentFailed { get; private set; } = null!;

  public Event<InvoiceGeneratedEventV1> InvoiceGeneratedEvent { get; private set; } = null!;

  public Event<InvoiceFailedEventV1> InvoiceFailed { get; private set; } = null!;

  private static Task AddSaleFailedAsync<TMessage>(
    BehaviorContext<SaleSagaState, TMessage> context,
    string reason)
    where TMessage : class, IIntegrationEvent
    => AddOutboxAsync(
      context,
      new SaleFailedEventV1(
        Guid.NewGuid(),
        context.Message.CorrelationId,
        context.Saga.SaleId,
        context.Saga.BusinessId,
        context.Saga.BranchId,
        context.Saga.UserId,
        reason,
        DateTimeOffset.UtcNow));

  private static Task AddOutboxAsync<TMessage, TEvent>(
    BehaviorContext<SaleSagaState, TMessage> context,
    TEvent integrationEvent)
    where TMessage : class
    where TEvent : class, IIntegrationEvent
  {
    var outbox = context.GetPayload<IServiceProvider>().GetRequiredService<IOutboxWriter>();
    return outbox.AddAsync(integrationEvent, context.CancellationToken);
  }

  private static void MarkUpdated(
    ILogger logger,
    SaleSagaState state,
    DateTimeOffset occurredAt,
    string nextState)
  {
    LogTransition(logger, state, state.CurrentState, nextState);
    state.UpdatedAt = occurredAt;
    state.Version++;
  }

  private static void MarkFailed(
    ILogger logger,
    SaleSagaState state,
    DateTimeOffset occurredAt,
    string reason)
  {
    state.FailedAt = occurredAt;
    state.FailureReason = reason;
    MarkUpdated(logger, state, occurredAt, nameof(Failed));
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
