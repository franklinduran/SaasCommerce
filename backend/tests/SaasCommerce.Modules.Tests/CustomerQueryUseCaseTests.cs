#pragma warning disable CA1707

using FluentAssertions;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Customers.Application.Abstractions;
using SaasCommerce.Modules.Customers.Application.Credits;
using SaasCommerce.Modules.Customers.Application.Customers;
using SaasCommerce.Modules.Customers.Domain;
using SaasCommerce.Modules.Customers.Domain.Credits;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Tests;

public sealed class CustomerQueryUseCaseTests
{
  private static readonly DateTimeOffset Now = new(2026, 5, 19, 10, 0, 0, TimeSpan.Zero);
  private readonly Guid businessId = Guid.NewGuid();
  private readonly Guid customerId = Guid.NewGuid();

  // ── GetCustomerCreditMovementsUseCase ────────────────────────────────────

  [Fact]
  public async Task GetCreditMovements_ShouldFail_WhenPageInvalid()
  {
    var uc = new GetCustomerCreditMovementsUseCase(
      new StubCustomerRepo(), new StubCreditRepo(), Authed(businessId));

    var result = await uc.ExecuteAsync(new GetCustomerCreditMovementsQuery(customerId, 0, 10));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(CustomerCreditErrors.InvalidCreditOperation);
  }

  [Fact]
  public async Task GetCreditMovements_ShouldFail_WhenPageSizeInvalid()
  {
    var uc = new GetCustomerCreditMovementsUseCase(
      new StubCustomerRepo(), new StubCreditRepo(), Authed(businessId));

    var result = await uc.ExecuteAsync(new GetCustomerCreditMovementsQuery(customerId, 1, 7));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(CustomerCreditErrors.InvalidCreditOperation);
  }

  [Fact]
  public async Task GetCreditMovements_ShouldFail_WhenNoBusinessContext()
  {
    var uc = new GetCustomerCreditMovementsUseCase(
      new StubCustomerRepo(), new StubCreditRepo(), Anonymous());

    var result = await uc.ExecuteAsync(new GetCustomerCreditMovementsQuery(customerId, 1, 10));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(CustomerCreditErrors.UserContextRequired);
  }

  [Fact]
  public async Task GetCreditMovements_ShouldFail_WhenCustomerNotFound()
  {
    var uc = new GetCustomerCreditMovementsUseCase(
      new StubCustomerRepo(), new StubCreditRepo(), Authed(businessId));

    var result = await uc.ExecuteAsync(new GetCustomerCreditMovementsQuery(customerId, 1, 10));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(CustomerCreditErrors.CustomerNotFound);
  }

  [Fact]
  public async Task GetCreditMovements_ShouldReturnMovements_WhenValid()
  {
    var customerRepo = new StubCustomerRepo();
    customerRepo.Add(MakeCustomer());
    var creditRepo = new StubCreditRepo();
    var account = new CustomerCreditAccount(Guid.NewGuid(), new BusinessId(businessId), customerId, 1000, Now);
    creditRepo.Movements.Add(account.ApplyDebit(Guid.NewGuid(), Guid.NewGuid(), 150, "venta", null, Now));
    creditRepo.Movements.Add(account.ApplyDebit(Guid.NewGuid(), Guid.NewGuid(), 50, null, null, Now));
    var uc = new GetCustomerCreditMovementsUseCase(customerRepo, creditRepo, Authed(businessId));

    var result = await uc.ExecuteAsync(new GetCustomerCreditMovementsQuery(customerId, 1, 10));

    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(2);
    result.Value.TotalItems.Should().Be(2);
  }

  // ── GetCustomerCreditSummaryUseCase ──────────────────────────────────────

  [Fact]
  public async Task GetCreditSummary_ShouldFail_WhenNoBusinessContext()
  {
    var uc = new GetCustomerCreditSummaryUseCase(
      new StubCustomerRepo(), new StubCreditRepo(), Anonymous());

    var result = await uc.ExecuteAsync(new GetCustomerCreditSummaryQuery(customerId));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(CustomerCreditErrors.UserContextRequired);
  }

