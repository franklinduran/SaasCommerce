#pragma warning disable CA1707

using FluentAssertions;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Audit;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Realtime;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Contracts.Events;
using SaasCommerce.Modules.Catalog.Contracts.Sales;
using SaasCommerce.Modules.Customers.Application.Abstractions;
using SaasCommerce.Modules.Customers.Application.Customers;
using SaasCommerce.Modules.Customers.Application.Credits;
using SaasCommerce.Modules.Customers.Domain;
using SaasCommerce.Modules.Customers.Domain.Credits;
using SaasCommerce.Modules.Billing.Application.Abstractions;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Application.Sales;
using SaasCommerce.Modules.Sales.Contracts.Events.V1;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.Modules.Sales.Contracts.Responses;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Tests;

public sealed class CustomersSalesApiWorkflowTests
{
  private static readonly DateTimeOffset Now = new(2026, 5, 17, 10, 0, 0, TimeSpan.Zero);

  [Fact]
  public async Task CreateCustomer_ShouldCreateCustomer_WhenRequestIsValid()
  {
    var scenario = TestScenario.Create();
    var useCase = new CreateCustomerUseCase(
      scenario.Customers,
      scenario.CurrentUser,
      scenario.Clock,
      scenario.UnitOfWork);

    var result = await useCase.ExecuteAsync(new CreateCustomerCommand(
      "Maria Perez",
      "809-555-0102",
      "MARIA@EXAMPLE.COM"));

    result.IsSuccess.Should().BeTrue();
    result.Value.FullName.Should().Be("Maria Perez");
    result.Value.Phone.Should().Be("8095550102");
    result.Value.Email.Should().Be("maria@example.com");
    result.Value.BusinessId.Should().Be(scenario.BusinessId);
  }

  [Fact]
  public async Task CreateCustomer_ShouldFail_WhenNameIsEmpty()
  {
    var scenario = TestScenario.Create();
    var useCase = new CreateCustomerUseCase(
      scenario.Customers,
      scenario.CurrentUser,
      scenario.Clock,
      scenario.UnitOfWork);

    var result = await useCase.ExecuteAsync(new CreateCustomerCommand("", null, null));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(CustomerErrors.InvalidCustomer);
  }

  [Fact]
  public async Task GetCustomer_ShouldReturnNotFound_WhenCustomerDoesNotExist()
  {
    var scenario = TestScenario.Create();
    var useCase = new GetCustomerByIdUseCase(scenario.Customers, scenario.CurrentUser);

    var result = await useCase.ExecuteAsync(new GetCustomerByIdQuery(Guid.NewGuid()));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(CustomerErrors.CustomerNotFound);
  }

  [Fact]
  public async Task GetCustomer_ShouldNotReturnCustomerFromAnotherBusiness()
  {
    var scenario = TestScenario.Create();
    var otherBusiness = Guid.NewGuid();
    var customer = new Customer(
      Guid.NewGuid(),
      new BusinessId(otherBusiness),
      "Cliente Otro Negocio",
      null,
      null,
      Now);
    await scenario.Customers.AddAsync(customer);
    var useCase = new GetCustomerByIdUseCase(scenario.Customers, scenario.CurrentUser);

    var result = await useCase.ExecuteAsync(new GetCustomerByIdQuery(customer.Id));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(CustomerErrors.CustomerNotFound);
  }

  [Fact]
  public async Task UpdateCustomer_ShouldUpdateCustomer_WhenRequestIsValid()
  {
    var scenario = TestScenario.Create();
    var customer = await scenario.AddCustomerAsync();
    var useCase = new UpdateCustomerUseCase(
      scenario.Customers,
      scenario.CurrentUser,
      scenario.Clock,
      scenario.UnitOfWork);

    var result = await useCase.ExecuteAsync(new UpdateCustomerCommand(
      customer.Id,
      "Maria Perez Actualizada",
      "8295550102",
      "maria.actualizada@example.com",
      true));

    result.IsSuccess.Should().BeTrue();
    result.Value.FullName.Should().Be("Maria Perez Actualizada");
    result.Value.Phone.Should().Be("8295550102");
  }

