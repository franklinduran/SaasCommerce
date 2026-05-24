#pragma warning disable CA1707

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Contracts.Events;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Application.DailyClosings;
using SaasCommerce.Modules.Sales.Contracts.Events.V1;
using SaasCommerce.Modules.Sales.Contracts.Responses;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Tests;

public sealed class DailyClosingUseCaseTests
{
  private static readonly DateTimeOffset Now = new(2026, 5, 23, 18, 0, 0, TimeSpan.Zero);
  private static readonly DateOnly Today = new(2026, 5, 23);
  private static readonly Guid BusinessGuid = Guid.Parse("11111111-1111-1111-1111-111111111111");
  private static readonly Guid BranchGuid = Guid.Parse("22222222-2222-2222-2222-222222222222");
  private static readonly Guid UserGuid = Guid.Parse("33333333-3333-3333-3333-333333333333");
  private static readonly BusinessId BId = new(BusinessGuid);
  private static readonly BranchId BranchId = new(BranchGuid);

  // ── GetDailyClosingsHandler ──────────────────────────────────────────────

  [Fact]
  public async Task GetDailyClosings_ShouldFail_WhenNotAuthenticated()
  {
    var handler = new GetDailyClosingsHandler(new FakeDailyClosingReadRepo(), Anonymous());

    var result = await handler.Handle(new GetDailyClosingsQuery(null, null, null));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(DailyClosingErrors.UserContextRequired);
  }

  [Fact]
  public async Task GetDailyClosings_ShouldReturnEmptyList_WhenNoClosings()
  {
    var handler = new GetDailyClosingsHandler(new FakeDailyClosingReadRepo(), Authed());

    var result = await handler.Handle(new GetDailyClosingsQuery(null, null, null));

    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().BeEmpty();
    result.Value.TotalCount.Should().Be(0);
  }

  [Fact]
  public async Task GetDailyClosings_ShouldClampPageSize_WhenTooLarge()
  {
    var handler = new GetDailyClosingsHandler(new FakeDailyClosingReadRepo(), Authed());

    var result = await handler.Handle(new GetDailyClosingsQuery(null, null, null, Page: 1, PageSize: 999));

    result.IsSuccess.Should().BeTrue();
    result.Value.PageSize.Should().Be(100); // clamped to 100
  }

  [Fact]
  public async Task GetDailyClosings_ShouldEnforceMinimumPage()
  {
    var handler = new GetDailyClosingsHandler(new FakeDailyClosingReadRepo(), Authed());

    var result = await handler.Handle(new GetDailyClosingsQuery(null, null, null, Page: -5, PageSize: 10));

    result.IsSuccess.Should().BeTrue();
    result.Value.Page.Should().Be(1); // minimum page = 1
  }

  // ── GetDailyClosingDetailHandler ─────────────────────────────────────────

  [Fact]
  public async Task GetDailyClosingDetail_ShouldFail_WhenNotAuthenticated()
  {
    var handler = new GetDailyClosingDetailHandler(new FakeDailyClosingReadRepo(), Anonymous());

    var result = await handler.Handle(new GetDailyClosingDetailQuery(Guid.NewGuid()));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(DailyClosingErrors.UserContextRequired);
  }

  [Fact]
  public async Task GetDailyClosingDetail_ShouldFail_WhenClosingNotFound()
  {
    var handler = new GetDailyClosingDetailHandler(new FakeDailyClosingReadRepo(), Authed());

    var result = await handler.Handle(new GetDailyClosingDetailQuery(Guid.NewGuid()));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(DailyClosingErrors.NotFound);
  }

  [Fact]
  public async Task GetDailyClosingDetail_ShouldReturnDetail_WhenFound()
  {
    var detail = BuildDetailResponse();
    var repo = new FakeDailyClosingReadRepo { Detail = detail };
    var handler = new GetDailyClosingDetailHandler(repo, Authed());

    var result = await handler.Handle(new GetDailyClosingDetailQuery(detail.Id));

    result.IsSuccess.Should().BeTrue();
    result.Value.Id.Should().Be(detail.Id);
    result.Value.TotalSales.Should().Be(12_000m);
  }

  // ── CreateDailyClosingHandler (via InMemory EF) ──────────────────────────

  [Fact]
  public async Task CreateDailyClosing_ShouldFail_WhenNotAuthenticated()
  {
    await using var db = CreateDbContext();
    var handler = new CreateDailyClosingHandler(
      new FakeDailyClosingRepository(), new FakeDataGatherer(), Anonymous(), db,
      new RecordingOutbox(), new NoopUow(), new FixedClock());

    var result = await handler.Handle(new CreateDailyClosingCommand(Today, BranchGuid, null));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(DailyClosingErrors.UserContextRequired);
  }

