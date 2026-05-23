#pragma warning disable CA1707

using FluentAssertions;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Contracts.Events;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Application.Expenses;
using SaasCommerce.Modules.Sales.Contracts.Events.V1;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Tests;

public sealed class ExpensesUseCaseTests
{
  private static readonly DateTimeOffset Now = new(2026, 5, 22, 10, 0, 0, TimeSpan.Zero);
  private static readonly Guid CategoryIdValue = Guid.Parse("CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCCCC");

  // ── CreateExpenseCategoryHandler ──────────────────────────────────────────

  [Fact]
  public async Task CreateExpenseCategory_ShouldFail_WhenNoBusinessContext()
  {
    var handler = new CreateExpenseCategoryHandler(
      new FakeCategoryRepo(), Anonymous(), new NoopUow(), new FixedClock());

    var result = await handler.Handle(new CreateExpenseCategoryCommand("Alquiler"));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(ExpenseErrors.UserContextRequired);
  }

  [Fact]
  public async Task CreateExpenseCategory_ShouldFail_WhenNameIsEmpty()
  {
    var handler = new CreateExpenseCategoryHandler(
      new FakeCategoryRepo(), Authed(), new NoopUow(), new FixedClock());

    var result = await handler.Handle(new CreateExpenseCategoryCommand("   "));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(ExpenseErrors.InvalidExpense);
  }

  [Fact]
  public async Task CreateExpenseCategory_ShouldFail_WhenDuplicateName()
  {
    var repo = new FakeCategoryRepo { NameExists = true };
    var handler = new CreateExpenseCategoryHandler(repo, Authed(), new NoopUow(), new FixedClock());

    var result = await handler.Handle(new CreateExpenseCategoryCommand("Alquiler"));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(ExpenseErrors.DuplicateCategoryName);
  }

  [Fact]
  public async Task CreateExpenseCategory_ShouldSucceed_AndPersist()
  {
    var repo = new FakeCategoryRepo();
    var handler = new CreateExpenseCategoryHandler(repo, Authed(), new NoopUow(), new FixedClock());

    var result = await handler.Handle(new CreateExpenseCategoryCommand("Servicios"));

    result.IsSuccess.Should().BeTrue();
    result.Value.Name.Should().Be("Servicios");
    result.Value.IsActive.Should().BeTrue();
    repo.Added.Should().NotBeNull();
  }

  // ── GetExpenseCategoriesHandler ───────────────────────────────────────────

  [Fact]
  public async Task GetExpenseCategories_ShouldFail_WhenNoBusinessContext()
  {
    var handler = new GetExpenseCategoriesHandler(new FakeCategoryRepo(), Anonymous());

    var result = await handler.Handle();

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(ExpenseErrors.UserContextRequired);
  }

  [Fact]
  public async Task GetExpenseCategories_ShouldReturnEmpty_WhenNoneExist()
  {
    var handler = new GetExpenseCategoriesHandler(new FakeCategoryRepo(), Authed());

    var result = await handler.Handle();

    result.IsSuccess.Should().BeTrue();
    result.Value.Should().BeEmpty();
  }

  [Fact]
  public async Task GetExpenseCategories_ShouldReturnCategories()
  {
    var bId = new BusinessId(Guid.NewGuid());
    var categories = new[]
    {
      ExpenseCategory.Create(Guid.NewGuid(), bId, "Alquiler", Now),
      ExpenseCategory.Create(Guid.NewGuid(), bId, "Servicios", Now)
    };
    var repo = new FakeCategoryRepo { Categories = categories };
    var handler = new GetExpenseCategoriesHandler(repo, Authed());

    var result = await handler.Handle();

    result.IsSuccess.Should().BeTrue();
    result.Value.Should().HaveCount(2);
  }

  // ── CreateOperatingExpenseHandler ────────────────────────────────────────

  [Fact]
  public async Task CreateExpense_ShouldFail_WhenNoBusinessContext()
  {
    var handler = BuildCreateHandler(Anonymous(), new FakeCategoryRepo(), new FakeCashRepo());

    var result = await handler.Handle(ValidCreateCommand());

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(ExpenseErrors.UserContextRequired);
  }