  [Fact]
  public async Task GetCreditSummary_ShouldFail_WhenCustomerNotFound()
  {
    var uc = new GetCustomerCreditSummaryUseCase(
      new StubCustomerRepo(), new StubCreditRepo(), Authed(businessId));

    var result = await uc.ExecuteAsync(new GetCustomerCreditSummaryQuery(customerId));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(CustomerCreditErrors.CustomerNotFound);
  }

  [Fact]
  public async Task GetCreditSummary_ShouldReturnSummary_WhenAccountExists()
  {
    var customerRepo = new StubCustomerRepo();
    customerRepo.Add(MakeCustomer());
    var creditRepo = new StubCreditRepo();
    var account = new CustomerCreditAccount(Guid.NewGuid(), new BusinessId(businessId), customerId, 5000, Now);
    account.ApplyDebit(Guid.NewGuid(), Guid.NewGuid(), 1200, null, null, Now);
    await creditRepo.AddAccountAsync(account);
    var uc = new GetCustomerCreditSummaryUseCase(customerRepo, creditRepo, Authed(businessId));

    var result = await uc.ExecuteAsync(new GetCustomerCreditSummaryQuery(customerId));

    result.IsSuccess.Should().BeTrue();
    result.Value.CurrentBalance.Should().Be(1200);
    result.Value.CreditLimit.Should().Be(5000);
  }

  [Fact]
  public async Task GetCreditSummary_ShouldReturnDefaultAccount_WhenNoAccountExists()
  {
    var customerRepo = new StubCustomerRepo();
    customerRepo.Add(MakeCustomer());
    var uc = new GetCustomerCreditSummaryUseCase(
      customerRepo, new StubCreditRepo(), Authed(businessId));

    var result = await uc.ExecuteAsync(new GetCustomerCreditSummaryQuery(customerId));

    result.IsSuccess.Should().BeTrue();
    result.Value.CurrentBalance.Should().Be(0);
    result.Value.CreditLimit.Should().Be(0);
  }

  // ── ListCustomersUseCase ─────────────────────────────────────────────────

  [Fact]
  public async Task ListCustomers_ShouldFail_WhenNoBusinessContext()
  {
    var uc = new ListCustomersUseCase(new StubCustomerRepo(), Anonymous());

    var result = await uc.ExecuteAsync(new ListCustomersQuery(null, null, 1, 10, null, null));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(CustomerErrors.UserContextRequired);
  }

  [Fact]
  public async Task ListCustomers_ShouldFail_WhenPageInvalid()
  {
    var uc = new ListCustomersUseCase(new StubCustomerRepo(), Authed(businessId));

    var result = await uc.ExecuteAsync(new ListCustomersQuery(null, null, 0, 10, null, null));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(CustomerErrors.InvalidCustomer);
  }

  [Fact]
  public async Task ListCustomers_ShouldFail_WhenSortInvalid()
  {
    var uc = new ListCustomersUseCase(new StubCustomerRepo(), Authed(businessId));

    var result = await uc.ExecuteAsync(
      new ListCustomersQuery(null, null, 1, 10, "NotAColumn", null));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(CustomerErrors.InvalidCustomer);
  }

  [Fact]
  public async Task ListCustomers_ShouldReturnCustomers_WithoutCreditRepo()
  {
    var customerRepo = new StubCustomerRepo();
    customerRepo.Add(MakeCustomer());
    var uc = new ListCustomersUseCase(customerRepo, Authed(businessId));

    var result = await uc.ExecuteAsync(
      new ListCustomersQuery("Cliente", true, 1, 10, "FullName", "Asc"));

    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().ContainSingle();
    result.Value.TotalItems.Should().Be(1);
  }

  [Fact]
  public async Task ListCustomers_ShouldIncludeCreditAccounts_WhenCreditRepoProvided()
  {
    var customerRepo = new StubCustomerRepo();
    customerRepo.Add(MakeCustomer());
    var creditRepo = new StubCreditRepo();
    await creditRepo.AddAccountAsync(
      new CustomerCreditAccount(Guid.NewGuid(), new BusinessId(businessId), customerId, 2000, Now));
    var uc = new ListCustomersUseCase(customerRepo, Authed(businessId), creditRepo);

    var result = await uc.ExecuteAsync(
      new ListCustomersQuery(null, null, 1, 25, null, "Desc"));

    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().ContainSingle();
  }