  [Fact]
  public async Task CreateDailyClosing_ShouldReturnExistingDraft_WhenIdempotent()
  {
    var existingId = Guid.NewGuid();
    var existing = BuildDomainClosing(existingId);
    var repo = new FakeDailyClosingRepository { ExistingByDate = existing };

    await using var db = CreateDbContext();
    var handler = new CreateDailyClosingHandler(
      repo, new FakeDataGatherer(), Authed(), db,
      new RecordingOutbox(), new NoopUow(), new FixedClock());

    var result = await handler.Handle(new CreateDailyClosingCommand(Today, BranchGuid, null));

    result.IsSuccess.Should().BeTrue();
    result.Value.Id.Should().Be(existingId);
  }

  [Fact]
  public async Task CreateDailyClosing_ShouldFail_WhenClosedClosingExistsForDate()
  {
    var existing = BuildDomainClosing(Guid.NewGuid());
    existing.Close(UserGuid, 5_000m, null, Now);
    var repo = new FakeDailyClosingRepository { ExistingByDate = existing };

    await using var db = CreateDbContext();
    var handler = new CreateDailyClosingHandler(
      repo, new FakeDataGatherer(), Authed(), db,
      new RecordingOutbox(), new NoopUow(), new FixedClock());

    var result = await handler.Handle(new CreateDailyClosingCommand(Today, BranchGuid, null));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(DailyClosingErrors.AlreadyClosed);
  }

  [Fact]
  public async Task CreateDailyClosing_ShouldSucceed_AndPublishEvent_WhenNewDate()
  {
    var repo = new FakeDailyClosingRepository();
    var outbox = new RecordingOutbox();

    await using var db = CreateDbContext();
    var handler = new CreateDailyClosingHandler(
      repo, new FakeDataGatherer(), Authed(), db,
      outbox, new NoopUow(), new FixedClock());

    var result = await handler.Handle(new CreateDailyClosingCommand(Today, BranchGuid, "Notas de cierre"));

    result.IsSuccess.Should().BeTrue();
    repo.Added.Should().NotBeNull();
    repo.Added!.Status.Should().Be(DailyClosingStatus.Draft);
    outbox.Events.OfType<DailyClosingCreatedEventV1>().Should().ContainSingle();
  }

  // ── CloseDailyClosingHandler ──────────────────────────────────────────────

  [Fact]
  public async Task CloseDailyClosing_ShouldFail_WhenNotAuthenticated()
  {
    await using var db = CreateDbContext();
    var handler = new CloseDailyClosingHandler(
      new FakeDailyClosingRepository(), Anonymous(), db,
      new RecordingOutbox(), new NoopUow(), new FixedClock());

    var result = await handler.Handle(new CloseDailyClosingCommand(Guid.NewGuid(), 5_000m, null));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(DailyClosingErrors.UserContextRequired);
  }

  [Fact]
  public async Task CloseDailyClosing_ShouldFail_WhenClosingNotFound()
  {
    await using var db = CreateDbContext();
    var handler = new CloseDailyClosingHandler(
      new FakeDailyClosingRepository(), Authed(), db,
      new RecordingOutbox(), new NoopUow(), new FixedClock());

    var result = await handler.Handle(new CloseDailyClosingCommand(Guid.NewGuid(), 5_000m, null));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(DailyClosingErrors.NotFound);
  }

  [Fact]
  public async Task CloseDailyClosing_ShouldFail_WhenAlreadyClosed()
  {
    var closing = BuildDomainClosing(Guid.NewGuid());
    closing.Close(UserGuid, 5_000m, null, Now);
    var repo = new FakeDailyClosingRepository { FoundById = closing };

    await using var db = CreateDbContext();
    var handler = new CloseDailyClosingHandler(
      repo, Authed(), db, new RecordingOutbox(), new NoopUow(), new FixedClock());

    var result = await handler.Handle(new CloseDailyClosingCommand(closing.Id, 5_000m, null));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(DailyClosingErrors.AlreadyClosedStatus);
  }

  [Fact]
  public async Task CloseDailyClosing_ShouldSucceed_AndPublishEvent_WhenNoCashSessionsOpen()
  {
    var closing = BuildDomainClosing(Guid.NewGuid());
    var repo = new FakeDailyClosingRepository { FoundById = closing };
    var outbox = new RecordingOutbox();

    await using var db = CreateDbContext(); // no open cash sessions seeded
    var handler = new CloseDailyClosingHandler(
      repo, Authed(), db, outbox, new NoopUow(), new FixedClock());

    var result = await handler.Handle(new CloseDailyClosingCommand(closing.Id, 4_900m, "Final"));

    result.IsSuccess.Should().BeTrue();
    result.Value.Status.Should().Be("Closed");
    result.Value.CashCounted.Should().Be(4_900m);
    outbox.Events.OfType<DailyClosingClosedEventV1>().Should().ContainSingle();
  }

  // ── Test Doubles ──────────────────────────────────────────────────────────

  private static FakeUser Authed()
    => new() { BusinessId = BusinessGuid, UserId = UserGuid, BranchId = BranchGuid, IsAuthenticated = true };

  private static FakeUser Anonymous()
    => new() { IsAuthenticated = false };

