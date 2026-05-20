#pragma warning disable CA1707

using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Audit;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Billing.Contracts.Events.V1;
using SaasCommerce.Modules.Identity.Domain;
using SaasCommerce.Modules.Inventory.Contracts.Events.V1;
using SaasCommerce.Modules.Payments.Contracts.Events.V1;
using SaasCommerce.Modules.Purchasing.Contracts.Events.V1;
using SaasCommerce.Modules.Sales.Contracts.Events.V1;
using SaasCommerce.Worker.Consumers;

namespace SaasCommerce.Worker.Tests;

public sealed class AuditConsumerTests
{
  private static readonly DateTimeOffset Now = new(2026, 5, 19, 10, 0, 0, TimeSpan.Zero);

  [Fact]
  public async Task SaleCompletedAuditConsumer_ShouldWriteAuditEntry_WithCorrectActionAndEntity()
  {
    var message = SaleCompleted();
    var writer = Substitute.For<IAuditLogWriter>();
    var consumer = new SaleCompletedAuditConsumer(
      writer,
      InboxStore(message.EventId, nameof(SaleCompletedAuditConsumer), alreadyProcessed: false),
      Clock(),
      NullLogger<SaleCompletedAuditConsumer>.Instance);

    AuditEntry? captured = null;
    await writer.WriteAsync(Arg.Do<AuditEntry>(e => captured = e), Arg.Any<CancellationToken>());

    await consumer.Consume(Context(message));

    captured.Should().NotBeNull();
    captured!.Action.Should().Be(AuditActionType.SaleCompleted);
    captured.EntityName.Should().Be(AuditEntityType.Sale);
    captured.EntityId.Should().Be(message.SaleId);
    captured.BusinessId.Value.Should().Be(message.BusinessId);
    captured.CorrelationId.Should().Be(message.CorrelationId);
  }

