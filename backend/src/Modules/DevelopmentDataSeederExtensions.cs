using Microsoft.Extensions.DependencyInjection;
using SaasCommerce.Modules.Identity.Infrastructure.Development;

namespace SaasCommerce.Modules;

public static class DevelopmentDataSeederExtensions
{
  public static Task SeedDevelopmentDataAsync(
    this IServiceProvider serviceProvider,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(serviceProvider);

    return SeedDevelopmentDataCoreAsync(serviceProvider, cancellationToken);
  }

  private static async Task SeedDevelopmentDataCoreAsync(
    IServiceProvider serviceProvider,
    CancellationToken cancellationToken)
  {
    using var scope = serviceProvider.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<DevelopmentDataSeeder>();

    await seeder.SeedAsync(cancellationToken);
  }
}