  [Fact]
  public async Task DeleteCustomer_ShouldDeactivateCustomer_WhenCustomerExists()
  {
    var scenario = TestScenario.Create();
    var customer = await scenario.AddCustomerAsync();
    var useCase = new DeleteCustomerUseCase(
      scenario.Customers,
      scenario.CurrentUser,
      scenario.Clock,
      scenario.UnitOfWork);

    var result = await useCase.ExecuteAsync(new DeleteCustomerCommand(customer.Id));

    result.IsSuccess.Should().BeTrue();
    result.Value.IsActive.Should().BeFalse();
    customer.IsActive.Should().BeFalse();
  }

  [Fact]
  public async Task CreateSale_ShouldCreateSaleAsReceived_WhenRequestIsValid()
  {
    var scenario = TestScenario.Create();
    scenario.ProductPolicies.Add(Policy(scenario.ProductId, scenario.BusinessId, 125));
    var useCase = scenario.CreateSaleUseCase();

    var result = await useCase.ExecuteAsync(new CreateSaleCommand(
      scenario.BranchId,
      null,
      "Cash",
      [new CreateSaleItemCommand(scenario.ProductId, 2)]));

    result.IsSuccess.Should().BeTrue();
    result.Value.Status.Should().Be(SaleStatus.Received.ToString());
    result.Value.Total.Should().Be(250);
    scenario.Outbox.Events.Should().ContainSingle(@event => @event is SaleCreatedEventV1);
  }

  [Fact]
  public async Task CreateSale_ShouldCalculateTotalFromProducts_WhenItemsAreValid()
  {
    var scenario = TestScenario.Create();
    var secondProductId = Guid.NewGuid();
    scenario.ProductPolicies.Add(Policy(scenario.ProductId, scenario.BusinessId, 100));
    scenario.ProductPolicies.Add(Policy(secondProductId, scenario.BusinessId, 25));
    var useCase = scenario.CreateSaleUseCase();

    var result = await useCase.ExecuteAsync(new CreateSaleCommand(
      scenario.BranchId,
      null,
      "Cash",
      [
        new CreateSaleItemCommand(scenario.ProductId, 2),
        new CreateSaleItemCommand(secondProductId, 3)
      ]));

    result.IsSuccess.Should().BeTrue();
    result.Value.Total.Should().Be(275);
  }

  [Fact]
  public async Task CreateSale_ShouldStoreCustomer_WhenCustomerExists()
  {
    var scenario = TestScenario.Create();
    var customer = await scenario.AddCustomerAsync();
    scenario.ProductPolicies.Add(Policy(scenario.ProductId, scenario.BusinessId, 125));
    var useCase = scenario.CreateSaleUseCase();

    var result = await useCase.ExecuteAsync(new CreateSaleCommand(
      scenario.BranchId,
      customer.Id,
      "Cash",
      [new CreateSaleItemCommand(scenario.ProductId, 1)]));

    result.IsSuccess.Should().BeTrue();
    result.Value.CustomerId.Should().Be(customer.Id);
  }

  [Fact]
  public async Task CreateSale_ShouldFail_WhenCustomerDoesNotExist()
  {
    var scenario = TestScenario.Create();
    scenario.ProductPolicies.Add(Policy(scenario.ProductId, scenario.BusinessId, 125));
    var useCase = scenario.CreateSaleUseCase();

    var result = await useCase.ExecuteAsync(new CreateSaleCommand(
      scenario.BranchId,
      Guid.NewGuid(),
      "Cash",
      [new CreateSaleItemCommand(scenario.ProductId, 1)]));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(SalesErrors.CustomerNotFound);
  }