  [Fact]
  public async Task SaleCompletedAuditConsumer_ShouldBeIdempotent_WhenMessageIsDuplicated()
  {
    var message = SaleCompleted();
    var writer = Substitute.For<IAuditLogWriter>();
    var consumer = new SaleCompletedAuditConsumer(
      writer,
      InboxStore(message.EventId, nameof(SaleCompletedAuditConsumer), alreadyProcessed: true),
      Clock(),
      NullLogger<SaleCompletedAuditConsumer>.Instance);

    await consumer.Consume(Context(message));

    await writer.DidNotReceive().WriteAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task SaleFailedAuditConsumer_ShouldWriteAuditEntry_WithReasonInDescription()
  {
    var message = SaleFailed();
    var writer = Substitute.For<IAuditLogWriter>();
    var consumer = new SaleFailedAuditConsumer(
      writer,
      InboxStore(message.EventId, nameof(SaleFailedAuditConsumer), alreadyProcessed: false),
      Clock(),
      NullLogger<SaleFailedAuditConsumer>.Instance);

    AuditEntry? captured = null;
    await writer.WriteAsync(Arg.Do<AuditEntry>(e => captured = e), Arg.Any<CancellationToken>());

    await consumer.Consume(Context(message));

    captured.Should().NotBeNull();
    captured!.Action.Should().Be(AuditActionType.SaleFailed);
    captured.EntityName.Should().Be(AuditEntityType.Sale);
    captured.Description.Should().Contain(message.Reason);
  }

  [Fact]
  public async Task PaymentRegisteredAuditConsumer_ShouldWriteAuditEntry_WithPaymentId()
  {
    var message = PaymentRegistered();
    var writer = Substitute.For<IAuditLogWriter>();
    var consumer = new PaymentRegisteredAuditConsumer(
      writer,
      InboxStore(message.EventId, nameof(PaymentRegisteredAuditConsumer), alreadyProcessed: false),
      Clock(),
      NullLogger<PaymentRegisteredAuditConsumer>.Instance);

    AuditEntry? captured = null;
    await writer.WriteAsync(Arg.Do<AuditEntry>(e => captured = e), Arg.Any<CancellationToken>());

    await consumer.Consume(Context(message));

    captured.Should().NotBeNull();
    captured!.Action.Should().Be(AuditActionType.PaymentRegistered);
    captured.EntityName.Should().Be(AuditEntityType.Payment);
    captured.EntityId.Should().Be(message.PaymentId);
  }

  [Fact]
  public async Task InvoiceGeneratedAuditConsumer_ShouldWriteAuditEntry_WithInvoiceId()
  {
    var message = InvoiceGenerated();
    var writer = Substitute.For<IAuditLogWriter>();
    var consumer = new InvoiceGeneratedAuditConsumer(
      writer,
      InboxStore(message.EventId, nameof(InvoiceGeneratedAuditConsumer), alreadyProcessed: false),
      Clock(),
      NullLogger<InvoiceGeneratedAuditConsumer>.Instance);

    AuditEntry? captured = null;
    await writer.WriteAsync(Arg.Do<AuditEntry>(e => captured = e), Arg.Any<CancellationToken>());

    await consumer.Consume(Context(message));

    captured.Should().NotBeNull();
    captured!.Action.Should().Be(AuditActionType.InvoiceGenerated);
    captured.EntityName.Should().Be(AuditEntityType.Invoice);
    captured.EntityId.Should().Be(message.InvoiceId);
  }

  [Fact]
  public async Task PurchaseReceivedAuditConsumer_ShouldWriteAuditEntry_WithPurchaseId()
  {
    var message = PurchaseReceived();
    var writer = Substitute.For<IAuditLogWriter>();
    var consumer = new PurchaseReceivedAuditConsumer(
      writer,
      InboxStore(message.EventId, nameof(PurchaseReceivedAuditConsumer), alreadyProcessed: false),
      Clock(),
      NullLogger<PurchaseReceivedAuditConsumer>.Instance);

    AuditEntry? captured = null;
    await writer.WriteAsync(Arg.Do<AuditEntry>(e => captured = e), Arg.Any<CancellationToken>());

    await consumer.Consume(Context(message));

    captured.Should().NotBeNull();
    captured!.Action.Should().Be(AuditActionType.PurchaseReceived);
    captured.EntityName.Should().Be(AuditEntityType.Purchase);
    captured.EntityId.Should().Be(message.PurchaseId);
  }

  [Fact]
  public async Task InventoryAdjustedAuditConsumer_ShouldWriteAuditEntry_WithNullUserId()
  {
    var message = InventoryAdjusted();
    var writer = Substitute.For<IAuditLogWriter>();
    var consumer = new InventoryAdjustedAuditConsumer(
      writer,
      InboxStore(message.EventId, nameof(InventoryAdjustedAuditConsumer), alreadyProcessed: false),
      Clock(),
      NullLogger<InventoryAdjustedAuditConsumer>.Instance);

    AuditEntry? captured = null;
    await writer.WriteAsync(Arg.Do<AuditEntry>(e => captured = e), Arg.Any<CancellationToken>());

    await consumer.Consume(Context(message));

    captured.Should().NotBeNull();
    captured!.Action.Should().Be(AuditActionType.InventoryAdjusted);
    captured.EntityName.Should().Be(AuditEntityType.Inventory);
    captured.UserId.Should().BeNull();
    captured.EntityId.Should().Be(message.ProductId);
  }

  [Fact]
  public async Task InventoryAdjustedAuditConsumer_ShouldBeIdempotent_WhenMessageIsDuplicated()
  {
    var message = InventoryAdjusted();
    var writer = Substitute.For<IAuditLogWriter>();
    var consumer = new InventoryAdjustedAuditConsumer(
      writer,
      InboxStore(message.EventId, nameof(InventoryAdjustedAuditConsumer), alreadyProcessed: true),
      Clock(),
      NullLogger<InventoryAdjustedAuditConsumer>.Instance);

    await consumer.Consume(Context(message));

    await writer.DidNotReceive().WriteAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>());
  }

  private static SaleCompletedEventV1 SaleCompleted()
    => new(
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      500,
      Now);

  private static SaleFailedEventV1 SaleFailed()
    => new(
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      "insufficient stock",
      Now);

  private static PaymentRegisteredEventV1 PaymentRegistered()
    => new(
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      1500,
      "Cash",
      Now);

  private static InvoiceGeneratedIntegrationEventV1 InvoiceGenerated()
    => new(
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Now);

  private static PurchaseReceivedEventV1 PurchaseReceived()
    => new(
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      [new PurchaseItemV1(Guid.NewGuid(), 10, 50, 500)],
      500,
      Now);

  private static InventoryAdjustedEventV1 InventoryAdjusted()
    => new(
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      5,
      10,
      Now);

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
}

#pragma warning restore CA1707