  [Fact]
  public async Task CreateExpense_ShouldFail_WhenInvalidPaymentMethod()
  {
    var handler = BuildCreateHandler(Authed(), new FakeCategoryRepo { Category = DefaultCategory() }, new FakeCashRepo());

    var result = await handler.Handle(ValidCreateCommand() with { PaymentMethod = "Bitcoins" });

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(ExpenseErrors.InvalidPaymentMethod);
  }

  [Fact]
  public async Task CreateExpense_ShouldFail_WhenInvalidStatus()
  {
    var handler = BuildCreateHandler(Authed(), new FakeCategoryRepo { Category = DefaultCategory() }, new FakeCashRepo());

    var result = await handler.Handle(ValidCreateCommand() with { Status = "Cancelled" });

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(ExpenseErrors.InvalidStatus);
  }

  [Fact]
  public async Task CreateExpense_ShouldFail_WhenAmountIsZero()
  {
    var handler = BuildCreateHandler(Authed(), new FakeCategoryRepo { Category = DefaultCategory() }, new FakeCashRepo());

    var result = await handler.Handle(ValidCreateCommand() with { Amount = 0 });

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(ExpenseErrors.InvalidAmount);
  }

  [Fact]
  public async Task CreateExpense_ShouldFail_WhenCategoryNotFound()
  {
    var handler = BuildCreateHandler(Authed(), new FakeCategoryRepo(), new FakeCashRepo());

    var result = await handler.Handle(ValidCreateCommand());

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(ExpenseErrors.CategoryNotFound);
  }

  [Fact]
  public async Task CreateExpense_ShouldFail_WhenCashPaymentAndNoOpenSession()
  {
    var handler = BuildCreateHandler(
      Authed(),
      new FakeCategoryRepo { Category = DefaultCategory() },
      new FakeCashRepo() /* no open session */);

    var result = await handler.Handle(ValidCreateCommand() with { Status = "Paid", PaymentMethod = "Cash" });

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(ExpenseErrors.CashSessionRequired);
  }

  [Fact]
  public async Task CreateExpense_ShouldSucceed_AsPending()
  {
    var expenseRepo = new FakeExpenseRepo();
    var handler = BuildCreateHandler(
      Authed(),
      new FakeCategoryRepo { Category = DefaultCategory() },
      new FakeCashRepo(),
      expenseRepo);
    var outbox = new RecordingOutbox();
    handler = BuildCreateHandler(Authed(), new FakeCategoryRepo { Category = DefaultCategory() }, new FakeCashRepo(), expenseRepo, outbox);

    var result = await handler.Handle(ValidCreateCommand() with { Status = "Pending" });

    result.IsSuccess.Should().BeTrue();
    result.Value.Status.Should().Be("Pending");
    expenseRepo.Added.Should().NotBeNull();
    outbox.Events.OfType<OperatingExpenseCreatedEventV1>().Should().ContainSingle();
  }

  [Fact]
  public async Task CreateExpense_ShouldSucceed_AsPaidCash_WithCashMovement()
  {
    var session = CreateOpenSession();
    var cashRepo = new FakeCashRepo { OpenSession = session };
    var expenseRepo = new FakeExpenseRepo();
    var outbox = new RecordingOutbox();
    var handler = BuildCreateHandler(Authed(), new FakeCategoryRepo { Category = DefaultCategory() }, cashRepo, expenseRepo, outbox);

    var result = await handler.Handle(ValidCreateCommand() with { Status = "Paid", PaymentMethod = "Cash" });

    result.IsSuccess.Should().BeTrue();
    result.Value.Status.Should().Be("Paid");
    result.Value.CashSessionId.Should().NotBeNull();
    result.Value.CashMovementId.Should().NotBeNull();
    session.Movements.Should().ContainSingle(m => m.Type == CashMovementType.CashOut);
    outbox.Events.OfType<OperatingExpenseCreatedEventV1>().Should().ContainSingle();
  }

  [Fact]
  public async Task CreateExpense_ShouldSucceed_AsPaidTransfer_WithoutCashSession()
  {
    var expenseRepo = new FakeExpenseRepo();
    var handler = BuildCreateHandler(Authed(), new FakeCategoryRepo { Category = DefaultCategory() }, new FakeCashRepo(), expenseRepo);

    var result = await handler.Handle(ValidCreateCommand() with { Status = "Paid", PaymentMethod = "Transfer" });

    result.IsSuccess.Should().BeTrue();
    result.Value.Status.Should().Be("Paid");
    result.Value.CashSessionId.Should().BeNull();
    result.Value.CashMovementId.Should().BeNull();
  }