  [Fact]
  public async Task CreateSale_ShouldFail_WhenBranchDoesNotMatchCurrentUser()
  {
    var scenario = TestScenario.Create();
    scenario.ProductPolicies.Add(Policy(scenario.ProductId, scenario.BusinessId, 125));
    var useCase = scenario.CreateSaleUseCase();

    var result = await useCase.ExecuteAsync(new CreateSaleCommand(
      Guid.NewGuid(),
      null,
      "Cash",
      [new CreateSaleItemCommand(scenario.ProductId, 1)]));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(SalesErrors.InvalidSale);
  }

  [Fact]
  public async Task CreateSale_ShouldFail_WhenItemsAreEmpty()
  {
    var scenario = TestScenario.Create();
    var useCase = scenario.CreateSaleUseCase();

    var result = await useCase.ExecuteAsync(new CreateSaleCommand(
      scenario.BranchId,
      null,
      "Cash",
      []));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(SalesErrors.InvalidSale);
  }

  [Fact]
  public async Task CreateSale_ShouldFail_WhenPaymentMethodIsEmpty()
  {
    var scenario = TestScenario.Create();
    scenario.ProductPolicies.Add(Policy(scenario.ProductId, scenario.BusinessId, 100));
    var useCase = scenario.CreateSaleUseCase();

    var result = await useCase.ExecuteAsync(new CreateSaleCommand(
      scenario.BranchId,
      null,
      "",
      [new CreateSaleItemCommand(scenario.ProductId, 1)]));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(SalesErrors.InvalidSale);
  }

  [Fact]
  public async Task CreateSale_ShouldFail_WhenQuantityIsZero()
  {
    var scenario = TestScenario.Create();
    scenario.ProductPolicies.Add(Policy(scenario.ProductId, scenario.BusinessId, 100));
    var useCase = scenario.CreateSaleUseCase();

    var result = await useCase.ExecuteAsync(new CreateSaleCommand(
      scenario.BranchId,
      null,
      "Cash",
      [new CreateSaleItemCommand(scenario.ProductId, 0)]));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(SalesErrors.InvalidSale);
  }

  [Fact]
  public async Task CreateSale_ShouldFail_WhenProductDoesNotBelongToBusiness()
  {
    var scenario = TestScenario.Create();
    scenario.ProductPolicies.Add(Policy(scenario.ProductId, Guid.NewGuid(), 100));
    var useCase = scenario.CreateSaleUseCase();

    var result = await useCase.ExecuteAsync(new CreateSaleCommand(
      scenario.BranchId,
      null,
      "Cash",
      [new CreateSaleItemCommand(scenario.ProductId, 1)]));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(SalesErrors.ProductNotFound);
  }

  [Fact]
  public async Task CreateSale_ShouldFail_WhenProductCannotBeSold()
  {
    var scenario = TestScenario.Create();
    scenario.ProductPolicies.Add(Policy(scenario.ProductId, scenario.BusinessId, 100, canBeSold: false));
    var useCase = scenario.CreateSaleUseCase();

    var result = await useCase.ExecuteAsync(new CreateSaleCommand(
      scenario.BranchId,
      null,
      "Cash",
      [new CreateSaleItemCommand(scenario.ProductId, 1)]));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(SalesErrors.ProductNotFound);
  }

  [Fact]
  public async Task CreateSale_ShouldCreateOutboxMessage_WhenSaleIsCreated()
  {
    var scenario = TestScenario.Create();
    scenario.ProductPolicies.Add(Policy(scenario.ProductId, scenario.BusinessId, 100));
    var useCase = scenario.CreateSaleUseCase();

    await useCase.ExecuteAsync(new CreateSaleCommand(
      scenario.BranchId,
      null,
      "Cash",
      [new CreateSaleItemCommand(scenario.ProductId, 1)]));

    scenario.Outbox.Events
      .OfType<SaleCreatedEventV1>()
      .Should()
      .ContainSingle(@event =>
        @event.BusinessId == scenario.BusinessId &&
        @event.Items.Single().UnitPrice == 100);
  }

