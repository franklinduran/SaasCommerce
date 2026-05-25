#pragma warning disable CA1707

using FluentAssertions;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Contracts.Events;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Application.Cash;
using SaasCommerce.Modules.Sales.Contracts.Events.V1;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Tests;

public sealed class CashUseCaseTests
{
  private static readonly DateTimeOffset Now = new(2026, 5, 22, 10, 0, 0, TimeSpan.Zero);

  // ── OpenCashSessionHandler ───────────────────────────────────────────────

  [Fact]
  public async Task OpenCashSession_ShouldFail_WhenNoBusinessContext()
  {
    var handler = new OpenCashSessionHandler(
      new FakeCashRepo(), Anonymous(), new RecordingOutbox(), new NoopUow(), new FixedClock());

    var result = await handler.Handle(new OpenCashSessionCommand(500, null));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(CashErrors.UserContextRequired);
  }

  [Fact]
  public async Task OpenCashSession_ShouldFail_WhenNegativeOpeningBalance()
  {
    var handler = new OpenCashSessionHandler(
      new FakeCashRepo(), Authed(), new RecordingOutbox(), new NoopUow(), new FixedClock());

    var result = await handler.Handle(new OpenCashSessionCommand(-1, null));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(CashErrors.InvalidClosingBalance);
  }

  [Fact]
  public async Task OpenCashSession_ShouldFail_WhenSessionAlreadyOpen()
  {
    var repo = new FakeCashRepo { HasOpen = true };
    var handler = new OpenCashSessionHandler(
      repo, Authed(), new RecordingOutbox(), new NoopUow(), new FixedClock());

    var result = await handler.Handle(new OpenCashSessionCommand(500, null));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(CashErrors.SessionAlreadyOpen);
  }

  [Fact]
  public async Task OpenCashSession_ShouldSucceed_AndPublishEvent_WhenValid()
  {
    var repo = new FakeCashRepo();
    var outbox = new RecordingOutbox();
    var handler = new OpenCashSessionHandler(repo, Authed(), outbox, new NoopUow(), new FixedClock());

    var result = await handler.Handle(new OpenCashSessionCommand(1000, "Test notes"));

    result.IsSuccess.Should().BeTrue();
    result.Value.OpeningBalance.Should().Be(1000);
    result.Value.Status.Should().Be("Open");
    repo.Added.Should().NotBeNull();
    outbox.Events.OfType<CashSessionOpenedEventV1>().Should().ContainSingle();
  }

  [Fact]
  public async Task OpenCashSession_ShouldSucceed_WithZeroOpeningBalance()
  {
    var repo = new FakeCashRepo();
    var handler = new OpenCashSessionHandler(
      repo, Authed(), new RecordingOutbox(), new NoopUow(), new FixedClock());

    var result = await handler.Handle(new OpenCashSessionCommand(0, null));

    result.IsSuccess.Should().BeTrue();
    result.Value.OpeningBalance.Should().Be(0);
  }

  // ── CloseCashSessionHandler ──────────────────────────────────────────────

  [Fact]
  public async Task CloseCashSession_ShouldFail_WhenNoBusinessContext()
  {
    var handler = new CloseCashSessionHandler(
      new FakeCashRepo(), Anonymous(), new RecordingOutbox(), new NoopUow(), new FixedClock());

    var result = await handler.Handle(new CloseCashSessionCommand(Guid.NewGuid(), 500));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(CashErrors.UserContextRequired);
  }

  [Fact]
  public async Task CloseCashSession_ShouldFail_WhenSessionNotFound()
  {
    var handler = new CloseCashSessionHandler(
      new FakeCashRepo(), Authed(), new RecordingOutbox(), new NoopUow(), new FixedClock());

    var result = await handler.Handle(new CloseCashSessionCommand(Guid.NewGuid(), 500));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(CashErrors.SessionNotFound);
  }

  [Fact]
  public async Task CloseCashSession_ShouldFail_WhenSessionAlreadyClosed()
  {
    var session = CreateOpenSession();
    session.Close(500, Now.AddMinutes(10));
    var repo = new FakeCashRepo { Session = session };

    var handler = new CloseCashSessionHandler(
      repo, Authed(), new RecordingOutbox(), new NoopUow(), new FixedClock());

    var result = await handler.Handle(new CloseCashSessionCommand(session.Id, 500));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(CashErrors.SessionNotOpen);
  }

  [Fact]
  public async Task CloseCashSession_ShouldFail_WhenClosingBalanceIsNegative()
  {
    var handler = new CloseCashSessionHandler(
      new FakeCashRepo(), Authed(), new RecordingOutbox(), new NoopUow(), new FixedClock());

    var result = await handler.Handle(new CloseCashSessionCommand(Guid.NewGuid(), -1));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(CashErrors.InvalidClosingBalance);
  }

