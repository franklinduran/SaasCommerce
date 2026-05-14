using SaasCommerce.BuildingBlocks;
using SaasCommerce.BuildingBlocks.Infrastructure.Messaging.Inbox;
using SaasCommerce.BuildingBlocks.Infrastructure.Messaging.Outbox;
using Microsoft.EntityFrameworkCore;

namespace SaasCommerce.BuildingBlocks.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
  public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

  public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    ArgumentNullException.ThrowIfNull(modelBuilder);

    modelBuilder.ApplyConfigurationsFromAssembly(BuildingBlocksAssemblyReference.Assembly);

    base.OnModelCreating(modelBuilder);
  }
}
