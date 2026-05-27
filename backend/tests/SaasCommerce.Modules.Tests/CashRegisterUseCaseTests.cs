#pragma warning disable CA1707

using FluentAssertions;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Contracts.Events;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Application.CashRegisters;
using SaasCommerce.Modules.Sales.Contracts.Events.V1;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Tests;

public sealed class CashRegisterUseCaseTests
{
  private static readonly DateTimeOffset Now = new(2026, 5, 26, 8, 0, 0, TimeSpan.Zero);

  // ── OpenCashRegisterHandler ──────────────────────────────────────────────

  [Fact]
  public async Task OpenCashRegisterHandler_ShouldFail_WhenNoBusinessContext()
  {
    var handler = new OpenCashRegisterHandler(
      new FakeCashRegisterRepo(), Anonymous(), new RecordingOutbox(), new NoopUow(), new FixedClock());

    var result = await handler.Handle(new OpenCashRegisterCommand(Guid.NewGuid(), 500, null));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(CashRegisterErrors.UserContextRequired);
  }

  [Fact]
  public async Task OpenCashRegisterHandler_ShouldFail_WhenNegativeOpeningAmount()
  {
    var handler = new OpenCashRegisterHandler(
      new FakeCashRegisterRepo(), Authed(), new RecordingOutbox(), new NoopUow(), new FixedClock());

    var result = await handler.Handle(new OpenCashRegisterCommand(Guid.NewGuid(), -1, null));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(CashRegisterErrors.InvalidOpeningAmount);
  }

  [Fact]
  public async Task OpenCashRegisterHandler_ShouldFail_WhenUserAlreadyHasOpenRegister()
  {
    var repo = new FakeCashRegisterRepo { HasOpen = true };
    var handler = new OpenCashRegisterHandler(
      repo, Authed(), new RecordingOutbox(), new NoopUow(), new FixedClock());

    var result = await handler.Handle(new OpenCashRegisterCommand(Guid.NewGuid(), 500, null));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(CashRegisterErrors.RegisterAlreadyOpen);
  }

  [Fact]
  public async Task OpenCashRegisterHandler_ShouldSucceed_AndPublishEvent_WhenValid()
  {
    var repo = new FakeCashRegisterRepo();
    var outbox = new RecordingOutbox();
    var handler = new OpenCashRegisterHandler(repo, Authed(), outbox, new NoopUow(), new FixedClock());

    var result = await handler.Handle(new OpenCashRegisterCommand(Guid.NewGuid(), 1000, "Notas apertura"));

    result.IsSuccess.Should().BeTrue();
    result.Value.OpenedAt.Should().Be(Now);
    repo.Added.Should().NotBeNull();
    outbox.Events.OfType<CashRegisterOpenedEventV1>().Should().ContainSingle();
  }

  [Fact]
  public async Task OpenCashRegisterHandler_ShouldSucceed_WithZeroOpeningAmount()
  {
    var repo = new FakeCashRegisterRepo();
    var handler = new OpenCashRegisterHandler(
      repo, Authed(), new RecordingOutbox(), new NoopUow(), new FixedClock());

    var result = await handler.Handle(new OpenCashRegisterCommand(Guid.NewGuid(), 0, null));

    result.IsSuccess.Should().BeTrue();
    repo.Added!.OpeningAmount.Should().Be(0);
  }

  // ── RegisterCashRegisterMovementHandler ──────────────────────────────────

  [Fact]
  public async Task RegisterMovementHandler_ShouldFail_WhenNoBusinessContext()
  {
    var handler = new RegisterCashRegisterMovementHandler(
      new FakeCashRegisterRepo(), Anonymous(), new RecordingOutbox(), new NoopUow(), new FixedClock());

    var result = await handler.Handle(
      new RegisterCashRegisterMovementCommand(Guid.NewGuid(), "CashIn", 100, "Test"));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(CashRegisterErrors.UserContextRequired);
  }

  [Fact]
  public async Task RegisterMovementHandler_ShouldFail_WhenInvalidMovementType()
  {
    var handler = new RegisterCashRegisterMovementHandler(
      new FakeCashRegisterRepo(), Authed(), new RecordingOutbox(), new NoopUow(), new FixedClock());

    var result = await handler.Handle(
      new RegisterCashRegisterMovementCommand(Guid.NewGuid(), "InvalidType", 100, "Test"));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(CashRegisterErrors.InvalidMovementType);
  }