  [Fact]
  public async Task CancelSale_ShouldCancelSale_WhenSaleCanBeCancelled()
  {
    var scenario = TestScenario.Create();
    var sale = scenario.CreateReceivedSale();
    await scenario.Sales.AddAsync(sale);
    var useCase = new CancelSaleUseCase(
      scenario.Sales,
      scenario.CurrentUser,
      scenario.Realtime,
      new NoopAuditLogWriter(),
      scenario.Clock,
      scenario.UnitOfWork);

    var result = await useCase.ExecuteAsync(new CancelSaleCommand(sale.Id, "Cliente cambio de opinion"));

    result.IsSuccess.Should().BeTrue();
    result.Value.Status.Should().Be(SaleStatus.Cancelled.ToString());
    scenario.Realtime.Notifications.Should().ContainSingle();
  }

  [Fact]
  public async Task CancelSale_ShouldFail_WhenSaleNotFound()
  {
    var scenario = TestScenario.Create();
    var useCase = new CancelSaleUseCase(
      scenario.Sales,
      scenario.CurrentUser,
      scenario.Realtime,
      new NoopAuditLogWriter(),
      scenario.Clock,
      scenario.UnitOfWork);

    var result = await useCase.ExecuteAsync(new CancelSaleCommand(Guid.NewGuid(), "not found"));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(SalesErrors.SaleNotFound);
  }

  [Fact]
  public async Task CancelSale_ShouldFail_WhenSaleIsCompleted()
  {
    var scenario = TestScenario.Create();
    var sale = scenario.CreateProcessingSale();
    sale.Complete(Now);
    await scenario.Sales.AddAsync(sale);
    var useCase = new CancelSaleUseCase(
      scenario.Sales,
      scenario.CurrentUser,
      scenario.Realtime,
      new NoopAuditLogWriter(),
      scenario.Clock,
      scenario.UnitOfWork);

    var result = await useCase.ExecuteAsync(new CancelSaleCommand(sale.Id, "late cancel"));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(SalesErrors.InvalidSaleState);
  }

  // ── Credit sale paths ───────────────────────────────────────────────────

  [Fact]
  public async Task CreateSale_CreditSale_ShouldFail_WhenNoCustomerId()
  {
    var scenario = TestScenario.Create();
    scenario.ProductPolicies.Add(Policy(scenario.ProductId, scenario.BusinessId, 100));
    var creditRepo = new InMemoryCustomerCreditRepository();
    var useCase = scenario.CreateCreditSaleUseCase(creditRepo);

    // Credit payment method with no customer
    var result = await useCase.ExecuteAsync(new CreateSaleCommand(
      scenario.BranchId,
      null,
      "Credit",
      [new CreateSaleItemCommand(scenario.ProductId, 1)]));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(SalesErrors.InvalidSale);
  }

  [Fact]
  public async Task CreateSale_CreditSale_ShouldSucceed_WhenCustomerHasUnlimitedCredit()
  {
    var scenario = TestScenario.Create();
    var customer = await scenario.AddCustomerAsync();
    scenario.ProductPolicies.Add(Policy(scenario.ProductId, scenario.BusinessId, 100));
    var creditRepo = new InMemoryCustomerCreditRepository();
    var useCase = scenario.CreateCreditSaleUseCase(creditRepo);

    // Credit limit = 0 means unlimited
    var result = await useCase.ExecuteAsync(new CreateSaleCommand(
      scenario.BranchId,
      customer.Id,
      "Credit",
      [new CreateSaleItemCommand(scenario.ProductId, 1)]));

    result.IsSuccess.Should().BeTrue();
    // A new credit account was auto-created
    creditRepo.Accounts.Should().ContainSingle();
  }