  // ── PayOperatingExpenseHandler ────────────────────────────────────────────

  [Fact]
  public async Task PayExpense_ShouldFail_WhenNoBusinessContext()
  {
    var handler = new PayOperatingExpenseHandler(
      new FakeExpenseRepo(), new FakeCategoryRepo(), new FakeCashRepo(),
      Anonymous(), new RecordingOutbox(), new NoopUow(), new FixedClock());

    var result = await handler.Handle(new PayOperatingExpenseCommand(Guid.NewGuid(), "Cash"));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(ExpenseErrors.UserContextRequired);
  }

  [Fact]
  public async Task PayExpense_ShouldFail_WhenInvalidPaymentMethod()
  {
    var handler = new PayOperatingExpenseHandler(
      new FakeExpenseRepo(), new FakeCategoryRepo(), new FakeCashRepo(),
      Authed(), new RecordingOutbox(), new NoopUow(), new FixedClock());

    var result = await handler.Handle(new PayOperatingExpenseCommand(Guid.NewGuid(), "BTC"));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(ExpenseErrors.InvalidPaymentMethod);
  }

  [Fact]
  public async Task PayExpense_ShouldFail_WhenExpenseNotFound()
  {
    var handler = new PayOperatingExpenseHandler(
      new FakeExpenseRepo(), new FakeCategoryRepo(), new FakeCashRepo(),
      Authed(), new RecordingOutbox(), new NoopUow(), new FixedClock());

    var result = await handler.Handle(new PayOperatingExpenseCommand(Guid.NewGuid(), "Transfer"));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(ExpenseErrors.ExpenseNotFound);
  }

  [Fact]
  public async Task PayExpense_ShouldFail_WhenExpenseAlreadyCancelled()
  {
    var expense = NewPendingExpense();
    expense.Cancel(Now.AddMinutes(-5));
    var expenseRepo = new FakeExpenseRepo { Expense = expense };

    var handler = new PayOperatingExpenseHandler(
      expenseRepo, new FakeCategoryRepo(), new FakeCashRepo(),
      Authed(), new RecordingOutbox(), new NoopUow(), new FixedClock());

    var result = await handler.Handle(new PayOperatingExpenseCommand(expense.Id, "Transfer"));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(ExpenseErrors.ExpenseAlreadyCancelled);
  }

  [Fact]
  public async Task PayExpense_ShouldBeIdempotent_WhenAlreadyPaid()
  {
    var expense = NewPendingExpense();
    expense.Pay(Guid.NewGuid(), Now.AddMinutes(-5));
    var expenseRepo = new FakeExpenseRepo { Expense = expense };
    var outbox = new RecordingOutbox();

    var handler = new PayOperatingExpenseHandler(
      expenseRepo, new FakeCategoryRepo { Category = DefaultCategory() }, new FakeCashRepo(),
      Authed(), outbox, new NoopUow(), new FixedClock());

    var result = await handler.Handle(new PayOperatingExpenseCommand(expense.Id, "Transfer"));

    result.IsSuccess.Should().BeTrue();
    result.Value.Status.Should().Be("Paid");
    outbox.Events.Should().BeEmpty(); // no new event for already-paid
  }

  [Fact]
  public async Task PayExpense_ShouldFail_WhenCashAndNoOpenSession()
  {
    var expense = NewPendingExpense();
    var expenseRepo = new FakeExpenseRepo { Expense = expense };

    var handler = new PayOperatingExpenseHandler(
      expenseRepo, new FakeCategoryRepo(), new FakeCashRepo(),
      Authed(), new RecordingOutbox(), new NoopUow(), new FixedClock());

    var result = await handler.Handle(new PayOperatingExpenseCommand(expense.Id, "Cash"));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(ExpenseErrors.CashSessionRequired);
  }