  [Fact]
  public async Task RegisterMovementHandler_ShouldFail_WhenRegisterNotFound()
  {
    var handler = new RegisterCashRegisterMovementHandler(
      new FakeCashRegisterRepo(), Authed(), new RecordingOutbox(), new NoopUow(), new FixedClock());

    var result = await handler.Handle(
      new RegisterCashRegisterMovementCommand(Guid.NewGuid(), "CashIn", 100, "Test"));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(CashRegisterErrors.RegisterNotFound);
  }

  [Fact]
  public async Task RegisterMovementHandler_ShouldSucceed_AndPublishEvent_WhenValid()
  {
    var register = CreateOpenRegister();
    var repo = new FakeCashRegisterRepo { Register = register };
    var outbox = new RecordingOutbox();
    var handler = new RegisterCashRegisterMovementHandler(
      repo, Authed(), outbox, new NoopUow(), new FixedClock());

    var result = await handler.Handle(
      new RegisterCashRegisterMovementCommand(register.Id, "CashIn", 200, "Depósito"));

    result.IsSuccess.Should().BeTrue();
    result.Value.MovementType.Should().Be("CashIn");
    result.Value.Amount.Should().Be(200);
    outbox.Events.OfType<CashRegisterMovementRegisteredEventV1>().Should().ContainSingle();
  }

  [Fact]
  public async Task RegisterMovementHandler_ShouldAccept_CaseInsensitiveMovementType()
  {
    var register = CreateOpenRegister();
    var repo = new FakeCashRegisterRepo { Register = register };
    var handler = new RegisterCashRegisterMovementHandler(
      repo, Authed(), new RecordingOutbox(), new NoopUow(), new FixedClock());

    var result = await handler.Handle(
      new RegisterCashRegisterMovementCommand(register.Id, "cashout", 50, "Retiro"));

    result.IsSuccess.Should().BeTrue();
    result.Value.MovementType.Should().Be("CashOut");
  }

  // ── CloseCashRegisterHandler ──────────────────────────────────────────────

  [Fact]
  public async Task CloseCashRegisterHandler_ShouldFail_WhenRegisterNotFound()
  {
    var handler = new CloseCashRegisterHandler(
      new FakeCashRegisterRepo(), new FakeCashRegisterCalculator(),
      Authed(), new RecordingOutbox(), new NoopUow(), new FixedClock());

    var result = await handler.Handle(new CloseCashRegisterCommand(Guid.NewGuid(), 500, null));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(CashRegisterErrors.RegisterNotFound);
  }

  [Fact]
  public async Task CloseCashRegisterHandler_ShouldPublishCashRegisterClosedEvent()
  {
    var register = CreateOpenRegister(openingAmount: 1000);
    var repo = new FakeCashRegisterRepo { Register = register };
    var outbox = new RecordingOutbox();
    var handler = new CloseCashRegisterHandler(
      repo, new FakeCashRegisterCalculator(cashSales: 500),
      Authed(), outbox, new NoopUow(), new FixedClock());

    // Expected = 1000 + 500 = 1500; Counted = 1500 → Balanced
    var result = await handler.Handle(new CloseCashRegisterCommand(register.Id, 1500, null));

    result.IsSuccess.Should().BeTrue();
    outbox.Events.OfType<CashRegisterClosedEventV1>().Should().ContainSingle();
  }

  [Fact]
  public async Task CloseCashRegisterHandler_ShouldPublishDifferenceEvent_WhenShortage()
  {
    var register = CreateOpenRegister(openingAmount: 1000);
    var repo = new FakeCashRegisterRepo { Register = register };
    var outbox = new RecordingOutbox();
    var handler = new CloseCashRegisterHandler(
      repo, new FakeCashRegisterCalculator(),
      Authed(), outbox, new NoopUow(), new FixedClock());

    // Expected = 1000; Counted = 900 → Shortage -100
    var result = await handler.Handle(new CloseCashRegisterCommand(register.Id, 900, null));

    result.IsSuccess.Should().BeTrue();
    outbox.Events.OfType<CashRegisterClosedEventV1>().Should().ContainSingle();
    outbox.Events.OfType<CashRegisterDifferenceDetectedEventV1>().Should().ContainSingle();
  }

