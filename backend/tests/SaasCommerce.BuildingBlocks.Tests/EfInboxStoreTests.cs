using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;

namespace SaasCommerce.BuildingBlocks.Tests;

public sealed class EfInboxStoreTests
{
  [Fact]
  public async Task HasProcessedAsyncShouldReturnFalseWhenMessageDoesNotExist()
  {
    await using var dbContext = CreateDbContext();
    var store = new EfInboxStore(dbContext);

    var hasProcessed = await store.HasProcessedAsync(Guid.NewGuid(), "TestConsumer");

    hasProcessed.Should().BeFalse();
  }

  [Fact]
  public async Task MarkProcessedAsyncShouldPersistInboxMessage()
  {
    await using var dbContext = CreateDbContext();
    var store = new EfInboxStore(dbContext);
    var eventId = Guid.NewGuid();
    var businessId = Guid.NewGuid();
    var correlationId = Guid.NewGuid();
    var processedAt = new DateTimeOffset(2026, 5, 14, 12, 0, 0, TimeSpan.Zero);

    await store.MarkProcessedAsync(eventId, "TestConsumer", businessId, correlationId, processedAt);

    var message = await dbContext.InboxMessages.SingleAsync();
    message.EventId.Should().Be(eventId);
    message.ConsumerName.Should().Be("TestConsumer");
    message.BusinessId.Should().Be(businessId);
    message.CorrelationId.Should().Be(correlationId);
    message.ProcessedAt.Should().Be(processedAt);
    (await store.HasProcessedAsync(eventId, "TestConsumer")).Should().BeTrue();
  }

  [Fact]
  public async Task MarkProcessedAsyncShouldNotCreateDuplicateForSameEventAndConsumer()
  {
    await using var dbContext = CreateDbContext();
    var store = new EfInboxStore(dbContext);
    var eventId = Guid.NewGuid();
    var businessId = Guid.NewGuid();
    var correlationId = Guid.NewGuid();

    await store.MarkProcessedAsync(eventId, "TestConsumer", businessId, correlationId, DateTimeOffset.UtcNow);
    await store.MarkProcessedAsync(eventId, "TestConsumer", businessId, correlationId, DateTimeOffset.UtcNow);

    var count = await dbContext.InboxMessages.CountAsync(message => message.EventId == eventId);
    count.Should().Be(1);
  }

  [Fact]
  public async Task SameEventIdCanBeProcessedByDifferentConsumers()
  {
    await using var dbContext = CreateDbContext();
    var store = new EfInboxStore(dbContext);
    var eventId = Guid.NewGuid();
    var businessId = Guid.NewGuid();
    var correlationId = Guid.NewGuid();

    await store.MarkProcessedAsync(eventId, "FirstConsumer", businessId, correlationId, DateTimeOffset.UtcNow);
    await store.MarkProcessedAsync(eventId, "SecondConsumer", businessId, correlationId, DateTimeOffset.UtcNow);

    (await dbContext.InboxMessages.CountAsync(message => message.EventId == eventId)).Should().Be(2);
    (await store.HasProcessedAsync(eventId, "FirstConsumer")).Should().BeTrue();
    (await store.HasProcessedAsync(eventId, "SecondConsumer")).Should().BeTrue();
  }

  private static AppDbContext CreateDbContext()
  {
    var options = new DbContextOptionsBuilder<AppDbContext>()
      .UseInMemoryDatabase(Guid.NewGuid().ToString("D"))
      .Options;

    return new AppDbContext(options);
  }
}
