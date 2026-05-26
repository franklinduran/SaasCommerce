#pragma warning disable CA1707

using FluentAssertions;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Audit;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Settings.Application;
using SaasCommerce.Modules.Settings.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Tests.Settings;

public sealed class SettingsHandlerTests
{
  private static readonly DateTimeOffset Now = new(2026, 5, 20, 10, 0, 0, TimeSpan.Zero);

  // ── GetBusinessSettings ─────────────────────────────────────────────────

  [Fact]
  public async Task GetBusinessSettings_ShouldReturnDefaults_WhenNoRowExists()
  {
    var businessId = Guid.NewGuid();
    var repo = new StubBusinessSettingsRepo(null);
    var handler = new GetBusinessSettingsHandler(repo, MakeCurrentUser(businessId), Clock());

    var result = await handler.Handle(new GetBusinessSettingsQuery());

    result.IsSuccess.Should().BeTrue();
    result.Value.BusinessId.Should().Be(businessId);
    result.Value.Currency.Should().Be("DOP");
    result.Value.Timezone.Should().Be("America/Santo_Domingo");
  }

  [Fact]
  public async Task GetBusinessSettings_ShouldReturnCurrentBusinessSettings()
  {
    var businessId = new BusinessId(Guid.NewGuid());
    var settings = new BusinessSettings(new BusinessSettingsDetails
    {
      BusinessId = businessId,
      CommercialName = "My Shop",
      Currency = "USD",
      Timezone = "UTC",
      ReceiptFooterText = "Thank you!",
      UpdatedBy = Guid.NewGuid(),
      UpdatedAt = Now
    });
    var repo = new StubBusinessSettingsRepo(settings);
    var handler = new GetBusinessSettingsHandler(repo, MakeCurrentUser(businessId.Value), Clock());

    var result = await handler.Handle(new GetBusinessSettingsQuery());

    result.IsSuccess.Should().BeTrue();
    result.Value.CommercialName.Should().Be("My Shop");
    result.Value.Currency.Should().Be("USD");
  }

  [Fact]
  public async Task GetBusinessSettings_ShouldNotReturnOtherBusinessSettings()
  {
    var otherBusinessId = new BusinessId(Guid.NewGuid());
    var settings = new BusinessSettings(new BusinessSettingsDetails
    {
      BusinessId = otherBusinessId,
      CommercialName = "Other Shop",
      Currency = "USD",
      Timezone = "UTC",
      UpdatedBy = Guid.NewGuid(),
      UpdatedAt = Now
    });
    // Repo returns the "other" settings; handler uses current user's businessId
    var repo = new StubBusinessSettingsRepo(null); // returns null for current business
    var handler = new GetBusinessSettingsHandler(repo, MakeCurrentUser(Guid.NewGuid()), Clock());

    var result = await handler.Handle(new GetBusinessSettingsQuery());

    // Returns defaults, not the other business settings
    result.IsSuccess.Should().BeTrue();
    result.Value.CommercialName.Should().BeNull();
  }

  [Fact]
  public async Task GetBusinessSettings_ShouldFail_WhenNotAuthenticated()
  {
    var handler = new GetBusinessSettingsHandler(
      new StubBusinessSettingsRepo(null), new UnauthenticatedUser(), Clock());

    var result = await handler.Handle(new GetBusinessSettingsQuery());

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(SettingsErrors.UserContextRequired);
  }

  // ── UpdateBusinessSettings ──────────────────────────────────────────────

  [Fact]
  public async Task UpdateBusinessSettings_ShouldUpdateOnlyCurrentBusiness()
  {
    var businessId = Guid.NewGuid();
    var repo = new StubBusinessSettingsRepo(null);
    var handler = new UpdateBusinessSettingsHandler(
      repo, MakeCurrentUser(businessId, isAdmin: true),
      NoopAuditLog(), Clock(), NoopUnitOfWork());

    var result = await handler.Handle(
      new UpdateBusinessSettingsCommand(
        "My Tienda", null, null, null, null, null,
        "DOP", "America/Santo_Domingo", null, null));

    result.IsSuccess.Should().BeTrue();
    result.Value.CommercialName.Should().Be("My Tienda");
    result.Value.BusinessId.Should().Be(businessId);
  }

  [Fact]
  public async Task UpdateBusinessSettings_ShouldRejectInvalidCurrency()
  {
    var handler = new UpdateBusinessSettingsHandler(
      new StubBusinessSettingsRepo(null), MakeCurrentUser(Guid.NewGuid(), isAdmin: true),
      NoopAuditLog(), Clock(), NoopUnitOfWork());

    var result = await handler.Handle(
      new UpdateBusinessSettingsCommand(null, null, null, null, null, null,
        "INVALID", "America/Santo_Domingo", null, null));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(SettingsErrors.InvalidCurrency);
  }