  [Fact]
  public async Task CloseCashRegisterHandler_ShouldNotPublishDifferenceEvent_WhenBalanced()
  {
    var register = CreateOpenRegister(openingAmount: 1000);
    var repo = new FakeCashRegisterRepo { Register = register };
    var outbox = new RecordingOutbox();
    var handler = new CloseCashRegisterHandler(
      repo, new FakeCashRegisterCalculator(),
      Authed(), outbox, new NoopUow(), new FixedClock());

    // Expected = 1000; Counted = 1000 → Balanced
    var result = await handler.Handle(new CloseCashRegisterCommand(register.Id, 1000, null));

    result.IsSuccess.Should().BeTrue();
    outbox.Events.OfType<CashRegisterDifferenceDetectedEventV1>().Should().BeEmpty();
  }

  // ── CloseCashRegisterHandler (missing paths) ──────────────────────────────

  [Fact]
  public async Task CloseCashRegisterHandler_ShouldFail_WhenNoBusinessContext()
  {
    var handler = new CloseCashRegisterHandler(
      new FakeCashRegisterRepo(), new FakeCashRegisterCalculator(),
      Anonymous(), new RecordingOutbox(), new NoopUow(), new FixedClock());

    var result = await handler.Handle(new CloseCashRegisterCommand(Guid.NewGuid(), 500, null));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(CashRegisterErrors.UserContextRequired);
  }

  [Fact]
  public async Task CloseCashRegisterHandler_ShouldFail_WhenNegativeCountedAmount()
  {
    var handler = new CloseCashRegisterHandler(
      new FakeCashRegisterRepo(), new FakeCashRegisterCalculator(),
      Authed(), new RecordingOutbox(), new NoopUow(), new FixedClock());

    var result = await handler.Handle(new CloseCashRegisterCommand(Guid.NewGuid(), -1, null));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(CashRegisterErrors.InvalidCountedAmount);
  }

  [Fact]
  public async Task CloseCashRegisterHandler_ShouldFail_WhenRegisterNotOpen()
  {
    var register = CreateOpenRegister(openingAmount: 500);
    register.Close(500, new CashRegisterTotals(0, 0, 0, 0, 0), Now.AddHours(8), null);
    var repo = new FakeCashRegisterRepo { Register = register };
    var handler = new CloseCashRegisterHandler(
      repo, new FakeCashRegisterCalculator(),
      Authed(), new RecordingOutbox(), new NoopUow(), new FixedClock());

    var result = await handler.Handle(new CloseCashRegisterCommand(register.Id, 500, null));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(CashRegisterErrors.RegisterNotOpen);
  }

  // ── RegisterCashRegisterMovementHandler (missing paths) ───────────────────

  [Fact]
  public async Task RegisterMovementHandler_ShouldFail_WhenAmountIsZero()
  {
    var handler = new RegisterCashRegisterMovementHandler(
      new FakeCashRegisterRepo(), Authed(), new RecordingOutbox(), new NoopUow(), new FixedClock());

    var result = await handler.Handle(
      new RegisterCashRegisterMovementCommand(Guid.NewGuid(), "CashIn", 0, "Test"));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(CashRegisterErrors.InvalidMovementAmount);
  }

  [Fact]
  public async Task RegisterMovementHandler_ShouldFail_WhenRegisterNotOpen()
  {
    var register = CreateOpenRegister(openingAmount: 500);
    register.Close(500, new CashRegisterTotals(0, 0, 0, 0, 0), Now.AddHours(8), null);
    var repo = new FakeCashRegisterRepo { Register = register };
    var handler = new RegisterCashRegisterMovementHandler(
      repo, Authed(), new RecordingOutbox(), new NoopUow(), new FixedClock());

    var result = await handler.Handle(
      new RegisterCashRegisterMovementCommand(register.Id, "CashIn", 100, "Test"));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(CashRegisterErrors.RegisterNotOpen);
  }

  // ── GetCashRegisterDetailHandler ──────────────────────────────────────────

  [Fact]
  public async Task GetCashRegisterDetailHandler_ShouldFail_WhenNoBusinessContext()
  {
    var handler = new GetCashRegisterDetailHandler(new FakeCashRegisterRepo(), Anonymous());

    var result = await handler.Handle(new GetCashRegisterDetailQuery(Guid.NewGuid()));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(CashRegisterErrors.UserContextRequired);
  }

  [Fact]
  public async Task GetCashRegisterDetailHandler_ShouldFail_WhenRegisterNotFound()
  {
    var handler = new GetCashRegisterDetailHandler(new FakeCashRegisterRepo(), Authed());

    var result = await handler.Handle(new GetCashRegisterDetailQuery(Guid.NewGuid()));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(CashRegisterErrors.RegisterNotFound);
  }

