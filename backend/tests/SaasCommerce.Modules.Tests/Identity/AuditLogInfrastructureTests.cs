#pragma warning disable CA1707

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Audit;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Identity.Application.Audit;
using SaasCommerce.Modules.Identity.Domain;
using SaasCommerce.Modules.Identity.Infrastructure.Persistence;
using SaasCommerce.Modules.Tenancy.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Tests.Identity;

public sealed class AuditLogInfrastructureTests
{
  private static readonly DateTimeOffset Now = new(2026, 5, 19, 12, 0, 0, TimeSpan.Zero);

  // ── EfAuditLogWriter ─────────────────────────────────────────────────────

  [Fact]
  public async Task EfAuditLogWriter_ShouldPersistAuditLog_WhenEntryIsValid()
  {
    await using var dbContext = CreateDbContext();
    var businessId = new BusinessId(Guid.NewGuid());
    var clock = new FixedClock();
    var writer = new EfAuditLogWriter(dbContext, clock);
    var entry = new AuditEntry(businessId, Guid.NewGuid(), "sale.cancelled", "Sale", Guid.NewGuid(), "Test audit log");

    await writer.WriteAsync(entry);

    dbContext.Set<AuditLog>().Should().ContainSingle();
    var log = dbContext.Set<AuditLog>().Single();
    log.BusinessId.Should().Be(businessId);
    log.Action.Should().Be("sale.cancelled");
    log.EntityName.Should().Be("Sale");
  }

  [Fact]
  public async Task EfAuditLogWriter_ShouldThrow_WhenEntryIsNull()
  {
    await using var dbContext = CreateDbContext();
    var writer = new EfAuditLogWriter(dbContext, new FixedClock());

    var act = async () => await writer.WriteAsync(null!);

    await act.Should().ThrowAsync<ArgumentNullException>();
  }

  // ── EfAuditLogReadRepository ─────────────────────────────────────────────

  [Fact]
  public async Task EfAuditLogReadRepository_ShouldReturnLogs_WhenBusinessMatches()
  {
    await using var dbContext = CreateDbContext();
    var businessId = new BusinessId(Guid.NewGuid());
    var otherBusinessId = new BusinessId(Guid.NewGuid());
    var clock = new FixedClock();

    await SeedAuditLogAsync(dbContext, businessId, clock, "sale.cancelled", "Sale");
    await SeedAuditLogAsync(dbContext, otherBusinessId, clock, "user.created", "User");

    var repo = new EfAuditLogReadRepository(dbContext);
    var result = await repo.GetAuditLogsAsync(businessId, new AuditLogCriteria(null, null, null, null, 1, 50));

    result.Items.Should().ContainSingle();
    result.TotalItems.Should().Be(1);
    result.Items.First().Action.Should().Be("sale.cancelled");
  }

  [Fact]
  public async Task EfAuditLogReadRepository_ShouldFilterByAction()
  {
    await using var dbContext = CreateDbContext();
    var businessId = new BusinessId(Guid.NewGuid());
    var clock = new FixedClock();

    await SeedAuditLogAsync(dbContext, businessId, clock, "sale.cancelled", "Sale");
    await SeedAuditLogAsync(dbContext, businessId, clock, "user.disabled", "User");

    var repo = new EfAuditLogReadRepository(dbContext);
    var result = await repo.GetAuditLogsAsync(
      businessId,
      new AuditLogCriteria(null, null, "sale.cancelled", null, 1, 50));

    result.Items.Should().ContainSingle();
    result.Items.First().Action.Should().Be("sale.cancelled");
  }

  [Fact]
  public async Task EfAuditLogReadRepository_ShouldFilterByEntityName()
  {
    await using var dbContext = CreateDbContext();
    var businessId = new BusinessId(Guid.NewGuid());
    var clock = new FixedClock();

    await SeedAuditLogAsync(dbContext, businessId, clock, "sale.cancelled", "Sale");
    await SeedAuditLogAsync(dbContext, businessId, clock, "user.disabled", "User");

    var repo = new EfAuditLogReadRepository(dbContext);
    var result = await repo.GetAuditLogsAsync(
      businessId,
      new AuditLogCriteria(null, null, null, "Sale", 1, 50));

    result.Items.Should().ContainSingle();
    result.Items.First().EntityName.Should().Be("Sale");
  }

  [Fact]
  public async Task EfAuditLogReadRepository_ShouldFilterByUserId()
  {
    await using var dbContext = CreateDbContext();
    var businessId = new BusinessId(Guid.NewGuid());
    var userId = Guid.NewGuid();
    var clock = new FixedClock();

    await SeedAuditLogAsync(dbContext, businessId, clock, "sale.cancelled", "Sale", userId);
    await SeedAuditLogAsync(dbContext, businessId, clock, "user.disabled", "User", Guid.NewGuid());

    var repo = new EfAuditLogReadRepository(dbContext);
    var result = await repo.GetAuditLogsAsync(
      businessId,
      new AuditLogCriteria(null, userId, null, null, 1, 50));

    result.Items.Should().ContainSingle();
    result.Items.First().UserId.Should().Be(userId);
  }

