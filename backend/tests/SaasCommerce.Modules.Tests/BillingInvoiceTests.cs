using FluentAssertions;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Observability;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Contracts.Events;
using SaasCommerce.Modules.Billing.Application.Abstractions;
using SaasCommerce.Modules.Billing.Application.Invoices;
using SaasCommerce.Modules.Billing.Contracts.Events.V1;
using SaasCommerce.Modules.Billing.Domain;
using SaasCommerce.SharedKernel.Tenancy;

#pragma warning disable CA1707

namespace SaasCommerce.Modules.Tests;

public sealed class BillingInvoiceTests
{
  private static readonly DateTimeOffset Now = new(2026, 5, 18, 10, 0, 0, TimeSpan.Zero);

  [Fact]
  public async Task GenerateInvoice_ShouldCreateInvoice_WhenSaleIsCompleted()
  {
    var scenario = Scenario.Create();
    var handler = scenario.CreateGenerateHandler();

    var result = await handler.Handle(new GenerateInvoiceCommand(scenario.SaleId));

    result.IsSuccess.Should().BeTrue();
    result.Value.InvoiceNumber.Should().Be("RI-00000001");
    result.Value.Status.Should().Be(InvoiceStatus.Issued.ToString());
    scenario.Invoices.Items.Should().ContainSingle();
    scenario.Outbox.Events.Should().ContainSingle(@event => @event is InvoiceGeneratedEventV1);
    scenario.UnitOfWork.SaveCount.Should().Be(1);
  }

  [Fact]
  public async Task GenerateInvoice_ShouldNotDuplicateInvoice_WhenAlreadyExists()
  {
    var scenario = Scenario.Create();
    var existing = scenario.CreateInvoice();
    scenario.Invoices.Items.Add(existing);
    var handler = scenario.CreateGenerateHandler();

    var result = await handler.Handle(new GenerateInvoiceCommand(scenario.SaleId));

    result.IsSuccess.Should().BeTrue();
    result.Value.InvoiceId.Should().Be(existing.Id);
    scenario.Invoices.Items.Should().ContainSingle();
    scenario.Outbox.Events.Should().BeEmpty();
  }

  [Fact]
  public async Task GenerateInvoice_ShouldFail_WhenSaleIsCancelled()
  {
    var scenario = Scenario.Create("Cancelled");
    var handler = scenario.CreateGenerateHandler();

    var result = await handler.Handle(new GenerateInvoiceCommand(scenario.SaleId));

    result.IsFailure.Should().BeTrue();
    result.Error.Code.Should().Be("invoices.invalid_state");
    scenario.Invoices.Items.Should().BeEmpty();
  }

  [Fact]
  public async Task GetInvoices_ShouldFilterByBusinessId()
  {
    var scenario = Scenario.Create();
    scenario.Invoices.Items.Add(scenario.CreateInvoice());
    scenario.Invoices.Items.Add(Invoice.Issue(
      Guid.NewGuid(),
      Guid.NewGuid(),
      1,
      new InvoiceContext(new BusinessId(Guid.NewGuid()), new BranchId(Guid.NewGuid()), null),
      new InvoiceFinancials(90, 0, 0, 90),
      Now));
    var handler = new GetInvoicesHandler(scenario.Invoices, scenario.CurrentUser);

    var result = await handler.Handle(new GetInvoicesQuery(null, null, null, null, 1, 50));

    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().ContainSingle();
    result.Value.Items.Should().OnlyContain(invoice => invoice.BusinessId == scenario.BusinessId);
  }

  [Fact]
  public async Task CancelInvoice_ShouldChangeStatus_WhenInvoiceIsIssued()
  {
    var scenario = Scenario.Create();
    var invoice = scenario.CreateInvoice();
    scenario.Invoices.Items.Add(invoice);
    var handler = new CancelInvoiceHandler(
      scenario.Invoices,
      scenario.CurrentUser,
      scenario.Outbox,
      new TestCorrelationIdProvider(scenario.CorrelationId),
      scenario.Clock,
      scenario.UnitOfWork);

    var result = await handler.Handle(new CancelInvoiceCommand(invoice.Id));

    result.IsSuccess.Should().BeTrue();
    result.Value.Status.Should().Be(InvoiceStatus.Cancelled.ToString());
    scenario.Outbox.Events.Should().ContainSingle(@event => @event is InvoiceCancelledEventV1);
  }