  // ── Helpers / Stubs ──────────────────────────────────────────────────────

  private Customer MakeCustomer()
    => new(customerId, new BusinessId(businessId), "Cliente Test", "8095550000", "c@test.com", Now);

  private static FakeUser Authed(Guid bid)
    => new() { BusinessId = bid, UserId = Guid.NewGuid(), BranchId = Guid.NewGuid(), IsAuthenticated = true };

  private static FakeUser Anonymous()
    => new() { BusinessId = null, UserId = null, BranchId = null, IsAuthenticated = false };

  private sealed class FakeUser : ICurrentUserService
  {
    public Guid? UserId { get; init; }
    public Guid? BusinessId { get; init; }
    public Guid? BranchId { get; init; }
    public IReadOnlyCollection<string> Roles { get; init; } = [];
    public bool IsAuthenticated { get; init; }
  }

  private sealed class StubCustomerRepo : ICustomerRepository
  {
    private readonly List<Customer> items = [];

    public void Add(Customer c) => items.Add(c);

    public Task<Customer?> GetAsync(BusinessId businessId, Guid customerId, CancellationToken cancellationToken = default)
      => Task.FromResult(items.FirstOrDefault(c => c.BusinessId == businessId && c.Id == customerId));

    public Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
    {
      items.Add(customer);
      return Task.CompletedTask;
    }

    public Task<int> CountAsync(BusinessId businessId, CustomerSearchCriteria criteria, CancellationToken cancellationToken = default)
      => Task.FromResult(items.Count(c => c.BusinessId == businessId));

    public Task<IReadOnlyCollection<Customer>> ListAsync(BusinessId businessId, CustomerSearchCriteria criteria, CancellationToken cancellationToken = default)
      => Task.FromResult<IReadOnlyCollection<Customer>>(
        items.Where(c => c.BusinessId == businessId).ToArray());
  }

  private sealed class StubCreditRepo : ICustomerCreditRepository
  {
    private readonly Dictionary<(Guid, Guid), CustomerCreditAccount> accounts = [];

    public List<CustomerCreditMovement> Movements { get; } = [];

    public Task<CustomerCreditAccount?> GetAccountAsync(BusinessId businessId, Guid customerId, CancellationToken cancellationToken = default)
      => Task.FromResult(accounts.TryGetValue((businessId.Value, customerId), out var a) ? a : null);

    public Task<IReadOnlyDictionary<Guid, CustomerCreditAccount>> ListAccountsAsync(BusinessId businessId, IReadOnlyCollection<Guid> customerIds, CancellationToken cancellationToken = default)
      => Task.FromResult<IReadOnlyDictionary<Guid, CustomerCreditAccount>>(
        accounts.Values
          .Where(a => a.BusinessId == businessId && customerIds.Contains(a.CustomerId))
          .ToDictionary(a => a.CustomerId));

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
      => Task.CompletedTask;

    public Task<bool> HasDebitForSaleAsync(BusinessId businessId, Guid saleId, CancellationToken cancellationToken = default)
      => Task.FromResult(false);

    public Task<bool> HasPaymentAsync(BusinessId businessId, Guid paymentId, CancellationToken cancellationToken = default)
      => Task.FromResult(false);

    public Task<int> CountMovementsAsync(BusinessId businessId, Guid customerId, CancellationToken cancellationToken = default)
      => Task.FromResult(Movements.Count(m => m.BusinessId == businessId && m.CustomerId == customerId));

    public Task<IReadOnlyCollection<CustomerCreditMovement>> ListMovementsAsync(BusinessId businessId, Guid customerId, int page, int pageSize, CancellationToken cancellationToken = default)
      => Task.FromResult<IReadOnlyCollection<CustomerCreditMovement>>(
        Movements
          .Where(m => m.BusinessId == businessId && m.CustomerId == customerId)
          .Skip((page - 1) * pageSize)
          .Take(pageSize)
          .ToArray());
  }
}

#pragma warning restore CA1707