  [Fact]
  public async Task PayExpense_ShouldSucceed_Transfer_AndPublishEvent()
  {
    var expense = NewPendingExpense();
    var expenseRepo = new FakeExpenseRepo { Expense = expense };
    var outbox = new RecordingOutbox();

    var handler = new PayOperatingExpenseHandler(
      expenseRepo, new FakeCategoryRepo { Category = DefaultCategory() }, new FakeCashRepo(),
      Authed(), outbox, new NoopUow(), new FixedClock());

    var result = await handler.Handle(new PayOperatingExpenseCommand(expense.Id, "Transfer"));

    result.IsSuccess.Should().BeTrue();
    result.Value.Status.Should().Be("Paid");
    outbox.Events.OfType<OperatingExpensePaidEventV1>().Should().ContainSingle();
  }

  [Fact]
  public async Task PayExpense_ShouldSucceed_Cash_WithCashMovement()
  {
    var expense = NewPendingExpense();
    var expenseRepo = new FakeExpenseRepo { Expense = expense };
    var session = CreateOpenSession();
    var cashRepo = new FakeCashRepo { OpenSession = session };
    var outbox = new RecordingOutbox();

    var handler = new PayOperatingExpenseHandler(
      expenseRepo, new FakeCategoryRepo { Category = DefaultCategory() }, cashRepo,
      Authed(), outbox, new NoopUow(), new FixedClock());

    var result = await handler.Handle(new PayOperatingExpenseCommand(expense.Id, "Cash"));

    result.IsSuccess.Should().BeTrue();
    result.Value.Status.Should().Be("Paid");
    result.Value.CashSessionId.Should().NotBeNull();
    result.Value.CashMovementId.Should().NotBeNull();
    session.Movements.Should().ContainSingle(m => m.Type == CashMovementType.CashOut);
    outbox.Events.OfType<OperatingExpensePaidEventV1>().Should().ContainSingle();
  }

  // ── CancelOperatingExpenseHandler ────────────────────────────────────────

  [Fact]
  public async Task CancelExpense_ShouldFail_WhenNoBusinessContext()
  {
    var handler = new CancelOperatingExpenseHandler(
      new FakeExpenseRepo(), new FakeCategoryRepo(), Anonymous(),
      new RecordingOutbox(), new NoopUow(), new FixedClock());

    var result = await handler.Handle(new CancelOperatingExpenseCommand(Guid.NewGuid()));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(ExpenseErrors.UserContextRequired);
  }

  [Fact]
  public async Task CancelExpense_ShouldFail_WhenExpenseNotFound()
  {
    var handler = new CancelOperatingExpenseHandler(
      new FakeExpenseRepo(), new FakeCategoryRepo(), Authed(),
      new RecordingOutbox(), new NoopUow(), new FixedClock());

    var result = await handler.Handle(new CancelOperatingExpenseCommand(Guid.NewGuid()));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(ExpenseErrors.ExpenseNotFound);
  }

  [Fact]
  public async Task CancelExpense_ShouldFail_WhenAlreadyPaid()
  {
    var expense = NewPendingExpense();
    expense.Pay(Guid.NewGuid(), Now.AddMinutes(-1));
    var expenseRepo = new FakeExpenseRepo { Expense = expense };

    var handler = new CancelOperatingExpenseHandler(
      expenseRepo, new FakeCategoryRepo(), Authed(),
      new RecordingOutbox(), new NoopUow(), new FixedClock());

    var result = await handler.Handle(new CancelOperatingExpenseCommand(expense.Id));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(ExpenseErrors.CannotCancelPaidExpense);
  }

  [Fact]
  public async Task CancelExpense_ShouldBeIdempotent_WhenAlreadyCancelled()
  {
    var expense = NewPendingExpense();
    expense.Cancel(Now.AddMinutes(-1));
    var expenseRepo = new FakeExpenseRepo { Expense = expense };
    var outbox = new RecordingOutbox();

    var handler = new CancelOperatingExpenseHandler(
      expenseRepo, new FakeCategoryRepo { Category = DefaultCategory() }, Authed(),
      outbox, new NoopUow(), new FixedClock());

    var result = await handler.Handle(new CancelOperatingExpenseCommand(expense.Id));

    result.IsSuccess.Should().BeTrue();
    result.Value.Status.Should().Be("Cancelled");
    outbox.Events.Should().BeEmpty(); // no new event for already-cancelled
  }