  [Fact]
  public async Task GenerateInvoice_ShouldFail_WhenSaleIdEmpty()
  {
    var scenario = Scenario.Create();
    var handler = scenario.CreateGenerateHandler();

    var result = await handler.Handle(new GenerateInvoiceCommand(Guid.Empty));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(InvoiceErrors.InvalidInvoice);
  }

  [Fact]
  public async Task GenerateInvoice_ShouldFail_WhenSaleNotFound()
  {
    var scenario = Scenario.Create();
    var handler = scenario.CreateGenerateHandler();

    var result = await handler.Handle(new GenerateInvoiceCommand(Guid.NewGuid()));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(InvoiceErrors.SaleNotFound);
  }

  [Fact]
  public async Task GenerateInvoice_ShouldPublishFailureEvents_WhenPublishFailureEventTrue()
  {
    var scenario = Scenario.Create();
    var handler = scenario.CreateGenerateHandler();

    var result = await handler.Handle(new GenerateInvoiceCommand(
      Guid.NewGuid(),
      CorrelationId: scenario.CorrelationId,
      BusinessId: scenario.BusinessId,
      UserId: scenario.UserId,
      PaymentId: Guid.NewGuid(),
      PublishFailureEvent: true));

    result.IsFailure.Should().BeTrue();
    scenario.Outbox.Events.OfType<InvoiceGenerationFailedEventV1>().Should().ContainSingle();
    scenario.Outbox.Events.OfType<InvoiceFailedEventV1>().Should().ContainSingle();
  }

  // ── GetInvoiceByIdHandler ────────────────────────────────────────────────

  [Fact]
  public async Task GetInvoiceById_ShouldReturnInvoice_WhenExists()
  {
    var scenario = Scenario.Create();
    var invoice = scenario.CreateInvoice();
    scenario.Invoices.Items.Add(invoice);
    var handler = new GetInvoiceByIdHandler(scenario.Invoices, scenario.CurrentUser);

    var result = await handler.Handle(new GetInvoiceByIdQuery(invoice.Id));

    result.IsSuccess.Should().BeTrue();
    result.Value.InvoiceId.Should().Be(invoice.Id);
  }

  [Fact]
  public async Task GetInvoiceById_ShouldFail_WhenNotFound()
  {
    var scenario = Scenario.Create();
    var handler = new GetInvoiceByIdHandler(scenario.Invoices, scenario.CurrentUser);

    var result = await handler.Handle(new GetInvoiceByIdQuery(Guid.NewGuid()));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(InvoiceErrors.InvoiceNotFound);
  }

  [Fact]
  public async Task GetInvoiceById_ShouldFail_WhenIdIsEmpty()
  {
    var scenario = Scenario.Create();
    var handler = new GetInvoiceByIdHandler(scenario.Invoices, scenario.CurrentUser);

    var result = await handler.Handle(new GetInvoiceByIdQuery(Guid.Empty));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(InvoiceErrors.InvalidInvoice);
  }

  // ── GetInvoiceBySaleHandler ──────────────────────────────────────────────

  [Fact]
  public async Task GetInvoiceBySale_ShouldReturnInvoice_WhenExists()
  {
    var scenario = Scenario.Create();
    var invoice = scenario.CreateInvoice();
    scenario.Invoices.Items.Add(invoice);
    var handler = new GetInvoiceBySaleHandler(scenario.Invoices, scenario.CurrentUser);

    var result = await handler.Handle(new GetInvoiceBySaleQuery(scenario.SaleId));

    result.IsSuccess.Should().BeTrue();
    result.Value.SaleId.Should().Be(scenario.SaleId);
  }