  [Fact]
  public async Task CreateSale_CreditSale_ShouldFail_WhenAccountIsBlocked()
  {
    var scenario = TestScenario.Create();
    var customer = await scenario.AddCustomerAsync();
    scenario.ProductPolicies.Add(Policy(scenario.ProductId, scenario.BusinessId, 100));
    var creditRepo = new InMemoryCustomerCreditRepository();

    // Create a blocked account
    var account = new CustomerCreditAccount(Guid.NewGuid(), new BusinessId(scenario.BusinessId), customer.Id, 1000, Now);
    account.Block(Now);
    creditRepo.Accounts.Add(account);

    var useCase = scenario.CreateCreditSaleUseCase(creditRepo);

    var result = await useCase.ExecuteAsync(new CreateSaleCommand(
      scenario.BranchId,
      customer.Id,
      "Credit",
      [new CreateSaleItemCommand(scenario.ProductId, 1)]));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(CustomerCreditErrors.CreditAccountBlocked);
  }

  [Fact]
  public async Task CreateSale_CreditSale_ShouldFail_WhenCreditLimitExceeded()
  {
    var scenario = TestScenario.Create();
    var customer = await scenario.AddCustomerAsync();
    scenario.ProductPolicies.Add(Policy(scenario.ProductId, scenario.BusinessId, 500));
    var creditRepo = new InMemoryCustomerCreditRepository();

    // Account with only 100 credit limit, sale total = 500
    var account = new CustomerCreditAccount(Guid.NewGuid(), new BusinessId(scenario.BusinessId), customer.Id, 100, Now);
    creditRepo.Accounts.Add(account);

    var useCase = scenario.CreateCreditSaleUseCase(creditRepo);

    var result = await useCase.ExecuteAsync(new CreateSaleCommand(
      scenario.BranchId,
      customer.Id,
      "Credit",
      [new CreateSaleItemCommand(scenario.ProductId, 1)]));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(CustomerCreditErrors.CreditLimitExceeded);
  }

  // ── SaleCreatedEventV1 consumer overload ────────────────────────────────

  [Fact]
  public async Task HandleSaleCreatedEvent_ShouldReturnSuccess_WhenSaleAlreadyExists()
  {
    var scenario = TestScenario.Create();
    var sale = scenario.CreateReceivedSale();
    await scenario.Sales.AddAsync(sale);
    var useCase = scenario.CreateSaleUseCase();
    var saleEvent = new SaleCreatedEventV1(
      Guid.NewGuid(), Guid.NewGuid(), sale.Id,
      scenario.BusinessId, scenario.BranchId, scenario.UserId,
      [new SaleItemV1(scenario.ProductId, 1, 125)],
      125, "Cash", Now);

    var result = await useCase.ExecuteAsync(saleEvent);

    result.IsSuccess.Should().BeTrue();
    // Sale was already there — no duplicate added
    scenario.Sales.CountAll().Should().Be(1);
  }

  [Fact]
  public async Task HandleSaleCreatedEvent_ShouldCreateAndProcessSale_WhenSaleIsNew()
  {
    var scenario = TestScenario.Create();
    var useCase = scenario.CreateSaleUseCase();
    var saleId = Guid.NewGuid();
    var saleEvent = new SaleCreatedEventV1(
      Guid.NewGuid(), Guid.NewGuid(), saleId,
      scenario.BusinessId, scenario.BranchId, scenario.UserId,
      [new SaleItemV1(scenario.ProductId, 1, 125)],
      125, "Cash", Now);

    var result = await useCase.ExecuteAsync(saleEvent);

    result.IsSuccess.Should().BeTrue();
    scenario.Sales.CountAll().Should().Be(1);
  }