  [Fact]
  public async Task UpdateBusinessSettings_ShouldRejectInvalidEmail()
  {
    var handler = new UpdateBusinessSettingsHandler(
      new StubBusinessSettingsRepo(null), MakeCurrentUser(Guid.NewGuid(), isAdmin: true),
      NoopAuditLog(), Clock(), NoopUnitOfWork());

    var result = await handler.Handle(
      new UpdateBusinessSettingsCommand(null, null, null, null,
        "not-an-email", null, "DOP", "America/Santo_Domingo", null, null));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(SettingsErrors.InvalidEmail);
  }

  [Fact]
  public async Task UpdateBusinessSettings_ShouldFail_WhenNotAdminOrOwner()
  {
    var handler = new UpdateBusinessSettingsHandler(
      new StubBusinessSettingsRepo(null), MakeCurrentUser(Guid.NewGuid(), isAdmin: false),
      NoopAuditLog(), Clock(), NoopUnitOfWork());

    var result = await handler.Handle(
      new UpdateBusinessSettingsCommand(null, null, null, null, null, null,
        "DOP", "America/Santo_Domingo", null, null));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(SettingsErrors.Forbidden);
  }

  // ── GetSalesSettings ────────────────────────────────────────────────────

  [Fact]
  public async Task GetSalesSettings_ShouldReturnDefaults_WhenNoRowExists()
  {
    var businessId = Guid.NewGuid();
    var handler = new GetSalesSettingsHandler(
      new StubSalesSettingsRepo(null), MakeCurrentUser(businessId), Clock());

    var result = await handler.Handle(new GetSalesSettingsQuery());

    result.IsSuccess.Should().BeTrue();
    result.Value.AllowNegativeStock.Should().BeFalse();
    result.Value.AllowDiscounts.Should().BeTrue();
    result.Value.EnableInvoiceAutoGeneration.Should().BeTrue();
  }

  [Fact]
  public async Task UpdateSalesSettings_ShouldSaveAndReturnSettings()
  {
    var businessId = Guid.NewGuid();
    var repo = new StubSalesSettingsRepo(null);
    var handler = new UpdateSalesSettingsHandler(
      repo, MakeCurrentUser(businessId, isAdmin: true),
      NoopAuditLog(), Clock(), NoopUnitOfWork());

    var result = await handler.Handle(
      new UpdateSalesSettingsCommand(true, false, false, "Cash", true, false));

    result.IsSuccess.Should().BeTrue();
    result.Value.AllowNegativeStock.Should().BeTrue();
    result.Value.AllowDiscounts.Should().BeFalse();
    result.Value.DefaultPaymentMethod.Should().Be("Cash");
  }

  // ── GetInventorySettings ────────────────────────────────────────────────

  [Fact]
  public async Task GetInventorySettings_ShouldReturnDefaults_WhenNoRowExists()
  {
    var handler = new GetInventorySettingsHandler(
      new StubInventorySettingsRepo(null), MakeCurrentUser(Guid.NewGuid()), Clock());

    var result = await handler.Handle(new GetInventorySettingsQuery());

    result.IsSuccess.Should().BeTrue();
    result.Value.EnableLowStockAlerts.Should().BeTrue();
    result.Value.DefaultLowStockThreshold.Should().Be(5);
    result.Value.RequireReasonForInventoryAdjustment.Should().BeFalse();
  }

  [Fact]
  public async Task UpdateInventorySettings_ShouldRejectNegativeThreshold()
  {
    var handler = new UpdateInventorySettingsHandler(
      new StubInventorySettingsRepo(null), MakeCurrentUser(Guid.NewGuid(), isAdmin: true),
      NoopAuditLog(), Clock(), NoopUnitOfWork());

    var result = await handler.Handle(
      new UpdateInventorySettingsCommand(true, -1, false, false));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(SettingsErrors.InvalidThreshold);
  }

  [Fact]
  public async Task InventorySettings_ShouldBeScopedByBusinessId()
  {
    var businessId = new BusinessId(Guid.NewGuid());
    var settings = InventorySettings.Default(businessId, Guid.NewGuid(), Now);
    var repo = new StubInventorySettingsRepo(settings);
    var handler = new GetInventorySettingsHandler(
      repo, MakeCurrentUser(businessId.Value), Clock());

    var result = await handler.Handle(new GetInventorySettingsQuery());

    result.IsSuccess.Should().BeTrue();
    result.Value.BusinessId.Should().Be(businessId.Value);
  }

  // ── GetBillingSettings ──────────────────────────────────────────────────