  [Fact]
  public async Task GetInvoiceBySale_ShouldFail_WhenNotFound()
  {
    var scenario = Scenario.Create();
    var handler = new GetInvoiceBySaleHandler(scenario.Invoices, scenario.CurrentUser);

    var result = await handler.Handle(new GetInvoiceBySaleQuery(Guid.NewGuid()));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(InvoiceErrors.InvoiceNotFound);
  }

  [Fact]
  public async Task GetInvoiceBySale_ShouldFail_WhenSaleIdIsEmpty()
  {
    var scenario = Scenario.Create();
    var handler = new GetInvoiceBySaleHandler(scenario.Invoices, scenario.CurrentUser);

    var result = await handler.Handle(new GetInvoiceBySaleQuery(Guid.Empty));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(InvoiceErrors.InvalidInvoice);
  }

  // ── CancelInvoice edge cases ─────────────────────────────────────────────

  [Fact]
  public async Task CancelInvoice_ShouldFail_WhenInvoiceNotFound()
  {
    var scenario = Scenario.Create();
    var handler = new CancelInvoiceHandler(
      scenario.Invoices,
      scenario.CurrentUser,
      scenario.Outbox,
      new TestCorrelationIdProvider(scenario.CorrelationId),
      scenario.Clock,
      scenario.UnitOfWork);

    var result = await handler.Handle(new CancelInvoiceCommand(Guid.NewGuid()));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(InvoiceErrors.InvoiceNotFound);
  }

  [Fact]
  public async Task GetInvoices_ShouldReturnPaginatedResults()
  {
    var scenario = Scenario.Create();
    scenario.Invoices.Items.Add(scenario.CreateInvoice());
    var handler = new GetInvoicesHandler(scenario.Invoices, scenario.CurrentUser);

    var result = await handler.Handle(new GetInvoicesQuery(null, null, null, null, 1, 10));

    result.IsSuccess.Should().BeTrue();
    result.Value.TotalItems.Should().Be(1);
    result.Value.Items.Should().ContainSingle();
  }

  [Fact]
  public void InvoiceGeneratedEventV1_ShouldExposeRequiredContractFields()
  {
    var @event = new InvoiceGeneratedEventV1(
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      125,
      Now);

    @event.EventId.Should().NotBeEmpty();
    @event.CorrelationId.Should().NotBeEmpty();
    @event.BusinessId.Should().NotBeEmpty();
    @event.SaleId.Should().NotBeEmpty();
    @event.InvoiceId.Should().NotBeEmpty();
    @event.OccurredAt.Should().Be(@event.CreatedAt);
  }

  private sealed class Scenario
  {
    private Scenario(string saleStatus)
    {
      Sale = new InvoiceSaleSnapshot(SaleId, BusinessId, BranchId, CustomerId, saleStatus, 250);
      Sales = new TestInvoiceSaleReader(Sale);
      CurrentUser = new TestCurrentUser(BusinessId, BranchId, UserId);
    }

    public Guid BusinessId { get; } = Guid.NewGuid();

    public Guid BranchId { get; } = Guid.NewGuid();

    public Guid UserId { get; } = Guid.NewGuid();

    public Guid SaleId { get; } = Guid.NewGuid();

    public Guid CustomerId { get; } = Guid.NewGuid();

    public Guid CorrelationId { get; } = Guid.NewGuid();

    public InvoiceSaleSnapshot Sale { get; }

    public TestInvoiceRepository Invoices { get; } = new();

    public TestOutboxWriter Outbox { get; } = new();

    public TestClock Clock { get; } = new();

    public TestUnitOfWork UnitOfWork { get; } = new();

    public TestCurrentUser CurrentUser { get; }

    public TestInvoiceSaleReader Sales { get; }

    public static Scenario Create(string saleStatus = "Completed") => new(saleStatus);

    public GenerateInvoiceHandler CreateGenerateHandler()
      => new(Invoices, Sales, Outbox, CurrentUser, Clock, UnitOfWork);