  private static ProductSalesPolicy Policy(
    Guid productId,
    Guid businessId,
    decimal salePrice,
    bool canBeSold = true)
    => new(
      productId,
      businessId,
      "Producto",
      $"SKU-{productId:N}",
      null,
      "Simple",
      "Unit",
      salePrice,
      "Itbis18",
      18,
      true,
      true,
      true,
      false,
      true,
      canBeSold,
      canBeSold ? null : "Product is not sellable.");

  private sealed class TestScenario
  {
    private TestScenario()
    {
      CurrentUser = new FakeCurrentUserService(BusinessId, BranchId, UserId);
    }

    public Guid BusinessId { get; } = Guid.NewGuid();

    public Guid BranchId { get; } = Guid.NewGuid();

    public Guid UserId { get; } = Guid.NewGuid();

    public Guid ProductId { get; } = Guid.NewGuid();

    public FakeCurrentUserService CurrentUser { get; }

    public InMemoryCustomerRepository Customers { get; } = new();

    public InMemorySaleRepository Sales { get; } = new();

    public FakeProductSalesPolicyReader ProductPolicies { get; } = new();

    public RecordingOutboxWriter Outbox { get; } = new();

    public RecordingRealtimeNotifier Realtime { get; } = new();

    public FixedClock Clock { get; } = new();

    public ISaleEventWriter SaleEvents => new SaleEventWriter(Outbox, Realtime, Clock);

    public NoopUnitOfWork UnitOfWork { get; } = new();

    public AllowAllSubscriptionLimitChecker SubscriptionLimits { get; } = new();

    public static TestScenario Create() => new();

    public CreateSaleUseCase CreateSaleUseCase()
      => new(
        new SaleHandlerContext(
          Sales,
          Customers,
          ProductPolicies,
          CurrentUser,
          SaleEvents,
          Clock,
          UnitOfWork),
        SubscriptionLimits);

    public CreateSaleUseCase CreateCreditSaleUseCase(ICustomerCreditRepository? creditRepo = null)
      => new(
        new SaleHandlerContext(
          Sales,
          Customers,
          ProductPolicies,
          CurrentUser,
          SaleEvents,
          Clock,
          UnitOfWork),
        SubscriptionLimits,
        creditRepo);

    public async Task<Customer> AddCustomerAsync()
    {
      var customer = new Customer(
        Guid.NewGuid(),
        new BusinessId(BusinessId),
        "Maria Perez",
        "8095550102",
        "maria@example.com",
        Now);

      await Customers.AddAsync(customer);

      return customer;
    }

    public Sale CreateReceivedSale()
      => Sale.Create(
        Guid.NewGuid(),
        new BusinessId(BusinessId),
        new BranchId(BranchId),
        UserId,
        [new SaleLine(ProductId, 1, 100)],
        "Cash",
        Now);

    public Sale CreateProcessingSale()
    {
      var sale = CreateReceivedSale();
      sale.MarkAsProcessing(Now);
      return sale;
    }
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

  private sealed class NoopAuditLogWriter : IAuditLogWriter
  {
    public Task WriteAsync(AuditEntry entry, CancellationToken cancellationToken = default)
      => Task.CompletedTask;
  }

  private sealed class RecordingOutboxWriter : IOutboxWriter
  {
    public List<IIntegrationEvent> Events { get; } = [];

    public Task AddAsync<TEvent>(
      TEvent integrationEvent,
      CancellationToken cancellationToken = default)
      where TEvent : class, IIntegrationEvent
    {
      Events.Add(integrationEvent);
      return Task.CompletedTask;
    }
  }

  private sealed class AllowAllSubscriptionLimitChecker : ISubscriptionLimitChecker
  {
    private static readonly SubscriptionLimitCheckResult Allowed = new(true, "OK", "Allowed.", 0, 1000);

    public Task<SubscriptionLimitCheckResult> CanCreateBranchAsync(
      BusinessId businessId,
      CancellationToken cancellationToken = default)
      => Task.FromResult(Allowed);

