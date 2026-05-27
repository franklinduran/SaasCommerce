#pragma warning disable CA1707

using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Catalog.Domain;
using SaasCommerce.Modules.Customers.Domain;
using SaasCommerce.Modules.Development;
using SaasCommerce.Modules.Purchasing.Domain;

namespace SaasCommerce.Api.Tests;

public sealed class DemoDataSeederTests
{
  private static readonly Guid AdminUserId = Guid.Parse("44444444-4444-4444-4444-444444444444");

  [Fact]
  public async Task DemoDataSeeder_ShouldSeedCorrectEntities_WhenEnabled()
  {
    using var factory = CreateFactory();
    using var scope = factory.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    var productCount = await dbContext.Set<Product>().CountAsync();
    var customerCount = await dbContext.Set<Customer>().CountAsync();
    var supplierCount = await dbContext.Set<Supplier>().CountAsync();

    productCount.Should().Be(20);
    customerCount.Should().Be(5);
    supplierCount.Should().Be(3);
  }

  [Fact]
  public async Task DemoDataSeeder_ShouldBeIdempotent_WhenCalledTwice()
  {
    using var factory = CreateFactory();
    using var scope = factory.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // First call already happened during app startup in Development mode.
    // Run a second call explicitly to verify idempotency.
    await DemoDataSeeder.SeedAsync(dbContext, AdminUserId, DateTimeOffset.UtcNow);

    var productCount = await dbContext.Set<Product>().CountAsync();
    var customerCount = await dbContext.Set<Customer>().CountAsync();
    var supplierCount = await dbContext.Set<Supplier>().CountAsync();

    productCount.Should().Be(20, "second seed call must not insert duplicate products");
    customerCount.Should().Be(5, "second seed call must not insert duplicate customers");
    supplierCount.Should().Be(3, "second seed call must not insert duplicate suppliers");
  }

  [Fact]
  public async Task DemoDataSeeder_ShouldSkipProducts_WhenSeedDemoDataEnabledIsFalse()
  {
    using var factory = CreateFactory(seedDemoDataEnabled: false);
    using var scope = factory.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    var productCount = await dbContext.Set<Product>().CountAsync();
    var customerCount = await dbContext.Set<Customer>().CountAsync();
    var supplierCount = await dbContext.Set<Supplier>().CountAsync();

    productCount.Should().Be(0, "demo products must be skipped when SeedDemoData:Enabled=false");
    customerCount.Should().Be(0, "demo customers must be skipped when SeedDemoData:Enabled=false");
    supplierCount.Should().Be(0, "demo suppliers must be skipped when SeedDemoData:Enabled=false");
  }

  private static WebApplicationFactory<Program> CreateFactory(bool seedDemoDataEnabled = true)
    => new WebApplicationFactory<Program>()
      .WithWebHostBuilder(builder =>
      {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
          configuration.AddInMemoryCollection(new Dictionary<string, string?>
          {
            ["ConnectionStrings:DefaultConnection"] = "",
            ["Database:InMemoryName"] = Guid.NewGuid().ToString("D"),
            ["RabbitMq:UseInMemory"] = "true",
            ["Jwt:Secret"] = "test-secret-with-at-least-32-characters",
            ["Jwt:Issuer"] = "SaasCommerce.Tests",
            ["Jwt:Audience"] = "SaasCommerce.Tests",
            ["Jwt:AccessTokenMinutes"] = "30",
            ["SeedDemoData:Enabled"] = seedDemoDataEnabled.ToString().ToLowerInvariant()
          });
        });
      });
}

#pragma warning restore CA1707