  [Fact]
  public async Task CloseCashSession_ShouldReturnBalanced_WhenClosingMatchesSystem()
  {
    var session = CreateOpenSession(openingBalance: 1000);
    var repo = new FakeCashRepo { Session = session };
    var outbox = new RecordingOutbox();

    var handler = new CloseCashSessionHandler(repo, Authed(), outbox, new NoopUow(), new FixedClock());

    var result = await handler.Handle(new CloseCashSessionCommand(session.Id, 1000));

    result.IsSuccess.Should().BeTrue();
    result.Value.Outcome.Should().Be("Balanced");
    result.Value.Difference.Should().Be(0);
    outbox.Events.OfType<CashSessionClosedEventV1>().Should().ContainSingle();
  }

  [Fact]
  public async Task CloseCashSession_ShouldReturnShortage_WhenClosingBelowSystem()
  {
    var session = CreateOpenSession(openingBalance: 1000);
    var repo = new FakeCashRepo { Session = session };

    var handler = new CloseCashSessionHandler(
      repo, Authed(), new RecordingOutbox(), new NoopUow(), new FixedClock());

    var result = await handler.Handle(new CloseCashSessionCommand(session.Id, 900));

    result.IsSuccess.Should().BeTrue();
    result.Value.Outcome.Should().Be("Shortage");
    result.Value.Difference.Should().Be(-100);
  }

  [Fact]
  public async Task CloseCashSession_ShouldReturnSurplus_WhenClosingExceedsSystem()
  {
    var session = CreateOpenSession(openingBalance: 1000);
    var repo = new FakeCashRepo { Session = session };

    var handler = new CloseCashSessionHandler(
      repo, Authed(), new RecordingOutbox(), new NoopUow(), new FixedClock());

    var result = await handler.Handle(new CloseCashSessionCommand(session.Id, 1100));

    result.IsSuccess.Should().BeTrue();
    result.Value.Outcome.Should().Be("Surplus");
    result.Value.Difference.Should().Be(100);
  }

  // ── RegisterCashMovementHandler ──────────────────────────────────────────

  [Fact]
  public async Task RegisterCashMovement_ShouldFail_WhenNoBusinessContext()
  {
    var handler = new RegisterCashMovementHandler(
      new FakeCashRepo(), Anonymous(), new RecordingOutbox(), new NoopUow(), new FixedClock());

    var result = await handler.Handle(new RegisterCashMovementCommand(Guid.NewGuid(), "CashIn", 100, "Test"));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(CashErrors.UserContextRequired);
  }

  [Fact]
  public async Task RegisterCashMovement_ShouldFail_WhenInvalidMovementType()
  {
    var handler = new RegisterCashMovementHandler(
      new FakeCashRepo(), Authed(), new RecordingOutbox(), new NoopUow(), new FixedClock());

    var result = await handler.Handle(new RegisterCashMovementCommand(Guid.NewGuid(), "InvalidType", 100, "Test"));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(CashErrors.InvalidMovementType);
  }

  [Fact]
  public async Task RegisterCashMovement_ShouldFail_WhenAmountIsZero()
  {
    var handler = new RegisterCashMovementHandler(
      new FakeCashRepo(), Authed(), new RecordingOutbox(), new NoopUow(), new FixedClock());

    var result = await handler.Handle(new RegisterCashMovementCommand(Guid.NewGuid(), "CashIn", 0, "Test"));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(CashErrors.InvalidAmount);
  }

  [Fact]
  public async Task RegisterCashMovement_ShouldFail_WhenSessionNotFound()
  {
    var handler = new RegisterCashMovementHandler(
      new FakeCashRepo(), Authed(), new RecordingOutbox(), new NoopUow(), new FixedClock());

    var result = await handler.Handle(new RegisterCashMovementCommand(Guid.NewGuid(), "CashIn", 100, "Test"));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(CashErrors.SessionNotFound);
  }

  [Fact]
  public async Task RegisterCashMovement_ShouldFail_WhenSessionIsClosed()
  {
    var session = CreateOpenSession(openingBalance: 500);
    session.Close(500, Now.AddMinutes(10));
    var repo = new FakeCashRepo { Session = session };

    var handler = new RegisterCashMovementHandler(
      repo, Authed(), new RecordingOutbox(), new NoopUow(), new FixedClock());

    var result = await handler.Handle(new RegisterCashMovementCommand(session.Id, "CashIn", 100, "Late"));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(CashErrors.SessionNotOpen);
  }

