#pragma warning disable CA1707

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Billing.Application.Abstractions;
using SaasCommerce.Modules.Tenancy.Application.Branches;
using SaasCommerce.Modules.Tenancy.Domain;
using SaasCommerce.Modules.Tenancy.Infrastructure.Persistence;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Tests;

public sealed class BranchHandlerCoverageTests
{
  private static readonly DateTimeOffset Now = new(2026, 5, 25, 12, 0, 0, TimeSpan.Zero);

  [Fact]
  public async Task BranchRepository_ShouldFilterExistCountAndAddBranches()
  {
    await using var dbContext = CreateDbContext();
    var businessId = new BusinessId(Guid.NewGuid());
    var main = Branch(businessId, "Principal", "MAIN", isMain: true);
    var secondary = Branch(businessId, "Secundaria", "SEC", isMain: false);
    secondary.Deactivate(false, Now.AddMinutes(1));
    dbContext.AddRange(main, secondary, Branch(new BusinessId(Guid.NewGuid()), "Otra", "OTH", false));
    await dbContext.SaveChangesAsync();
    var repository = new EfBranchRepository(dbContext);

    var all = await repository.ListAsync(businessId, null);
    var active = await repository.ListAsync(businessId, true);
    var inactive = await repository.ListAsync(businessId, false);
    var existsCode = await repository.ExistsByCodeAsync(businessId, "MAIN", null);
    var existsName = await repository.ExistsByNameAsync(businessId, " Principal ", null);
    var excludedCode = await repository.ExistsByCodeAsync(businessId, "MAIN", main.Id);
    var countActive = await repository.CountActiveAsync(businessId);
    var hasMain = await repository.HasMainBranchAsync(businessId, null);
    var loaded = await repository.GetByIdAsync(businessId, main.Id);
    await repository.AddAsync(Branch(businessId, "Tercera", "TER", false));
    await dbContext.SaveChangesAsync();

    all.Should().HaveCount(2);
    active.Should().ContainSingle(b => b.Name == "Principal");
    inactive.Should().ContainSingle(b => b.Name == "Secundaria");
    existsCode.Should().BeTrue();
    existsName.Should().BeTrue();
    excludedCode.Should().BeFalse();
    countActive.Should().Be(1);
    hasMain.Should().BeTrue();
    loaded.Should().NotBeNull();
  }

  [Fact]
  public async Task BranchHandlers_ShouldCreateUpdateDeactivateAndActivate()
  {
    await using var dbContext = CreateDbContext();
    var currentUser = TestCurrentUser.Create();
    var repository = new EfBranchRepository(dbContext);
    var unitOfWork = new EfUnitOfWork(dbContext);
    var create = new CreateBranchHandler(repository, currentUser, new FixedClock(), unitOfWork, new AllowBranchLimitChecker());

    var created = await create.Handle(new CreateBranchCommand("Principal", " main ", " Calle 1 ", "809", true));

    created.IsSuccess.Should().BeTrue();
    created.Value.Code.Should().Be("MAIN");
    created.Value.Address.Should().Be("Calle 1");

    var update = new UpdateBranchHandler(repository, currentUser, new FixedClock(), unitOfWork);
    var updated = await update.Handle(new UpdateBranchCommand(created.Value.Id, "Principal Norte", "Calle 2", null));

    updated.IsSuccess.Should().BeTrue();
    updated.Value.Name.Should().Be("Principal Norte");

    await create.Handle(new CreateBranchCommand("Secundaria", "SEC", null, null, false));
    var deactivate = new DeactivateBranchHandler(repository, currentUser, new FixedClock(), unitOfWork);
    var deactivated = await deactivate.Handle(new DeactivateBranchCommand(created.Value.Id));

    deactivated.IsSuccess.Should().BeTrue();
    deactivated.Value.IsActive.Should().BeFalse();

    var activate = new ActivateBranchHandler(repository, currentUser, new FixedClock(), unitOfWork);
    var activated = await activate.Handle(new ActivateBranchCommand(created.Value.Id));

    activated.IsSuccess.Should().BeTrue();
    activated.Value.IsActive.Should().BeTrue();
  }

