using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks;
using SaasCommerce.BuildingBlocks.Infrastructure.Messaging.Inbox;
using SaasCommerce.BuildingBlocks.Infrastructure.Messaging.Outbox;
using SaasCommerce.BuildingBlocks.Infrastructure.Messaging.Sagas.Sales;

namespace SaasCommerce.BuildingBlocks.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
  public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

  public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

  public DbSet<SaleSagaState> SaleSagaStates => Set<SaleSagaState>();

  /// <summary>
  /// Subscription plans are configured via module IEntityTypeConfiguration.
  /// Using dynamic Set<T>() to avoid circular dependency issues.
  /// </summary>
  public DbSet<dynamic> SubscriptionPlans => Set<dynamic>();

  /// <summary>
  /// Business subscriptions are configured via module IEntityTypeConfiguration.
  /// Using dynamic Set<T>() to avoid circular dependency issues.
  /// </summary>
  public DbSet<dynamic> BusinessSubscriptions => Set<dynamic>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    ArgumentNullException.ThrowIfNull(modelBuilder);

    modelBuilder.ApplyConfigurationsFromAssembly(BuildingBlocksAssemblyReference.Assembly);
    ApplyModuleConfigurations(modelBuilder);

    base.OnModelCreating(modelBuilder);
  }

  private static void ApplyModuleConfigurations(ModelBuilder modelBuilder)
  {
    var moduleAssemblies = AppDomain.CurrentDomain
      .GetAssemblies()
      .Where(assembly => assembly.GetName().Name == "SaasCommerce.Modules");

    foreach (var assembly in moduleAssemblies)
    {
      modelBuilder.ApplyConfigurationsFromAssembly(assembly);
    }
  }
}