  [Fact]
  public async Task GetCashRegisterDetailHandler_ShouldSucceed_WhenRegisterFound()
  {
    var register = CreateOpenRegister(openingAmount: 1500);
    var repo = new FakeCashRegisterRepo { Register = register };
    var handler = new GetCashRegisterDetailHandler(repo, Authed());

    var result = await handler.Handle(new GetCashRegisterDetailQuery(register.Id));

    result.IsSuccess.Should().BeTrue();
    result.Value.OpeningAmount.Should().Be(1500);
    result.Value.Status.Should().Be("Open");
  }

  // ── GetDailyCashRegisterSummaryHandler ────────────────────────────────────

  [Fact]
  public async Task GetDailyCashRegisterSummaryHandler_ShouldFail_WhenNoBusinessContext()
  {
    var handler = new GetDailyCashRegisterSummaryHandler(new FakeCashRegisterRepo(), Anonymous());

    var result = await handler.Handle(
      new GetDailyCashRegisterSummaryQuery(DateOnly.FromDateTime(Now.Date), null));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(CashRegisterErrors.UserContextRequired);
  }

  [Fact]
  public async Task GetDailyCashRegisterSummaryHandler_ShouldReturnAggregatedSummary_WhenRegistersExist()
  {
    var open = CreateOpenRegister(openingAmount: 1000);
    var closed = CreateOpenRegister(openingAmount: 500);
    closed.Close(500, new CashRegisterTotals(CashSales: 300, 0, 0, 0, 0), Now.AddHours(8), null);
    var repo = new FakeCashRegisterRepo { DailySummaryData = [open, closed] };
    var handler = new GetDailyCashRegisterSummaryHandler(repo, Authed());

    var result = await handler.Handle(
      new GetDailyCashRegisterSummaryQuery(DateOnly.FromDateTime(Now.Date), null));

    result.IsSuccess.Should().BeTrue();
    result.Value.OpenRegisters.Should().Be(1);
    result.Value.ClosedRegisters.Should().Be(1);
    result.Value.Registers.Should().HaveCount(2);
  }

  // ── GetActiveCashRegisterHandler ──────────────────────────────────────────

  [Fact]
  public async Task GetActiveCashRegisterHandler_ShouldFail_WhenNoBusinessContext()
  {
    var handler = new GetActiveCashRegisterHandler(new FakeCashRegisterRepo(), Anonymous());

    var result = await handler.Handle();

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(CashRegisterErrors.UserContextRequired);
  }

  [Fact]
  public async Task GetActiveCashRegisterHandler_ShouldReturnNull_WhenNoOpenRegister()
  {
    var handler = new GetActiveCashRegisterHandler(new FakeCashRegisterRepo(), Authed());

    var result = await handler.Handle();

    result.IsSuccess.Should().BeTrue();
    result.Value.Should().BeNull();
  }

  [Fact]
  public async Task GetActiveCashRegisterHandler_ShouldReturnRegister_WhenOpenRegisterExists()
  {
    var register = CreateOpenRegister(openingAmount: 2000);
    var repo = new FakeCashRegisterRepo { OpenRegister = register };
    var handler = new GetActiveCashRegisterHandler(repo, Authed());

    var result = await handler.Handle();

    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeNull();
    result.Value!.OpeningAmount.Should().Be(2000);
    result.Value.Status.Should().Be("Open");
  }

  [Fact]
  public async Task CashRegisterQueries_ShouldFilterByBusinessId()
  {
    var businessId = Guid.NewGuid();
    var user = new FakeUser { BusinessId = businessId, UserId = Guid.NewGuid(), IsAuthenticated = true };
    var repo = new FakeCashRegisterRepo();
    var handler = new GetActiveCashRegisterHandler(repo, user);

    await handler.Handle();

    repo.LastQueriedBusinessId.Should().Be(new BusinessId(businessId));
  }

  [Fact]
  public async Task GetCashRegistersHandler_ShouldFail_WhenNoBusinessContext()
  {
    var handler = new GetCashRegistersHandler(new FakeCashRegisterRepo(), Anonymous());

    var result = await handler.Handle(new GetCashRegistersQuery(null, null, null, null, 1, 50));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(CashRegisterErrors.UserContextRequired);
  }

