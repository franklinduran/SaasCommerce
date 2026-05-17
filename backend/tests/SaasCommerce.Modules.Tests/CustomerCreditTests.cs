#pragma warning disable CA1707

using FluentAssertions;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Observability;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Contracts.Events;
using SaasCommerce.Modules.Customers.Application.Abstractions;
using SaasCommerce.Modules.Customers.Application.Credits;
using SaasCommerce.Modules.Customers.Domain;
using SaasCommerce.Modules.Customers.Domain.Credits;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Contracts.Events.V1;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Tests;

public sealed class CustomerCreditTests
{
  private static readonly DateTimeOffset Now = new(2026, 5, 17, 12, 0, 0, TimeSpan.Zero);

  [Fact]
  public void CustomerCreditAccount_ShouldIncreaseBalance_WhenDebitIsApplied()
  {
    var scenario = TestScenario.Create();
    var account = scenario.CreateAccount(creditLimit: 500);

    var movement = account.ApplyDebit(Guid.NewGuid(), Guid.NewGuid(), 200, "Venta fiada", scenario.UserId, Now);

    account.CurrentBalance.Should().Be(200);
    movement.PreviousBalance.Should().Be(0);
    movement.NewBalance.Should().Be(200);
    movement.Type.Should().Be(CustomerCreditMovementType.Debit);
  }

  [Fact]
  public void CustomerCreditAccount_ShouldDecreaseBalance_WhenPaymentIsApplied()
  {
    var scenario = TestScenario.Create();
    var account = scenario.CreateAccount(creditLimit: 500);
    account.ApplyDebit(Guid.NewGuid(), Guid.NewGuid(), 200, null, scenario.UserId, Now);

    var movement = account.ApplyPayment(Guid.NewGuid(), Guid.NewGuid(), 75, "Abono", scenario.UserId, Now);

    account.CurrentBalance.Should().Be(125);
    movement.PreviousBalance.Should().Be(200);
    movement.NewBalance.Should().Be(125);
    movement.Type.Should().Be(CustomerCreditMovementType.Payment);
  }

  [Fact]
  public void CustomerCreditAccount_ShouldThrow_WhenPaymentExceedsBalance()
  {
    var scenario = TestScenario.Create();
    var account = scenario.CreateAccount(creditLimit: 500);
    account.ApplyDebit(Guid.NewGuid(), Guid.NewGuid(), 100, null, scenario.UserId, Now);

    var act = () => account.ApplyPayment(Guid.NewGuid(), Guid.NewGuid(), 101, null, scenario.UserId, Now);

    act.Should().Throw<InvalidOperationException>();
  }

  [Fact]
  public void CustomerCreditAccount_ShouldThrow_WhenCreditLimitIsExceeded()
  {
    var scenario = TestScenario.Create();
    var account = scenario.CreateAccount(creditLimit: 100);

    var act = () => account.ApplyDebit(Guid.NewGuid(), Guid.NewGuid(), 101, null, scenario.UserId, Now);

    act.Should().Throw<InvalidOperationException>();
  }

  [Fact]
  public void CustomerCreditAccount_ShouldThrow_WhenAccountIsBlocked()
  {
    var scenario = TestScenario.Create();
    var account = scenario.CreateAccount(creditLimit: 0);
    account.Block(Now);

    var act = () => account.ApplyDebit(Guid.NewGuid(), Guid.NewGuid(), 100, null, scenario.UserId, Now);

    act.Should().Throw<InvalidOperationException>();
  }