  [Fact]
  public async Task EfAuditLogReadRepository_ShouldFilterByDateFrom()
  {
    await using var dbContext = CreateDbContext();
    var businessId = new BusinessId(Guid.NewGuid());
    var clock = new FixedClock();
    var dateFrom = Now.AddDays(-1);

    var oldEntry = new AuditEntry(businessId, null, "old.action", "Sale", null);
    var recentEntry = new AuditEntry(businessId, null, "new.action", "Sale", null);
    var oldLog = new AuditLog(Guid.NewGuid(), oldEntry, Now.AddDays(-2));
    var recentLog = new AuditLog(Guid.NewGuid(), recentEntry, Now);

    dbContext.Set<AuditLog>().AddRange(oldLog, recentLog);
    await dbContext.SaveChangesAsync();

    var repo = new EfAuditLogReadRepository(dbContext);
    var result = await repo.GetAuditLogsAsync(
      businessId,
      new AuditLogCriteria(dateFrom, null, null, null, 1, 50));

    result.Items.Should().ContainSingle();
    result.Items.First().Action.Should().Be("new.action");
  }

  [Fact]
  public async Task EfAuditLogReadRepository_ShouldResolvUserFullName_WhenUserExists()
  {
    await using var dbContext = CreateDbContext();
    var businessId = new BusinessId(Guid.NewGuid());
    var branchId = new BranchId(Guid.NewGuid());
    var userId = Guid.NewGuid();
    var clock = new FixedClock();

    // Seed a real business with a user
    var business = new Business(businessId, "Test Business", Now);
    business.AddBranch(branchId, "Main", Now, isMain: true);
    var user = new User(userId, businessId, branchId, "Juan Perez", "juan@test.com", "hash", Now);
    dbContext.Add(business);
    dbContext.Add(user);
    await dbContext.SaveChangesAsync();

    await SeedAuditLogAsync(dbContext, businessId, clock, "sale.cancelled", "Sale", userId);

    var repo = new EfAuditLogReadRepository(dbContext);
    var result = await repo.GetAuditLogsAsync(
      businessId,
      new AuditLogCriteria(null, null, null, null, 1, 50));

    result.Items.Should().ContainSingle();
    result.Items.First().UserFullName.Should().Be("Juan Perez");
  }

  [Fact]
  public async Task EfAuditLogReadRepository_ShouldReturnPaginationMetadata()
  {
    await using var dbContext = CreateDbContext();
    var businessId = new BusinessId(Guid.NewGuid());
    var clock = new FixedClock();

    for (var i = 0; i < 5; i++)
    {
      await SeedAuditLogAsync(dbContext, businessId, clock, $"action.{i}", "Sale");
    }

    var repo = new EfAuditLogReadRepository(dbContext);

    // Page 1 of 2 (pageSize=3, totalItems=5)
    var result = await repo.GetAuditLogsAsync(
      businessId,
      new AuditLogCriteria(null, null, null, null, 1, 3));

    result.TotalItems.Should().Be(5);
    result.TotalPages.Should().Be(2);
    result.HasNextPage.Should().BeTrue();
    result.HasPreviousPage.Should().BeFalse();
    result.Items.Should().HaveCount(3);
  }

  [Fact]
  public async Task EfAuditLogReadRepository_ShouldReturnEmptyList_WhenNoLogs()
  {
    await using var dbContext = CreateDbContext();
    var businessId = new BusinessId(Guid.NewGuid());

    var repo = new EfAuditLogReadRepository(dbContext);
    var result = await repo.GetAuditLogsAsync(
      businessId,
      new AuditLogCriteria(null, null, null, null, 1, 50));

    result.Items.Should().BeEmpty();
    result.TotalItems.Should().Be(0);
    result.TotalPages.Should().Be(0);
    result.HasNextPage.Should().BeFalse();
    result.HasPreviousPage.Should().BeFalse();
  }

  // ── Helpers ──────────────────────────────────────────────────────────────

  private static async Task SeedAuditLogAsync(
    AppDbContext dbContext,
    BusinessId businessId,
    FixedClock clock,
    string action,
    string entityName,
    Guid? userId = null)
  {
    var entry = new AuditEntry(businessId, userId, action, entityName, null);
    var log = new AuditLog(Guid.NewGuid(), entry, clock.UtcNow);
    dbContext.Set<AuditLog>().Add(log);
    await dbContext.SaveChangesAsync();
  }

  private static AppDbContext CreateDbContext()
  {
    var options = new DbContextOptionsBuilder<AppDbContext>()
      .UseInMemoryDatabase(Guid.NewGuid().ToString("D"))
      .Options;

    return new AppDbContext(options);
  }

  private sealed class FixedClock : IClock
  {
    public DateTimeOffset UtcNow => Now;
  }
}

#pragma warning restore CA1707