  private static AppDbContext CreateDbContext()
  {
    var options = new DbContextOptionsBuilder<AppDbContext>()
      .UseInMemoryDatabase(Guid.NewGuid().ToString("D"))
      .Options;
    return new AppDbContext(options);
  }

  private static DailyClosing BuildDomainClosing(Guid id)
    => DailyClosing.Create(
      id, BId, BranchId, UserGuid, Today,
      12_000m, 4_000m, 5_000m, 2_000m, 1_000m, 30,
      4_800m, 2_000m, 7_200m, 4_800m, 2_800m, 40m, 23.33m,
      1_000m, 2, 500m, null, Now);

  private static DailyClosingDetailResponse BuildDetailResponse()
    => new(
      Id: Guid.NewGuid(),
      BusinessId: BusinessGuid,
      BranchId: BranchGuid,
      BranchName: "Sucursal Central",
      ClosingDate: Today.ToString("yyyy-MM-dd"),
      Status: "Draft",
      TotalSales: 12_000m,
      CashSales: 4_000m,
      TransferSales: 5_000m,
      CardSales: 2_000m,
      CreditSales: 1_000m,
      SalesCount: 30,
      CashExpected: 4_800m,
      CashCounted: null,
      CashDifference: null,
      TotalExpenses: 2_000m,
      TotalCost: 7_200m,
      GrossProfit: 4_800m,
      EstimatedNetProfit: 2_800m,
      GrossMarginPercent: 40m,
      NetMarginPercent: 23.33m,
      NewCreditsAmount: 1_000m,
      NewCreditsCount: 2,
      CreditPaymentsReceived: 500m,
      Notes: null,
      CreatedAt: Now,
      ClosedAt: null,
      ClosedByUserId: null,
      Alerts: []);

  private sealed class FakeUser : ICurrentUserService
  {
    public Guid? UserId { get; init; }
    public Guid? BusinessId { get; init; }
    public Guid? BranchId { get; init; }
    public IReadOnlyCollection<string> Roles { get; init; } = [];
    public bool IsAuthenticated { get; init; }
  }

  private sealed class FixedClock : IClock
  {
    public DateTimeOffset UtcNow => Now;
  }

  private sealed class NoopUow : IUnitOfWork
  {
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
      => Task.FromResult(1);
  }

  private sealed class RecordingOutbox : IOutboxWriter
  {
    public List<IIntegrationEvent> Events { get; } = [];

    public Task AddAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
      where TEvent : class, IIntegrationEvent
    {
      Events.Add(integrationEvent);
      return Task.CompletedTask;
    }
  }

  private sealed class FakeDailyClosingRepository : IDailyClosingRepository
  {
    public DailyClosing? ExistingByDate { get; set; }
    public DailyClosing? FoundById { get; set; }
    public DailyClosing? Added { get; private set; }

    public Task<DailyClosing?> GetByIdAsync(Guid id, BusinessId businessId, CancellationToken cancellationToken = default)
      => Task.FromResult(FoundById?.Id == id ? FoundById : null);

    public Task<DailyClosing?> GetByDateAndBranchAsync(
      BusinessId businessId, BranchId branchId, DateOnly date, CancellationToken cancellationToken = default)
      => Task.FromResult(ExistingByDate);

    public Task AddAsync(DailyClosing closing, CancellationToken cancellationToken = default)
    {
      Added = closing;
      return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
      => Task.CompletedTask;
  }

  private sealed class FakeDailyClosingReadRepo : IDailyClosingReadRepository
  {
    public DailyClosingDetailResponse? Detail { get; set; }

    public Task<(IReadOnlyCollection<DailyClosingListItemResponse> Items, int TotalCount)> GetListAsync(
      BusinessId businessId, Guid? branchId, DateOnly? dateFrom, DateOnly? dateTo,
      int page, int pageSize, CancellationToken cancellationToken = default)
      => Task.FromResult<(IReadOnlyCollection<DailyClosingListItemResponse>, int)>(([], 0));

    public Task<DailyClosingDetailResponse?> GetDetailAsync(
      Guid id, BusinessId businessId, CancellationToken cancellationToken = default)
      => Task.FromResult(Detail?.Id == id ? Detail : null);
  }

  private sealed class FakeDataGatherer : IDailyClosingDataGatherer
  {
    public Task<DailyClosingData> GatherAsync(
      BusinessId businessId, BranchId branchId, DateOnly date, CancellationToken cancellationToken = default)
      => Task.FromResult(new DailyClosingData(
        TotalSales: 12_000m,
        CashSales: 4_000m,
        TransferSales: 5_000m,
        CardSales: 2_000m,
        CreditSales: 1_000m,
        SalesCount: 30,
        CashSessionOpeningBalance: 800m,
        HasOpenCashSessions: false,
        TotalExpenses: 2_000m,
        TotalCost: 7_200m,
        HasMissingCosts: false,
        NewCreditsAmount: 1_000m,
        NewCreditsCount: 2,
        CreditPaymentsReceived: 500m));
  }
}

#pragma warning restore CA1707