  [Fact]
  public async Task CancelExpense_ShouldSucceed_AndPublishEvent()
  {
    var expense = NewPendingExpense();
    var expenseRepo = new FakeExpenseRepo { Expense = expense };
    var outbox = new RecordingOutbox();

    var handler = new CancelOperatingExpenseHandler(
      expenseRepo, new FakeCategoryRepo { Category = DefaultCategory() }, Authed(),
      outbox, new NoopUow(), new FixedClock());

    var result = await handler.Handle(new CancelOperatingExpenseCommand(expense.Id));

    result.IsSuccess.Should().BeTrue();
    result.Value.Status.Should().Be("Cancelled");
    outbox.Events.OfType<OperatingExpenseCancelledEventV1>().Should().ContainSingle();
  }

  // ── GetOperatingExpenseDetailHandler ─────────────────────────────────────

  [Fact]
  public async Task GetExpenseDetail_ShouldFail_WhenNoBusinessContext()
  {
    var handler = new GetOperatingExpenseDetailHandler(
      new FakeExpenseRepo(), new FakeCategoryRepo(), Anonymous());

    var result = await handler.Handle(new GetOperatingExpenseDetailQuery(Guid.NewGuid()));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(ExpenseErrors.UserContextRequired);
  }

  [Fact]
  public async Task GetExpenseDetail_ShouldFail_WhenExpenseNotFound()
  {
    var handler = new GetOperatingExpenseDetailHandler(
      new FakeExpenseRepo(), new FakeCategoryRepo(), Authed());

    var result = await handler.Handle(new GetOperatingExpenseDetailQuery(Guid.NewGuid()));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(ExpenseErrors.ExpenseNotFound);
  }

  [Fact]
  public async Task GetExpenseDetail_ShouldSucceed_WithCategoryName()
  {
    var expense = NewPendingExpense();
    var handler = new GetOperatingExpenseDetailHandler(
      new FakeExpenseRepo { Expense = expense },
      new FakeCategoryRepo { Category = DefaultCategory() },
      Authed());

    var result = await handler.Handle(new GetOperatingExpenseDetailQuery(expense.Id));

    result.IsSuccess.Should().BeTrue();
    result.Value.Id.Should().Be(expense.Id);
    result.Value.CategoryName.Should().Be("Servicios");
  }

  // ── GetOperatingExpensesHandler ───────────────────────────────────────────

  [Fact]
  public async Task GetExpenses_ShouldFail_WhenNoBusinessContext()
  {
    var handler = new GetOperatingExpensesHandler(
      new FakeExpenseRepo(), new FakeCategoryRepo(), Anonymous());

    var result = await handler.Handle(new GetOperatingExpensesQuery(null, null, null, null, null, null, 1, 20));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(ExpenseErrors.UserContextRequired);
  }

  [Fact]
  public async Task GetExpenses_ShouldReturnEmptyPage_WhenNoneExist()
  {
    var handler = new GetOperatingExpensesHandler(
      new FakeExpenseRepo(), new FakeCategoryRepo(), Authed());

    var result = await handler.Handle(new GetOperatingExpensesQuery(null, null, null, null, null, null, 1, 20));

    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().BeEmpty();
    result.Value.TotalCount.Should().Be(0);
  }

  // ── Helpers & Test Doubles ────────────────────────────────────────────────

  private static CreateOperatingExpenseHandler BuildCreateHandler(
    ICurrentUserService user,
    FakeCategoryRepo categoryRepo,
    FakeCashRepo cashRepo,
    FakeExpenseRepo? expenseRepo = null,
    RecordingOutbox? outbox = null)
    => new(
      expenseRepo ?? new FakeExpenseRepo(),
      categoryRepo,
      cashRepo,
      user,
      outbox ?? new RecordingOutbox(),
      new NoopUow(),
      new FixedClock());

  private static CreateOperatingExpenseCommand ValidCreateCommand()
    => new(
      Guid.NewGuid(),
      CategoryIdValue,
      "Pago de electricidad",
      1500m,
      "Cash",
      "Pending",
      Now,
      null);

  private static FakeUser Authed()
    => new()
    {
      BusinessId = Guid.NewGuid(),
      UserId = Guid.NewGuid(),
      BranchId = Guid.NewGuid(),
      IsAuthenticated = true
    };

  private static FakeUser Anonymous()
    => new() { IsAuthenticated = false };