    public Task<SubscriptionLimitCheckResult> CanCreateUserAsync(
      BusinessId businessId,
      CancellationToken cancellationToken = default)
      => Task.FromResult(Allowed);

    public Task<SubscriptionLimitCheckResult> CanCreateProductAsync(
      BusinessId businessId,
      CancellationToken cancellationToken = default)
      => Task.FromResult(Allowed);

    public Task<SubscriptionLimitCheckResult> CanCreateSaleAsync(
      BusinessId businessId,
      CancellationToken cancellationToken = default)
      => Task.FromResult(Allowed);

    public Task<SubscriptionLimitCheckResult> CanUseInventoryTransfersAsync(
      BusinessId businessId,
      CancellationToken cancellationToken = default)
      => Task.FromResult(Allowed);

    public Task<SubscriptionLimitCheckResult> CanUseAdvancedReportsAsync(
      BusinessId businessId,
      CancellationToken cancellationToken = default)
      => Task.FromResult(Allowed);

    public Task<SubscriptionLimitCheckResult> CanUseAuditLogsAsync(
      BusinessId businessId,
      CancellationToken cancellationToken = default)
      => Task.FromResult(Allowed);
  }

  private sealed class RecordingRealtimeNotifier : IRealtimeNotifier
  {
    public List<object> Notifications { get; } = [];

    public Task NotifyBusinessAsync(
      Guid businessId,
      string eventName,
      object payload,
      CancellationToken cancellationToken = default)
    {
      Notifications.Add(payload);
      return Task.CompletedTask;
    }

    public Task NotifyBranchAsync(
      Guid branchId,
      string eventName,
      object payload,
      CancellationToken cancellationToken = default)
      => Task.CompletedTask;

    public Task NotifyUserAsync(
      Guid userId,
      string eventName,
      object payload,
      CancellationToken cancellationToken = default)
      => Task.CompletedTask;
  }

  private sealed class FakeProductSalesPolicyReader : IProductSalesPolicyReader
  {
    private readonly Dictionary<Guid, ProductSalesPolicy> policies = [];

    public void Add(ProductSalesPolicy policy) => policies[policy.ProductId] = policy;

    public Task<ProductSalesPolicy?> GetSalesPolicyAsync(
      Guid businessId,
      Guid productId,
      CancellationToken cancellationToken = default)
      => Task.FromResult(
        policies.TryGetValue(productId, out var policy) && policy.BusinessId == businessId
          ? policy
          : null);
  }

  private sealed class InMemoryCustomerRepository : ICustomerRepository
  {
    private readonly Dictionary<(Guid BusinessId, Guid CustomerId), Customer> customers = [];

    public Task<Customer?> GetAsync(
      BusinessId businessId,
      Guid customerId,
      CancellationToken cancellationToken = default)
      => Task.FromResult(
        customers.TryGetValue((businessId.Value, customerId), out var customer)
          ? customer
          : null);

    public Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
    {
      customers[(customer.BusinessId.Value, customer.Id)] = customer;
      return Task.CompletedTask;
    }

    public Task<int> CountAsync(
      BusinessId businessId,
      CustomerSearchCriteria criteria,
      CancellationToken cancellationToken = default)
      => Task.FromResult(customers.Values.Count(customer => customer.BusinessId == businessId));

    public Task<IReadOnlyCollection<Customer>> ListAsync(
      BusinessId businessId,
      CustomerSearchCriteria criteria,
      CancellationToken cancellationToken = default)
      => Task.FromResult<IReadOnlyCollection<Customer>>(
        customers.Values.Where(customer => customer.BusinessId == businessId).ToArray());

    public Task<IReadOnlyCollection<Customer>> ExportAllAsync(
      BusinessId businessId,
      CancellationToken cancellationToken = default)
      => Task.FromResult<IReadOnlyCollection<Customer>>(
        customers.Values.Where(customer => customer.BusinessId == businessId).ToArray());
  }