  [Fact]
  public async Task RegisterCustomerPaymentHandler_ShouldCreatePaymentMovement_WhenAmountIsValid()
  {
    var scenario = TestScenario.Create();
    await scenario.Customers.AddAsync(scenario.CreateCustomer());
    var account = scenario.CreateAccount(creditLimit: 0);
    account.ApplyDebit(Guid.NewGuid(), Guid.NewGuid(), 250, null, scenario.UserId, Now);
    await scenario.Credits.AddAccountAsync(account);
    var useCase = scenario.CreateRegisterPaymentUseCase();

    var result = await useCase.ExecuteAsync(new RegisterCustomerPaymentCommand(scenario.CustomerId, 100, "Abono"));

    result.IsSuccess.Should().BeTrue();
    result.Value.NewBalance.Should().Be(150);
    scenario.Credits.Payments.Should().ContainSingle();
    scenario.Credits.Movements.Should().ContainSingle(movement => movement.Type == CustomerCreditMovementType.Payment);
    scenario.Outbox.Events.Should().ContainSingle(@event => @event is SaasCommerce.Modules.Customers.Contracts.Events.V1.CustomerPaymentRegisteredEventV1);
  }

  [Fact]
  public async Task RegisterCustomerPaymentHandler_ShouldRejectPayment_WhenAmountExceedsBalance()
  {
    var scenario = TestScenario.Create();
    await scenario.Customers.AddAsync(scenario.CreateCustomer());
    var account = scenario.CreateAccount(creditLimit: 0);
    account.ApplyDebit(Guid.NewGuid(), Guid.NewGuid(), 50, null, scenario.UserId, Now);
    await scenario.Credits.AddAccountAsync(account);
    var useCase = scenario.CreateRegisterPaymentUseCase();

    var result = await useCase.ExecuteAsync(new RegisterCustomerPaymentCommand(scenario.CustomerId, 100, null));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(CustomerCreditErrors.PaymentExceedsBalance);
  }

  [Fact]
  public async Task RegisterCreditSaleConsumerUseCase_ShouldCreateDebitMovement_WhenCreditSaleCompleted()
  {
    var scenario = TestScenario.Create();
    var sale = scenario.CreateProcessingSale("Credit");
    await scenario.Sales.AddAsync(sale);
    await scenario.Credits.AddAccountAsync(scenario.CreateAccount(creditLimit: 0));
    var useCase = scenario.CreateRegisterCreditSaleUseCase();

    var result = await useCase.ExecuteAsync(scenario.SaleCompleted());

    result.IsSuccess.Should().BeTrue();
    scenario.Credits.Movements.Should().ContainSingle(movement =>
      movement.Type == CustomerCreditMovementType.Debit &&
      movement.SaleId == scenario.SaleId);
  }

  [Fact]
  public async Task RegisterCreditSaleConsumerUseCase_ShouldNotDuplicateMovement_WhenMessageIsDuplicated()
  {
    var scenario = TestScenario.Create();
    var sale = scenario.CreateProcessingSale("Credit");
    await scenario.Sales.AddAsync(sale);
    await scenario.Credits.AddAccountAsync(scenario.CreateAccount(creditLimit: 0));
    var useCase = scenario.CreateRegisterCreditSaleUseCase();
    var message = scenario.SaleCompleted();

    await useCase.ExecuteAsync(message);
    await useCase.ExecuteAsync(message);

    scenario.Credits.Movements.Where(movement => movement.SaleId == scenario.SaleId).Should().ContainSingle();
  }

  private sealed class TestScenario
  {
    private TestScenario()
    {
      CurrentUser = new FakeCurrentUserService(BusinessId, BranchId, UserId);
    }

    public Guid BusinessId { get; } = Guid.NewGuid();
    public Guid BranchId { get; } = Guid.NewGuid();
    public Guid UserId { get; } = Guid.NewGuid();
    public Guid CustomerId { get; } = Guid.NewGuid();
    public Guid SaleId { get; } = Guid.NewGuid();
    public Guid ProductId { get; } = Guid.NewGuid();
    public Guid CorrelationId { get; } = Guid.NewGuid();
    public FakeCurrentUserService CurrentUser { get; }
    public InMemoryCustomerRepository Customers { get; } = new();
    public InMemoryCreditRepository Credits { get; } = new();
    public InMemorySaleRepository Sales { get; } = new();
    public RecordingOutboxWriter Outbox { get; } = new();
    public FixedClock Clock { get; } = new();
    public NoopUnitOfWork UnitOfWork { get; } = new();
    public FixedCorrelationIdProvider Correlation { get; } = new();

