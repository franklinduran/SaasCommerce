using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace SaasCommerce.BuildingBlocks.Tests;

public sealed class EfInboxStoreTests
{
  [Fact]
  public async Task HasProcessedAsyncShouldReturnFalseWhenMessageDoesNotExist()
  {
    await using var dbContext = CreateDbContext();
    var store = new EfInboxStore(dbContext);

    var hasProcessed = await store.HasProcessedAsync(Guid.NewGuid());

    hasProcessed.Should().BeFalse();
  }

  [Fact]
  public async Task MarkProcessedAsyncShouldPersistInboxMessage()
  {
    await using var dbContext = CreateDbContext();
    var store = new EfInboxStore(dbContext);
    var eventId = Guid.NewGuid();
    var businessId = Guid.NewGuid();
    var processedAt = new DateTimeOffset(2026, 5, 14, 12, 0, 0, TimeSpan.Zero);

    await store.MarkProcessedAsync(eventId, businessId, "TestMessage", processedAt);

    var message = await dbContext.InboxMessages.SingleAsync();
    message.EventId.Should().Be(eventId);
    message.BusinessId.Should().Be(businessId);
    message.Type.Should().Be("TestMessage");
    message.ProcessedAt.Should().Be(processedAt);
    (await store.HasProcessedAsync(eventId)).Should().BeTrue();
  }

  [Fact]
  public async Task MarkProcessedAsyncShouldNotCreateDuplicateForSameEventId()
  {
    await using var dbContext = CreateDbContext();
    var store = new EfInboxStore(dbContext);
    var eventId = Guid.NewGuid();
    var businessId = Guid.NewGuid();

    await store.MarkProcessedAsync(eventId, businessId, "TestMessage", DateTimeOffset.UtcNow);
    await store.MarkProcessedAsync(eventId, businessId, "TestMessage", DateTimeOffset.UtcNow);

    var count = await dbContext.InboxMessages.CountAsync(message => message.EventId == eventId);
    count.Should().Be(1);
  }

  private static AppDbContext CreateDbContext()
  {
    var options = new DbContextOptionsBuilder<AppDbContext>()
      .UseInMemoryDatabase(Guid.NewGuid().ToString("D"))
      .Options;

    return new AppDbContext(options);
  }
}