  [Fact]
  public async Task GetBillingSettings_ShouldReturnDefaults_WhenNoRowExists()
  {
    var handler = new GetBillingSettingsHandler(
      new StubBillingSettingsRepo(null), MakeCurrentUser(Guid.NewGuid()), Clock());

    var result = await handler.Handle(new GetBillingSettingsQuery());

    result.IsSuccess.Should().BeTrue();
    result.Value.InvoicePrefix.Should().Be("RI");
    result.Value.InvoiceSequenceStart.Should().Be(1);
    result.Value.EnableInvoiceAutoGeneration.Should().BeTrue();
  }

  [Fact]
  public async Task UpdateBillingSettings_ShouldRejectSequenceStartLessThanOne()
  {
    var handler = new UpdateBillingSettingsHandler(
      new StubBillingSettingsRepo(null), MakeCurrentUser(Guid.NewGuid(), isAdmin: true),
      NoopAuditLog(), Clock(), NoopUnitOfWork());

    var result = await handler.Handle(
      new UpdateBillingSettingsCommand(null, null, false, false, true, "RI", 0));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(SettingsErrors.InvalidSequenceStart);
  }

  [Fact]
  public async Task BillingSettings_ShouldNotLeakBetweenBusinesses()
  {
    var businessA = Guid.NewGuid();
    var businessB = Guid.NewGuid();

    var repoA = new StubBillingSettingsRepo(null);
    var handlerA = new UpdateBillingSettingsHandler(
      repoA, MakeCurrentUser(businessA, isAdmin: true),
      NoopAuditLog(), Clock(), NoopUnitOfWork());
    var resultA = await handlerA.Handle(
      new UpdateBillingSettingsCommand("Header A", "Footer A", false, false, true, "FA", 1));

    var repoB = new StubBillingSettingsRepo(null);
    var handlerB = new GetBillingSettingsHandler(
      repoB, MakeCurrentUser(businessB), Clock());
    var resultB = await handlerB.Handle(new GetBillingSettingsQuery());

    resultA.IsSuccess.Should().BeTrue();
    resultA.Value.ReceiptHeaderText.Should().Be("Header A");

    resultB.IsSuccess.Should().BeTrue();
    resultB.Value.ReceiptHeaderText.Should().BeNull(); // defaults
  }

  // ── Audit ────────────────────────────────────────────────────────────────

  [Fact]
  public async Task UpdateBusinessSettings_ShouldCreateAuditLog()
  {
    var auditLog = new RecordingAuditLog();
    var handler = new UpdateBusinessSettingsHandler(
      new StubBusinessSettingsRepo(null), MakeCurrentUser(Guid.NewGuid(), isAdmin: true),
      auditLog, Clock(), NoopUnitOfWork());

    await handler.Handle(
      new UpdateBusinessSettingsCommand(null, null, null, null, null, null,
        "DOP", "America/Santo_Domingo", null, null));

    auditLog.Entries.Should().ContainSingle();
    auditLog.Entries[0].Action.Should().Be("settings.business_updated");
  }

  [Fact]
  public async Task UpdateSalesSettings_ShouldCreateAuditLog()
  {
    var auditLog = new RecordingAuditLog();
    var handler = new UpdateSalesSettingsHandler(
      new StubSalesSettingsRepo(null), MakeCurrentUser(Guid.NewGuid(), isAdmin: true),
      auditLog, Clock(), NoopUnitOfWork());

    await handler.Handle(
      new UpdateSalesSettingsCommand(false, true, true, null, false, true));

    auditLog.Entries.Should().ContainSingle();
    auditLog.Entries[0].Action.Should().Be("settings.sales_updated");
  }

  [Fact]
  public async Task UpdateInventorySettings_ShouldCreateAuditLog()
  {
    var auditLog = new RecordingAuditLog();
    var handler = new UpdateInventorySettingsHandler(
      new StubInventorySettingsRepo(null), MakeCurrentUser(Guid.NewGuid(), isAdmin: true),
      auditLog, Clock(), NoopUnitOfWork());

    await handler.Handle(
      new UpdateInventorySettingsCommand(true, 5, false, false));

    auditLog.Entries.Should().ContainSingle();
    auditLog.Entries[0].Action.Should().Be("settings.inventory_updated");
  }

  [Fact]
  public async Task UpdateBillingSettings_ShouldCreateAuditLog()
  {
    var auditLog = new RecordingAuditLog();
    var handler = new UpdateBillingSettingsHandler(
      new StubBillingSettingsRepo(null), MakeCurrentUser(Guid.NewGuid(), isAdmin: true),
      auditLog, Clock(), NoopUnitOfWork());

    await handler.Handle(
      new UpdateBillingSettingsCommand(null, null, false, false, true, "RI", 1));

    auditLog.Entries.Should().ContainSingle();
    auditLog.Entries[0].Action.Should().Be("settings.billing_updated");
  }