    public static TestScenario Create() => new();

    public Customer CreateCustomer()
      => new(CustomerId, new BusinessId(BusinessId), "Cliente Fiado", "8095550000", null, Now);

    public CustomerCreditAccount CreateAccount(decimal creditLimit)
      => new(Guid.NewGuid(), new BusinessId(BusinessId), CustomerId, creditLimit, Now);

    public Sale CreateProcessingSale(string paymentMethod)
    {
      var sale = Sale.Create(
        SaleId,
        new BusinessId(BusinessId),
        new BranchId(BranchId),
        UserId,
        [new SaleLine(ProductId, 2, 100)],
        paymentMethod,
        Now);
      sale.AssignCustomer(CustomerId);
      sale.MarkAsProcessing(Now);
      return sale;
    }

    public SaleCompletedEventV1 SaleCompleted()
      => new(
        Guid.NewGuid(),
        CorrelationId,
        SaleId,
        BusinessId,
        BranchId,
        UserId,
        Guid.NewGuid(),
        Guid.NewGuid(),
        200,
        Now);

    public RegisterCustomerPaymentUseCase CreateRegisterPaymentUseCase()
      => new(Customers, Credits, CurrentUser, Outbox, Correlation, Clock, UnitOfWork);

    public RegisterCreditSaleUseCase CreateRegisterCreditSaleUseCase()
      => new(Sales, Credits, Outbox, Clock, UnitOfWork);
  }

  private sealed class FakeCurrentUserService(Guid businessId, Guid branchId, Guid userId) : ICurrentUserService
  {
    public Guid? UserId { get; } = userId;
    public Guid? BusinessId { get; } = businessId;
    public Guid? BranchId { get; } = branchId;
    public IReadOnlyCollection<string> Roles => ["Admin"];
    public bool IsAuthenticated => true;
  }

  private sealed class FixedClock : IClock
  {
    public DateTimeOffset UtcNow => Now;
  }

  private sealed class NoopUnitOfWork : IUnitOfWork
  {
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
      => Task.FromResult(1);
  }

  private sealed class FixedCorrelationIdProvider : ICorrelationIdProvider
  {
    public string CorrelationId { get; } = Guid.NewGuid().ToString("D");
  }

  private sealed class RecordingOutboxWriter : IOutboxWriter
  {
    public List<IIntegrationEvent> Events { get; } = [];

    public Task AddAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
      where TEvent : class, IIntegrationEvent
    {
      Events.Add(integrationEvent);
      return Task.CompletedTask;
    }
  }

  private sealed class InMemoryCustomerRepository : ICustomerRepository
  {
    private readonly Dictionary<(Guid BusinessId, Guid CustomerId), Customer> customers = [];

    public Task<Customer?> GetAsync(BusinessId businessId, Guid customerId, CancellationToken cancellationToken = default)
      => Task.FromResult(customers.TryGetValue((businessId.Value, customerId), out var customer) ? customer : null);

    public Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
    {
      customers[(customer.BusinessId.Value, customer.Id)] = customer;
      return Task.CompletedTask;
    }

    public Task<int> CountAsync(BusinessId businessId, CustomerSearchCriteria criteria, CancellationToken cancellationToken = default)
      => Task.FromResult(customers.Values.Count(customer => customer.BusinessId == businessId));

    public Task<IReadOnlyCollection<Customer>> ListAsync(BusinessId businessId, CustomerSearchCriteria criteria, CancellationToken cancellationToken = default)
      => Task.FromResult<IReadOnlyCollection<Customer>>(customers.Values.Where(customer => customer.BusinessId == businessId).ToArray());
  }

  private sealed class InMemoryCreditRepository : ICustomerCreditRepository
  {
    private readonly Dictionary<(Guid BusinessId, Guid CustomerId), CustomerCreditAccount> accounts = [];

