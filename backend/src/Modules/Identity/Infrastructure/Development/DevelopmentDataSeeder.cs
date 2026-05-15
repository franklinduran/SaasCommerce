using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Identity.Application.Abstractions;
using SaasCommerce.Modules.Identity.Domain;
using SaasCommerce.Modules.Tenancy.Domain;
using SaasCommerce.SharedKernel.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace SaasCommerce.Modules.Identity.Infrastructure.Development;

public sealed class DevelopmentDataSeeder(
  AppDbContext dbContext,
  IPasswordHasher passwordHasher,
  IClock clock)
{
  public const string AdminEmail = "admin@test.com";
  public const string AdminPassword = "Admin123!";

  public async Task SeedAsync(CancellationToken cancellationToken = default)
  {
    var hasBusiness = await dbContext.Set<Business>()
      .AnyAsync(cancellationToken);

    if (hasBusiness)
    {
      return;
    }

    var now = clock.UtcNow;
    var businessId = new BusinessId(Guid.Parse("11111111-1111-1111-1111-111111111111"));
    var branchId = new BranchId(Guid.Parse("22222222-2222-2222-2222-222222222222"));
    var adminRole = new Role(
      Guid.Parse("33333333-3333-3333-3333-333333333333"),
      businessId,
      "Admin");
    var adminUser = new User(
      Guid.Parse("44444444-4444-4444-4444-444444444444"),
      businessId,
      branchId,
      "Admin",
      AdminEmail,
      passwordHasher.Hash(AdminPassword),
      now);
    var business = new Business(businessId, "Demo Business", now);

    business.AddBranch(branchId, "Main Branch", now, isMain: true);
    adminUser.AddRole(adminRole);

    dbContext.Add(business);
    dbContext.Add(adminRole);
    dbContext.Add(adminUser);

    await dbContext.SaveChangesAsync(cancellationToken);
  }
}
