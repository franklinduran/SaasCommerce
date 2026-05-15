using SaasCommerce.Modules.Identity.Infrastructure.Development;
using Microsoft.Extensions.DependencyInjection;

namespace SaasCommerce.Modules;

public static class DevelopmentDataSeederExtensions
{
  public static async Task SeedDevelopmentDataAsync(
    this IServiceProvider serviceProvider,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(serviceProvider);

    using var scope = serviceProvider.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<DevelopmentDataSeeder>();

    await seeder.SeedAsync(cancellationToken);
  }
}
