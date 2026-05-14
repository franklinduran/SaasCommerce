using SaasCommerce.Infrastructure.Messaging.Outbox;
using Microsoft.EntityFrameworkCore;

namespace SaasCommerce.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
  public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    ArgumentNullException.ThrowIfNull(modelBuilder);

    modelBuilder.ApplyConfigurationsFromAssembly(InfrastructureAssemblyReference.Assembly);

    base.OnModelCreating(modelBuilder);
  }
}