  [Fact]
  public async Task RegisterCashMovement_ShouldSucceed_AndPublishEvent_ForCashIn()
  {
    var session = CreateOpenSession(openingBalance: 500);
    var repo = new FakeCashRepo { Session = session };
    var outbox = new RecordingOutbox();

    var handler = new RegisterCashMovementHandler(repo, Authed(), outbox, new NoopUow(), new FixedClock());

    var result = await handler.Handle(new RegisterCashMovementCommand(session.Id, "CashIn", 200, "Depósito"));

    result.IsSuccess.Should().BeTrue();
    result.Value.Type.Should().Be("CashIn");
    result.Value.Amount.Should().Be(200);
    outbox.Events.OfType<CashMovementRegisteredEventV1>().Should().ContainSingle();
  }

  [Fact]
  public async Task RegisterCashMovement_ShouldSucceed_CaseInsensitiveType()
  {
    var session = CreateOpenSession(openingBalance: 500);
    var repo = new FakeCashRepo { Session = session };

    var handler = new RegisterCashMovementHandler(
      repo, Authed(), new RecordingOutbox(), new NoopUow(), new FixedClock());

    var result = await handler.Handle(new RegisterCashMovementCommand(session.Id, "cashin", 100, "Test"));

    result.IsSuccess.Should().BeTrue();
    result.Value.Type.Should().Be("CashIn");
  }

  // ── GetCurrentCashSessionHandler ─────────────────────────────────────────

  [Fact]
  public async Task GetCurrentCashSession_ShouldFail_WhenNoBusinessContext()
  {
    var handler = new GetCurrentCashSessionHandler(new FakeCashRepo(), Anonymous());

    var result = await handler.Handle();

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(CashErrors.UserContextRequired);
  }

  [Fact]
  public async Task GetCurrentCashSession_ShouldReturnNull_WhenNoOpenSession()
  {
    var handler = new GetCurrentCashSessionHandler(new FakeCashRepo(), Authed());

    var result = await handler.Handle();

    result.IsSuccess.Should().BeTrue();
    result.Value.Should().BeNull();
  }

  [Fact]
  public async Task GetCurrentCashSession_ShouldReturnSession_WhenOpenSessionExists()
  {
    var session = CreateOpenSession(openingBalance: 1500);
    var repo = new FakeCashRepo { OpenSession = session };

    var handler = new GetCurrentCashSessionHandler(repo, Authed());

    var result = await handler.Handle();

    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeNull();
    result.Value!.OpeningBalance.Should().Be(1500);
    result.Value.Status.Should().Be("Open");
  }

  // ── Helpers ──────────────────────────────────────────────────────────────

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

  private static CashSession CreateOpenSession(decimal openingBalance = 500)
    => CashSession.Create(
      Guid.NewGuid(),
      new BusinessId(Guid.NewGuid()),
      new BranchId(Guid.NewGuid()),
      Guid.NewGuid(),
      openingBalance,
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

  private sealed class FakeCashRepo : ICashSessionRepository
  {
    public bool HasOpen { get; set; }
    public CashSession? Session { get; set; }
    public CashSession? OpenSession { get; set; }
    public CashSession? Added { get; private set; }

    public Task<CashSession?> GetAsync(
      BusinessId businessId, Guid sessionId, CancellationToken cancellationToken = default)
      => Task.FromResult(Session?.Id == sessionId ? Session : null);

    public Task<CashSession?> GetOpenSessionAsync(
      BusinessId businessId, BranchId branchId, CancellationToken cancellationToken = default)
      => Task.FromResult(OpenSession);

    public Task<bool> HasOpenSessionAsync(
      BusinessId businessId, BranchId branchId, CancellationToken cancellationToken = default)
      => Task.FromResult(HasOpen);

    public Task AddAsync(CashSession session, CancellationToken cancellationToken = default)
    {
      Added = session;
      return Task.CompletedTask;
    }

    public Task<int> CountAsync(
      BusinessId businessId, CashSessionSearchCriteria criteria, CancellationToken cancellationToken = default)
      => Task.FromResult(0);

    public Task<IReadOnlyCollection<CashSession>> ListAsync(
      BusinessId businessId, CashSessionSearchCriteria criteria, CancellationToken cancellationToken = default)
      => Task.FromResult<IReadOnlyCollection<CashSession>>([]);

    public Task<IReadOnlyCollection<CashSession>> ExportAllAsync(
      BusinessId businessId, DateTimeOffset? dateFrom, DateTimeOffset? dateTo, CancellationToken cancellationToken = default)
      => Task.FromResult<IReadOnlyCollection<CashSession>>([]);
  }
}

#pragma warning restore CA1707
