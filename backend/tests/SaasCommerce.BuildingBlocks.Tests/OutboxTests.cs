using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Contracts.Events.V1;
using SaasCommerce.BuildingBlocks.Infrastructure.Messaging.Outbox;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;

namespace SaasCommerce.BuildingBlocks.Tests;

public sealed class OutboxTests
{
  private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

  [Fact]
  public async Task OutboxWriterShouldPersistPendingMessageMetadata()
  {
    await using var dbContext = CreateDbContext();
    var clock = Substitute.For<IClock>();
    var createdAt = new DateTimeOffset(2026, 5, 16, 9, 0, 0, TimeSpan.Zero);
    var integrationEvent = CreatePingEvent();
    var writer = new EfOutboxWriter(dbContext, clock);

    clock.UtcNow.Returns(createdAt);

    await writer.AddAsync(integrationEvent);
    await dbContext.SaveChangesAsync();

    var message = await dbContext.OutboxMessages.SingleAsync();
    message.EventId.Should().Be(integrationEvent.EventId);
    message.CorrelationId.Should().Be(integrationEvent.CorrelationId);
    message.BusinessId.Should().Be(integrationEvent.BusinessId);
    message.EventType.Should().Contain(nameof(TechnicalPingIntegrationEventV1));
    message.Payload.Should().Contain(integrationEvent.Message);
    message.Status.Should().Be(OutboxMessageStatus.Pending);
    message.Attempts.Should().Be(0);
    message.CreatedAt.Should().Be(createdAt);
  }

  [Fact]
  public async Task OutboxPublisherShouldPublishPendingMessagesAndMarkPublished()
  {
    await using var dbContext = CreateDbContext();
    var eventBus = Substitute.For<IEventBus>();
    var clock = Substitute.For<IClock>();
    var publishedAt = new DateTimeOffset(2026, 5, 16, 10, 0, 0, TimeSpan.Zero);
    var integrationEvent = CreatePingEvent();
    var message = CreateOutboxMessage(integrationEvent);
    dbContext.OutboxMessages.Add(message);
    await dbContext.SaveChangesAsync();
    var publisher = CreatePublisher(dbContext, eventBus, clock);

    clock.UtcNow.Returns(publishedAt);

    var published = await publisher.PublishPendingAsync();

    published.Should().Be(1);
    await eventBus.Received(1).PublishAsync(
      Arg.Any<object>(),
      typeof(TechnicalPingIntegrationEventV1),
      Arg.Any<CancellationToken>());
    message.Status.Should().Be(OutboxMessageStatus.Published);
    message.Attempts.Should().Be(1);
    message.PublishedAt.Should().Be(publishedAt);
    message.LastError.Should().BeNull();
  }

  [Fact]
  public async Task OutboxPublisherShouldKeepFailedMessagesForRetry()
  {
    await using var dbContext = CreateDbContext();
    var eventBus = Substitute.For<IEventBus>();
    var clock = Substitute.For<IClock>();
    var integrationEvent = CreatePingEvent();
    var message = CreateOutboxMessage(integrationEvent);
    dbContext.OutboxMessages.Add(message);
    await dbContext.SaveChangesAsync();
    var publisher = CreatePublisher(dbContext, eventBus, clock);

    eventBus
      .PublishAsync(Arg.Any<object>(), Arg.Any<Type>(), Arg.Any<CancellationToken>())
      .Returns<Task>(_ => throw new InvalidOperationException("broker offline"));

    var published = await publisher.PublishPendingAsync();

    published.Should().Be(0);
    message.Status.Should().Be(OutboxMessageStatus.Failed);
    message.Attempts.Should().Be(1);
    message.PublishedAt.Should().BeNull();
    message.LastError.Should().Contain("broker offline");
  }

  [Fact]
  public async Task OutboxPublisherHostedServiceShouldPublishPendingMessages()
  {
    await using var dbContext = CreateDbContext();
    var eventBus = Substitute.For<IEventBus>();
    var clock = Substitute.For<IClock>();
    var integrationEvent = CreatePingEvent();
    var message = CreateOutboxMessage(integrationEvent);
    dbContext.OutboxMessages.Add(message);
    await dbContext.SaveChangesAsync();
    await using var provider = new ServiceCollection()
      .AddSingleton(dbContext)
      .AddSingleton(eventBus)
      .AddSingleton(clock)
      .AddSingleton(Options.Create(new OutboxPublisherOptions
      {
        BatchSize = 10,
        PollingIntervalSeconds = 60
      }))
      .AddSingleton<ILogger<OutboxPublisher>>(NullLogger<OutboxPublisher>.Instance)
      .AddSingleton<OutboxPublisher>()
      .BuildServiceProvider(true);
    var service = new OutboxPublisherHostedService(
      provider.GetRequiredService<IServiceScopeFactory>(),
      Options.Create(new OutboxPublisherOptions { BatchSize = 10, PollingIntervalSeconds = 60 }),
      NullLogger<OutboxPublisherHostedService>.Instance);

    eventBus
      .PublishAsync(Arg.Any<object>(), Arg.Any<Type>(), Arg.Any<CancellationToken>())
      .Returns(Task.CompletedTask);
    clock.UtcNow.Returns(integrationEvent.OccurredAt.AddSeconds(1));

    await service.StartAsync(CancellationToken.None);
    await WaitUntilAsync(() => message.Status == OutboxMessageStatus.Published);
    await service.StopAsync(CancellationToken.None);

    message.Status.Should().Be(OutboxMessageStatus.Published);
    message.Attempts.Should().Be(1);
  }

  private static OutboxPublisher CreatePublisher(
    AppDbContext dbContext,
    IEventBus eventBus,
    IClock clock)
    => new(
      dbContext,
      eventBus,
      clock,
      Options.Create(new OutboxPublisherOptions { BatchSize = 10, PollingIntervalSeconds = 1 }),
      NullLogger<OutboxPublisher>.Instance);

  private static OutboxMessage CreateOutboxMessage(TechnicalPingIntegrationEventV1 integrationEvent)
    => new(
      integrationEvent.EventId,
      integrationEvent.CorrelationId,
      integrationEvent.BusinessId,
      typeof(TechnicalPingIntegrationEventV1).AssemblyQualifiedName!,
      JsonSerializer.Serialize(integrationEvent, SerializerOptions),
      integrationEvent.OccurredAt,
      integrationEvent.OccurredAt);

  private static TechnicalPingIntegrationEventV1 CreatePingEvent()
    => new(
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      new DateTimeOffset(2026, 5, 16, 8, 0, 0, TimeSpan.Zero),
      "stage-7-ping");

  private static AppDbContext CreateDbContext()
  {
    var options = new DbContextOptionsBuilder<AppDbContext>()
      .UseInMemoryDatabase(Guid.NewGuid().ToString("D"))
      .Options;

    return new AppDbContext(options);
  }

  private static async Task WaitUntilAsync(Func<bool> condition)
  {
    using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));

    while (!condition())
    {
      timeout.Token.ThrowIfCancellationRequested();

      await Task.Delay(50, timeout.Token);
    }
  }
}