    public Invoice CreateInvoice()
      => Invoice.Issue(
        Guid.NewGuid(),
        SaleId,
        1,
        new InvoiceContext(new BusinessId(BusinessId), new BranchId(BranchId), CustomerId),
        new InvoiceFinancials(250, 0, 0, 250),
        Now);
  }

  private sealed class TestInvoiceRepository : IInvoiceRepository
  {
    public List<Invoice> Items { get; } = [];

    public Task AddAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
      Items.Add(invoice);
      return Task.CompletedTask;
    }

    public Task<int> CountAsync(
      BusinessId businessId,
      InvoiceSearchCriteria criteria,
      CancellationToken cancellationToken = default)
      => Task.FromResult(Filter(businessId, criteria).Count);

    public Task<Invoice?> GetAsync(
      BusinessId businessId,
      Guid invoiceId,
      CancellationToken cancellationToken = default)
      => Task.FromResult(Items.SingleOrDefault(invoice => invoice.BusinessId == businessId && invoice.Id == invoiceId));

    public Task<Invoice?> GetBySaleAsync(
      BusinessId businessId,
      Guid saleId,
      CancellationToken cancellationToken = default)
      => Task.FromResult(Items.SingleOrDefault(invoice => invoice.BusinessId == businessId && invoice.SaleId == saleId));

    public Task<int> GetNextSequenceAsync(BusinessId businessId, CancellationToken cancellationToken = default)
      => Task.FromResult(
        Items
          .Where(invoice => invoice.BusinessId == businessId)
          .Select(invoice => invoice.Sequence)
          .DefaultIfEmpty()
          .Max() + 1);

    public Task<IReadOnlyCollection<Invoice>> ListAsync(
      BusinessId businessId,
      InvoiceSearchCriteria criteria,
      CancellationToken cancellationToken = default)
      => Task.FromResult<IReadOnlyCollection<Invoice>>(Filter(businessId, criteria));

    private List<Invoice> Filter(BusinessId businessId, InvoiceSearchCriteria criteria)
      => Items
        .Where(invoice => invoice.BusinessId == businessId)
        .Where(invoice => string.IsNullOrWhiteSpace(criteria.Status) || invoice.Status.ToString() == criteria.Status)
        .OrderByDescending(invoice => invoice.CreatedAt)
        .Skip((criteria.Page - 1) * criteria.PageSize)
        .Take(criteria.PageSize)
        .ToList();
  }

  private sealed class TestInvoiceSaleReader(InvoiceSaleSnapshot sale) : IInvoiceSaleReader
  {
    public Task<InvoiceSaleSnapshot?> GetAsync(
      Guid businessId,
      Guid saleId,
      CancellationToken cancellationToken = default)
      => Task.FromResult(
        sale.BusinessId == businessId && sale.SaleId == saleId
          ? sale
          : null);
  }

  private sealed class TestOutboxWriter : IOutboxWriter
  {
    public List<IIntegrationEvent> Events { get; } = [];

    public Task AddAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
      where TEvent : class, IIntegrationEvent
    {
      Events.Add(integrationEvent);
      return Task.CompletedTask;
    }
  }

  private sealed class TestCurrentUser(Guid businessId, Guid branchId, Guid userId) : ICurrentUserService
  {
    public Guid? UserId => userId;

    public Guid? BusinessId => businessId;

    public Guid? BranchId => branchId;

    public IReadOnlyCollection<string> Roles => ["Admin"];

    public bool IsAuthenticated => true;
  }

  private sealed class TestClock : IClock
  {
    public DateTimeOffset UtcNow => Now;
  }

  private sealed class TestUnitOfWork : IUnitOfWork
  {
    public int SaveCount { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
      SaveCount++;
      return Task.FromResult(1);
    }
  }

  private sealed class TestCorrelationIdProvider(Guid correlationId) : ICorrelationIdProvider
  {
    public string CorrelationId => correlationId.ToString("D");
  }
}

#pragma warning restore CA1707