  [Fact]
  public async Task BranchHandlers_ShouldReturnFailures()
  {
    await using var dbContext = CreateDbContext();
    var currentUser = TestCurrentUser.Create();
    var repository = new EfBranchRepository(dbContext);
    var unitOfWork = new EfUnitOfWork(dbContext);
    var create = new CreateBranchHandler(repository, currentUser, new FixedClock(), unitOfWork, new AllowBranchLimitChecker());
    var first = await create.Handle(new CreateBranchCommand("Principal", "MAIN", null, null, true));

    (await create.Handle(new CreateBranchCommand("Otra", "MAIN", null, null, false)))
      .Error.Should().Be(BranchErrors.CodeAlreadyExists);
    (await create.Handle(new CreateBranchCommand("Principal", "ALT", null, null, false)))
      .Error.Should().Be(BranchErrors.NameAlreadyExists);
    (await create.Handle(new CreateBranchCommand("Nueva main", "NM", null, null, true)))
      .Error.Should().Be(BranchErrors.MainBranchAlreadyExists);

    var limited = await new CreateBranchHandler(repository, currentUser, new FixedClock(), unitOfWork, new DenyBranchLimitChecker())
      .Handle(new CreateBranchCommand("Limitada", "LIM", null, null, false));
    limited.Error.Code.Should().Be("subscription.limit_reached");

    var noContext = TestCurrentUser.Create() with { BusinessId = null };
    (await new CreateBranchHandler(repository, noContext, new FixedClock(), unitOfWork, new AllowBranchLimitChecker())
      .Handle(new CreateBranchCommand("No context", "NC", null, null, false)))
      .Error.Should().Be(BranchErrors.UserContextRequired);

    var update = new UpdateBranchHandler(repository, currentUser, new FixedClock(), unitOfWork);
    (await update.Handle(new UpdateBranchCommand(Guid.NewGuid(), "Missing", null, null)))
      .Error.Should().Be(BranchErrors.NotFound);
    (await update.Handle(new UpdateBranchCommand(first.Value.Id, "Principal", null, null)))
      .IsSuccess.Should().BeTrue();

    var activate = new ActivateBranchHandler(repository, currentUser, new FixedClock(), unitOfWork);
    (await activate.Handle(new ActivateBranchCommand(first.Value.Id)))
      .Error.Should().Be(BranchErrors.AlreadyActive);

    var deactivate = new DeactivateBranchHandler(repository, currentUser, new FixedClock(), unitOfWork);
    (await deactivate.Handle(new DeactivateBranchCommand(Guid.NewGuid())))
      .Error.Should().Be(BranchErrors.NotFound);
    (await deactivate.Handle(new DeactivateBranchCommand(first.Value.Id)))
      .Error.Should().Be(BranchErrors.CannotDeactivateLastActive);
  }

  private static AppDbContext CreateDbContext()
  {
    var options = new DbContextOptionsBuilder<AppDbContext>()
      .UseInMemoryDatabase(Guid.NewGuid().ToString("D"))
      .Options;

    return new AppDbContext(options);
  }

  private static Branch Branch(BusinessId businessId, string name, string code, bool isMain)
    => new(BranchId.New(), businessId, name, code, Now, isMain);

  private sealed class FixedClock : IClock
  {
    public DateTimeOffset UtcNow => Now;
  }

  private sealed record TestCurrentUser : ICurrentUserService
  {
    public Guid? UserId { get; init; }

    public Guid? BusinessId { get; init; }

    public Guid? BranchId { get; init; }

    public IReadOnlyCollection<string> Roles { get; init; } = [];

    public bool IsAuthenticated { get; init; }

    public static TestCurrentUser Create()
      => new()
      {
        UserId = Guid.NewGuid(),
        BusinessId = Guid.NewGuid(),
        BranchId = Guid.NewGuid(),
        Roles = ["Admin"],
        IsAuthenticated = true
      };
  }

  private class AllowBranchLimitChecker : ISubscriptionLimitChecker
  {
    private static readonly SubscriptionLimitCheckResult Allowed = new(true, "OK", "Allowed.", 0, 10);

    public virtual Task<SubscriptionLimitCheckResult> CanCreateBranchAsync(BusinessId businessId, CancellationToken cancellationToken = default)
      => Task.FromResult(Allowed);

    public Task<SubscriptionLimitCheckResult> CanCreateUserAsync(BusinessId businessId, CancellationToken cancellationToken = default)
      => Task.FromResult(Allowed);

    public Task<SubscriptionLimitCheckResult> CanCreateProductAsync(BusinessId businessId, CancellationToken cancellationToken = default)
      => Task.FromResult(Allowed);

    public Task<SubscriptionLimitCheckResult> CanCreateSaleAsync(BusinessId businessId, CancellationToken cancellationToken = default)
      => Task.FromResult(Allowed);

    public Task<SubscriptionLimitCheckResult> CanUseInventoryTransfersAsync(BusinessId businessId, CancellationToken cancellationToken = default)
      => Task.FromResult(Allowed);

    public Task<SubscriptionLimitCheckResult> CanUseAdvancedReportsAsync(BusinessId businessId, CancellationToken cancellationToken = default)
      => Task.FromResult(Allowed);

    public Task<SubscriptionLimitCheckResult> CanUseAuditLogsAsync(BusinessId businessId, CancellationToken cancellationToken = default)
      => Task.FromResult(Allowed);
  }

  private sealed class DenyBranchLimitChecker : AllowBranchLimitChecker
  {
    public override Task<SubscriptionLimitCheckResult> CanCreateBranchAsync(BusinessId businessId, CancellationToken cancellationToken = default)
      => Task.FromResult(new SubscriptionLimitCheckResult(false, "LIMIT", "No branches.", 10, 10));
  }
}

#pragma warning restore CA1707