    public List<CustomerCreditMovement> Movements { get; } = [];
    public List<CustomerPayment> Payments { get; } = [];

    public Task<CustomerCreditAccount?> GetAccountAsync(BusinessId businessId, Guid customerId, CancellationToken cancellationToken = default)
      => Task.FromResult(accounts.TryGetValue((businessId.Value, customerId), out var account) ? account : null);

    public Task<IReadOnlyDictionary<Guid, CustomerCreditAccount>> ListAccountsAsync(BusinessId businessId, IReadOnlyCollection<Guid> customerIds, CancellationToken cancellationToken = default)
      => Task.FromResult<IReadOnlyDictionary<Guid, CustomerCreditAccount>>(
        accounts.Values
          .Where(account => account.BusinessId == businessId && customerIds.Contains(account.CustomerId))
          .ToDictionary(account => account.CustomerId));

    public Task AddAccountAsync(CustomerCreditAccount account, CancellationToken cancellationToken = default)
    {
      accounts[(account.BusinessId.Value, account.CustomerId)] = account;
      return Task.CompletedTask;
    }

    public Task AddMovementAsync(CustomerCreditMovement movement, CancellationToken cancellationToken = default)
    {
      Movements.Add(movement);
      return Task.CompletedTask;
    }

    public Task AddPaymentAsync(CustomerPayment payment, CancellationToken cancellationToken = default)
    {
      Payments.Add(payment);
      return Task.CompletedTask;
    }

    public Task<bool> HasDebitForSaleAsync(BusinessId businessId, Guid saleId, CancellationToken cancellationToken = default)
      => Task.FromResult(Movements.Any(movement =>
        movement.BusinessId == businessId &&
        movement.SaleId == saleId &&
        movement.Type == CustomerCreditMovementType.Debit));

    public Task<bool> HasPaymentAsync(BusinessId businessId, Guid paymentId, CancellationToken cancellationToken = default)
      => Task.FromResult(Payments.Any(payment => payment.BusinessId == businessId && payment.Id == paymentId));

    public Task<int> CountMovementsAsync(BusinessId businessId, Guid customerId, CancellationToken cancellationToken = default)
      => Task.FromResult(Movements.Count(movement => movement.BusinessId == businessId && movement.CustomerId == customerId));

    public Task<IReadOnlyCollection<CustomerCreditMovement>> ListMovementsAsync(BusinessId businessId, Guid customerId, int page, int pageSize, CancellationToken cancellationToken = default)
      => Task.FromResult<IReadOnlyCollection<CustomerCreditMovement>>(
        Movements
          .Where(movement => movement.BusinessId == businessId && movement.CustomerId == customerId)
          .Skip((page - 1) * pageSize)
          .Take(pageSize)
          .ToArray());
  }

  private sealed class InMemorySaleRepository : ISaleRepository
  {
    private readonly Dictionary<(Guid BusinessId, Guid SaleId), Sale> sales = [];

    public Task<Sale?> GetAsync(BusinessId businessId, Guid saleId, CancellationToken cancellationToken = default)
      => Task.FromResult(sales.TryGetValue((businessId.Value, saleId), out var sale) ? sale : null);

    public Task AddAsync(Sale sale, CancellationToken cancellationToken = default)
    {
      sales[(sale.BusinessId.Value, sale.Id)] = sale;
      return Task.CompletedTask;
    }

    public Task<int> CountAsync(BusinessId businessId, SaleSearchCriteria criteria, CancellationToken cancellationToken = default)
      => Task.FromResult(sales.Values.Count(sale => sale.BusinessId == businessId));

    public Task<IReadOnlyCollection<Sale>> ListAsync(BusinessId businessId, SaleSearchCriteria criteria, CancellationToken cancellationToken = default)
      => Task.FromResult<IReadOnlyCollection<Sale>>(sales.Values.Where(sale => sale.BusinessId == businessId).ToArray());
  }
}

#pragma warning restore CA1707