  [Fact]
  public async Task GetCashRegistersHandler_ShouldReturnPagedSummaryAndClampInvalidPaging()
  {
    var register = CreateOpenRegister(openingAmount: 750);
    var businessId = Guid.NewGuid();
    var user = new FakeUser { BusinessId = businessId, UserId = Guid.NewGuid(), IsAuthenticated = true };
    var repo = new FakeCashRegisterRepo
    {
      ListData = [register],
      TotalCount = 1,
    };
    var handler = new GetCashRegistersHandler(repo, user);

    var result = await handler.Handle(new GetCashRegistersQuery(register.BranchId.Value, "Open", null, null, -4, 300));

    result.IsSuccess.Should().BeTrue();
    result.Value.Page.Should().Be(1);
    result.Value.PageSize.Should().Be(100);
    result.Value.TotalCount.Should().Be(1);
    result.Value.Items.Should().ContainSingle(item =>
      item.CashRegisterId == register.Id &&
      item.OpeningAmount == 750 &&
      item.Status == "Open");
    repo.LastQueriedBusinessId.Should().Be(new BusinessId(businessId));
    repo.LastCriteria.Should().NotBeNull();
    repo.LastCriteria!.Page.Should().Be(1);
    repo.LastCriteria.PageSize.Should().Be(100);
  }

  // ── Helpers ───────────────────────────────────────────────────────────────

  private static FakeUser Authed()
    => new()
    {
      BusinessId = Guid.NewGuid(),
      UserId = Guid.NewGuid(),
      BranchId = Guid.NewGuid(),
      IsAuthenticated = true,
    };

  private static FakeUser Anonymous()
    => new() { IsAuthenticated = false };

  private static CashRegister CreateOpenRegister(decimal openingAmount = 500)
    => CashRegister.Open(
      Guid.NewGuid(),
      new BusinessId(Guid.NewGuid()),
      new BranchId(Guid.NewGuid()),
      Guid.NewGuid(),
      openingAmount,
      null,
      Now);

  // ── Test Doubles ──────────────────────────────────────────────────────────

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

  private sealed class FakeCashRegisterRepo : ICashRegisterRepository, ICashRegisterReadRepository
  {
    public bool HasOpen { get; set; }
    public CashRegister? Register { get; set; }
    public CashRegister? OpenRegister { get; set; }
    public CashRegister? Added { get; private set; }
    public BusinessId LastQueriedBusinessId { get; private set; }
    public IReadOnlyCollection<CashRegister> DailySummaryData { get; set; } = [];
    public IReadOnlyCollection<CashRegister> ListData { get; set; } = [];
    public int TotalCount { get; set; }
    public CashRegisterSearchCriteria? LastCriteria { get; private set; }

    public Task<CashRegister?> GetAsync(BusinessId businessId, Guid cashRegisterId, CancellationToken ct)
    {
      LastQueriedBusinessId = businessId;
      return Task.FromResult(Register?.Id == cashRegisterId ? Register : null);
    }

    public Task<CashRegister?> GetOpenRegisterAsync(BusinessId businessId, Guid userId, CancellationToken ct)
    {
      LastQueriedBusinessId = businessId;
      return Task.FromResult(OpenRegister);
    }

    public Task<bool> HasOpenRegisterAsync(BusinessId businessId, Guid userId, CancellationToken ct)
    {
      LastQueriedBusinessId = businessId;
      return Task.FromResult(HasOpen);
    }

    public Task AddAsync(CashRegister cashRegister, CancellationToken ct)
    {
      Added = cashRegister;
      return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<CashRegister>> ListAsync(
      BusinessId businessId, CashRegisterSearchCriteria criteria, CancellationToken ct)
    {
      LastQueriedBusinessId = businessId;
      LastCriteria = criteria;
      return Task.FromResult(ListData);
    }

    public Task<int> CountAsync(
      BusinessId businessId, CashRegisterSearchCriteria criteria, CancellationToken ct)
    {
      LastQueriedBusinessId = businessId;
      LastCriteria = criteria;
      return Task.FromResult(TotalCount);
    }

    public Task<IReadOnlyCollection<CashRegister>> GetDailySummaryAsync(
      BusinessId businessId, Guid? branchId, DateOnly summaryDate, CancellationToken ct)
      => Task.FromResult(DailySummaryData);
  }

  private sealed class FakeCashRegisterCalculator : ICashRegisterCalculator
  {
    private readonly decimal cashSales;

    public FakeCashRegisterCalculator(decimal cashSales = 0) => this.cashSales = cashSales;

    public Task<CashRegisterTotals> CalculateAsync(
      BusinessId businessId, BranchId branchId, DateTimeOffset openedAt, DateTimeOffset closedAt, CancellationToken ct)
      => Task.FromResult(new CashRegisterTotals(cashSales, 0, 0, 0, 0));
  }
}

#pragma warning restore CA1707