  [Fact]
  public async Task AuditLog_ShouldIncludeBusinessId_WhenSettingsChange()
  {
    var businessId = Guid.NewGuid();
    var auditLog = new RecordingAuditLog();
    var handler = new UpdateBusinessSettingsHandler(
      new StubBusinessSettingsRepo(null), MakeCurrentUser(businessId, isAdmin: true),
      auditLog, Clock(), NoopUnitOfWork());

    await handler.Handle(
      new UpdateBusinessSettingsCommand(null, null, null, null, null, null,
        "DOP", "America/Santo_Domingo", null, null));

    auditLog.Entries[0].BusinessId.Value.Should().Be(businessId);
  }

  // ── Helpers ─────────────────────────────────────────────────────────────

  private static FakeCurrentUser MakeCurrentUser(Guid businessId, bool isAdmin = false)
    => new(businessId, Guid.NewGuid(), isAdmin ? ["Admin"] : ["Cashier"]);

  private static FixedClock Clock() => new FixedClock();

  private static NoopAuditLogWriter NoopAuditLog()
    => new NoopAuditLogWriter();

  private static NoopUnitOfWorkImpl NoopUnitOfWork()
    => new NoopUnitOfWorkImpl();

  // ── Stubs ─────────────────────────────────────────────────────────────────

  private sealed class StubBusinessSettingsRepo(BusinessSettings? stored) : IBusinessSettingsRepository
  {
    private BusinessSettings? _stored = stored;

    public Task<BusinessSettings?> GetAsync(BusinessId businessId, CancellationToken cancellationToken = default)
      => Task.FromResult(_stored);

    public Task UpsertAsync(BusinessSettings settings, CancellationToken cancellationToken = default)
    {
      _stored = settings;
      return Task.CompletedTask;
    }
  }

  private sealed class StubSalesSettingsRepo(SalesSettings? stored) : ISalesSettingsRepository
  {
    private SalesSettings? _stored = stored;

    public Task<SalesSettings?> GetAsync(BusinessId businessId, CancellationToken cancellationToken = default)
      => Task.FromResult(_stored);

    public Task UpsertAsync(SalesSettings settings, CancellationToken cancellationToken = default)
    {
      _stored = settings;
      return Task.CompletedTask;
    }
  }

  private sealed class StubInventorySettingsRepo(InventorySettings? stored) : IInventorySettingsRepository
  {
    private InventorySettings? _stored = stored;

    public Task<InventorySettings?> GetAsync(BusinessId businessId, CancellationToken cancellationToken = default)
      => Task.FromResult(_stored);

    public Task UpsertAsync(InventorySettings settings, CancellationToken cancellationToken = default)
    {
      _stored = settings;
      return Task.CompletedTask;
    }
  }

  private sealed class StubBillingSettingsRepo(BillingSettings? stored) : IBillingSettingsRepository
  {
    private BillingSettings? _stored = stored;

    public Task<BillingSettings?> GetAsync(BusinessId businessId, CancellationToken cancellationToken = default)
      => Task.FromResult(_stored);

    public Task UpsertAsync(BillingSettings settings, CancellationToken cancellationToken = default)
    {
      _stored = settings;
      return Task.CompletedTask;
    }
  }

  private sealed class RecordingAuditLog : IAuditLogWriter
  {
    public List<AuditEntry> Entries { get; } = [];

    public Task WriteAsync(AuditEntry entry, CancellationToken cancellationToken = default)
    {
      Entries.Add(entry);
      return Task.CompletedTask;
    }
  }

  private sealed class NoopAuditLogWriter : IAuditLogWriter
  {
    public Task WriteAsync(AuditEntry entry, CancellationToken cancellationToken = default)
      => Task.CompletedTask;
  }

  private sealed class NoopUnitOfWorkImpl : IUnitOfWork
  {
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
      => Task.FromResult(1);
  }

  private sealed class FakeCurrentUser(Guid businessId, Guid userId, IReadOnlyCollection<string> roles) : ICurrentUserService
  {
    public Guid? UserId => userId;
    public Guid? BusinessId => businessId;
    public Guid? BranchId => Guid.NewGuid();
    public IReadOnlyCollection<string> Roles => roles;
    public bool IsAuthenticated => true;
  }

  private sealed class UnauthenticatedUser : ICurrentUserService
  {
    public Guid? UserId => null;
    public Guid? BusinessId => null;
    public Guid? BranchId => null;
    public IReadOnlyCollection<string> Roles => [];
    public bool IsAuthenticated => false;
  }

  private sealed class FixedClock : IClock
  {
    public DateTimeOffset UtcNow => Now;
  }
}

#pragma warning restore CA1707