  private static OperatingExpense NewPendingExpense()
    => OperatingExpense.CreatePending(
      Guid.NewGuid(),
      new BusinessId(Guid.NewGuid()),
      new BranchId(Guid.NewGuid()),
      Guid.NewGuid(),
      CategoryIdValue,
      "Factura agua",
      800m,
      ExpensePaymentMethod.Transfer,
      Now,
      null,
      Now);

  private static ExpenseCategory DefaultCategory()
    => ExpenseCategory.Create(CategoryIdValue, new BusinessId(Guid.NewGuid()), "Servicios", Now);

  private static CashSession CreateOpenSession()
    => CashSession.Create(
      Guid.NewGuid(),
      new BusinessId(Guid.NewGuid()),
      new BranchId(Guid.NewGuid()),
      Guid.NewGuid(),
      500m,
      null,
      Now);

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

  private sealed class FakeCategoryRepo : IExpenseCategoryRepository
  {
    public bool NameExists { get; set; }
    public ExpenseCategory? Category { get; set; }
    public ExpenseCategory? Added { get; private set; }
    public IReadOnlyCollection<ExpenseCategory> Categories { get; set; } = [];

    public Task<ExpenseCategory?> GetAsync(
      BusinessId businessId, Guid categoryId, CancellationToken cancellationToken = default)
      => Task.FromResult(Category?.Id == categoryId ? Category : null);

    public Task<bool> ExistsByNameAsync(
      BusinessId businessId, string name, CancellationToken cancellationToken = default)
      => Task.FromResult(NameExists);

    public Task AddAsync(ExpenseCategory category, CancellationToken cancellationToken = default)
    {
      Added = category;
      return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<ExpenseCategory>> ListAsync(
      BusinessId businessId, CancellationToken cancellationToken = default)
      => Task.FromResult(Categories);
  }

  private sealed class FakeExpenseRepo : IOperatingExpenseRepository
  {
    public OperatingExpense? Expense { get; set; }
    public OperatingExpense? Added { get; private set; }

    public Task<OperatingExpense?> GetAsync(
      BusinessId businessId, Guid expenseId, CancellationToken cancellationToken = default)
      => Task.FromResult(Expense?.Id == expenseId ? Expense : null);

    public Task AddAsync(OperatingExpense expense, CancellationToken cancellationToken = default)
    {
      Added = expense;
      return Task.CompletedTask;
    }

    public Task<int> CountAsync(
      BusinessId businessId, OperatingExpenseSearchCriteria criteria, CancellationToken cancellationToken = default)
      => Task.FromResult(0);

    public Task<IReadOnlyCollection<OperatingExpense>> ListAsync(
      BusinessId businessId, OperatingExpenseSearchCriteria criteria, CancellationToken cancellationToken = default)
      => Task.FromResult<IReadOnlyCollection<OperatingExpense>>([]);

    public Task<IReadOnlyCollection<OperatingExpense>> ListForSummaryAsync(
      BusinessId businessId, DateTimeOffset dateFrom, DateTimeOffset dateTo,
      Guid? branchId, CancellationToken cancellationToken = default)
      => Task.FromResult<IReadOnlyCollection<OperatingExpense>>([]);
  }

  private sealed class FakeCashRepo : ICashSessionRepository
  {
    public CashSession? OpenSession { get; set; }

    public Task<CashSession?> GetAsync(
      BusinessId businessId, Guid cashSessionId, CancellationToken cancellationToken = default)
      => Task.FromResult<CashSession?>(null);

    public Task<CashSession?> GetOpenSessionAsync(
      BusinessId businessId, BranchId branchId, CancellationToken cancellationToken = default)
      => Task.FromResult(OpenSession);

    public Task<bool> HasOpenSessionAsync(
      BusinessId businessId, BranchId branchId, CancellationToken cancellationToken = default)
      => Task.FromResult(OpenSession is not null);

    public Task AddAsync(CashSession session, CancellationToken cancellationToken = default)
      => Task.CompletedTask;

    public Task<int> CountAsync(
      BusinessId businessId, CashSessionSearchCriteria criteria, CancellationToken cancellationToken = default)
      => Task.FromResult(0);

    public Task<IReadOnlyCollection<CashSession>> ListAsync(
      BusinessId businessId, CashSessionSearchCriteria criteria, CancellationToken cancellationToken = default)
      => Task.FromResult<IReadOnlyCollection<CashSession>>([]);
  }
}

#pragma warning restore CA1707