  private sealed class InMemorySaleRepository : ISaleRepository
  {
    private readonly Dictionary<(Guid BusinessId, Guid SaleId), Sale> sales = [];

    public int CountAll() => sales.Count;

    public Task<Sale?> GetAsync(
      BusinessId businessId,
      Guid saleId,
      CancellationToken cancellationToken = default)
      => Task.FromResult(
        sales.TryGetValue((businessId.Value, saleId), out var sale)
          ? sale
          : null);

    public Task AddAsync(Sale sale, CancellationToken cancellationToken = default)
    {
      sales[(sale.BusinessId.Value, sale.Id)] = sale;
      return Task.CompletedTask;
    }

    public Task<int> CountAsync(
      BusinessId businessId,
      SaleSearchCriteria criteria,
      CancellationToken cancellationToken = default)
      => Task.FromResult(sales.Values.Count(sale => sale.BusinessId == businessId));

    public Task<IReadOnlyCollection<Sale>> ListAsync(
      BusinessId businessId,
      SaleSearchCriteria criteria,
      CancellationToken cancellationToken = default)
      => Task.FromResult<IReadOnlyCollection<Sale>>(
        sales.Values.Where(sale => sale.BusinessId == businessId).ToArray());
  }

  private sealed class InMemoryCustomerCreditRepository : ICustomerCreditRepository
  {
    public List<CustomerCreditAccount> Accounts { get; } = [];

    public Task<CustomerCreditAccount?> GetAccountAsync(
      BusinessId businessId,
      Guid customerId,
      CancellationToken cancellationToken = default)
      => Task.FromResult(
        Accounts.FirstOrDefault(a => a.BusinessId == businessId && a.CustomerId == customerId));

    public Task<IReadOnlyDictionary<Guid, CustomerCreditAccount>> ListAccountsAsync(
      BusinessId businessId,
      IReadOnlyCollection<Guid> customerIds,
      CancellationToken cancellationToken = default)
      => Task.FromResult<IReadOnlyDictionary<Guid, CustomerCreditAccount>>(
        Accounts
          .Where(a => a.BusinessId == businessId && customerIds.Contains(a.CustomerId))
          .ToDictionary(a => a.CustomerId));

    public Task AddAccountAsync(
      CustomerCreditAccount account,
      CancellationToken cancellationToken = default)
    {
      Accounts.Add(account);
      return Task.CompletedTask;
    }

    public Task AddMovementAsync(
      CustomerCreditMovement movement,
      CancellationToken cancellationToken = default)
      => Task.CompletedTask;

    public Task AddPaymentAsync(
      CustomerPayment payment,
      CancellationToken cancellationToken = default)
      => Task.CompletedTask;

    public Task<bool> HasDebitForSaleAsync(
      BusinessId businessId,
      Guid saleId,
      CancellationToken cancellationToken = default)
      => Task.FromResult(false);

    public Task<bool> HasPaymentAsync(
      BusinessId businessId,
      Guid paymentId,
      CancellationToken cancellationToken = default)
      => Task.FromResult(false);

    public Task<int> CountMovementsAsync(
      BusinessId businessId,
      Guid customerId,
      CancellationToken cancellationToken = default)
      => Task.FromResult(0);

    public Task<IReadOnlyCollection<CustomerCreditMovement>> ListMovementsAsync(
      BusinessId businessId,
      Guid customerId,
      int page,
      int pageSize,
      CancellationToken cancellationToken = default)
      => Task.FromResult<IReadOnlyCollection<CustomerCreditMovement>>([]);

    public Task<IReadOnlyCollection<CustomerCreditAccount>> ExportAllAccountsAsync(
      BusinessId businessId,
      CancellationToken cancellationToken = default)
      => Task.FromResult<IReadOnlyCollection<CustomerCreditAccount>>(
        Accounts.Where(a => a.BusinessId == businessId).ToArray());
  }
}

#pragma warning restore CA1707
