#pragma warning disable CA1707

using FluentAssertions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Observability;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Contracts.Events;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Catalog.Domain;
using SaasCommerce.Modules.Customers.Application.Abstractions;
using SaasCommerce.Modules.Customers.Domain.Credits;
using SaasCommerce.Modules.Inventory.Application.Abstractions;
using SaasCommerce.Modules.Inventory.Domain;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Application.Sales;
using SaasCommerce.Modules.Sales.Contracts.Events.V1;
using SaasCommerce.Modules.Sales.Contracts.Requests;
using SaasCommerce.Modules.Sales.Contracts.Responses;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.Modules.Sales.Infrastructure.Persistence;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Tests;

public sealed class SaleReturnWorkflowTests
{
  private static readonly DateTimeOffset Now = new(2026, 5, 26, 10, 0, 0, TimeSpan.Zero);

  [Fact]
  public void SaleReturn_ShouldRejectSalesThatAreNotCompleted()
  {
    var sale = CreateSale();

    var act = () => SaleReturn.Request(
      Guid.NewGuid(),
      sale,
      Guid.NewGuid(),
      "Cambio solicitado",
      [new SaleReturnLine(sale.Items.Single().Id, 1)],
      Now);

    act.Should().Throw<InvalidOperationException>();
  }

  [Fact]
  public void CreditNote_ShouldRequireApprovedReturn()
  {
    var sale = CreateCompletedSale();
    var saleReturn = CreateReturn(sale);

    var act = () => CreditNote.Generate(Guid.NewGuid(), sale, saleReturn, "NC-001", Now);

    act.Should().Throw<InvalidOperationException>();
  }

  [Fact]
  public void CreditNote_ShouldValidateIdentity()
  {
    var sale = CreateCompletedSale();
    var saleReturn = CreateReturn(sale);
    saleReturn.Approve(Now);

    FluentActions.Invoking(() => CreditNote.Generate(Guid.Empty, sale, saleReturn, "NC-001", Now))
      .Should().Throw<ArgumentException>();
    FluentActions.Invoking(() => CreditNote.Generate(Guid.NewGuid(), sale, saleReturn, " ", Now))
      .Should().Throw<ArgumentException>();
  }

  [Theory]
  [InlineData("id")]
  [InlineData("creditNoteId")]
  [InlineData("saleReturnItemId")]
  [InlineData("productId")]
  public void CreditNoteItem_ShouldRejectMissingIdentifiers(string parameterName)
  {
    var act = () => CreateCreditNoteItemForValidation(parameterName);

    act.Should().Throw<TargetInvocationException>()
      .WithInnerException<ArgumentException>()
      .WithParameterName(parameterName);
  }

  [Theory]
  [InlineData("quantity")]
  [InlineData("unitPrice")]
  public void CreditNoteItem_ShouldRejectInvalidAmounts(string parameterName)
  {
    var act = () => CreateCreditNoteItemForValidation(parameterName);

    act.Should().Throw<TargetInvocationException>()
      .WithInnerException<ArgumentOutOfRangeException>()
      .WithParameterName(parameterName);
  }

  [Fact]
  public void SaleReturnContracts_ShouldExposeValues()
  {
    var item = new CreateSaleReturnItemRequest(Guid.NewGuid(), 1);
    var request = new CreateSaleReturnRequest("Cambio", [item]);
    var generated = new CreditNoteGeneratedEventV1(
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid(),
      null,
      100,
      Now);

    request.Reason.Should().Be("Cambio");
    request.Items.Should().ContainSingle();
    generated.OccurredAt.Should().Be(Now);
  }

  [Fact]
  public async Task RequestSaleReturn_ShouldPersistAndQueueRequestedEvent()
  {
    var scenario = Scenario.Create();
    var sale = CreateCompletedSale(scenario);
    await scenario.Sales.AddAsync(sale);
    var handler = new RequestSaleReturnHandler(
      scenario.ReturnDependencies,
      new RequestSaleReturnCommandValidator());

    var result = await handler.ExecuteAsync(new RequestSaleReturnCommand(
      sale.Id,
      "Cliente devuelve una unidad",
      [new RequestSaleReturnItemCommand(sale.Items.Single().Id, 1)]));

    result.IsSuccess.Should().BeTrue();
    scenario.Returns.Items.Should().ContainSingle(item => item.SaleId == sale.Id);
    scenario.Outbox.Events.Should().ContainSingle(@event => @event is SaleReturnRequestedEventV1);
    scenario.UnitOfWork.SaveChangesCount.Should().Be(1);
  }

  [Fact]
  public async Task RequestSaleReturn_ShouldRejectQuantityAlreadyReturned()
  {
    var scenario = Scenario.Create();
    var sale = CreateCompletedSale(scenario);
    var existingReturn = CreateReturn(sale);
    existingReturn.Approve(Now);
    await scenario.Sales.AddAsync(sale);
    await scenario.Returns.AddAsync(existingReturn);
    var handler = new RequestSaleReturnHandler(
      scenario.ReturnDependencies,
      new RequestSaleReturnCommandValidator());

    var result = await handler.ExecuteAsync(new RequestSaleReturnCommand(
      sale.Id,
      "Intento duplicado",
      [new RequestSaleReturnItemCommand(sale.Items.Single().Id, 2)]));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(SalesErrors.InvalidSaleReturn);
    scenario.Outbox.Events.Should().BeEmpty();
  }

  [Fact]
  public async Task ApproveSaleReturn_ShouldPublishApprovedEvent()
  {
    var scenario = Scenario.Create();
    var sale = CreateCompletedSale(scenario);
    var saleReturn = CreateReturn(sale);
    await scenario.Returns.AddAsync(saleReturn);
    var useCase = new ApproveSaleReturnUseCase(
      scenario.Returns,
      scenario.Outbox,
      scenario.Clock,
      scenario.UnitOfWork);

    var result = await useCase.ExecuteAsync(RequestedEvent(scenario, sale, saleReturn));

    result.IsSuccess.Should().BeTrue();
    saleReturn.Status.Should().Be(SaleReturnStatus.Approved);
    scenario.Outbox.Events.Should().ContainSingle(@event => @event is SaleReturnApprovedEventV1);
  }

  [Fact]
  public async Task RestoreInventoryFromSaleReturn_ShouldCreateReturnMovementOnlyOnce()
  {
    var scenario = Scenario.Create();
    var sale = CreateCompletedSale(scenario);
    var saleReturn = CreateReturn(sale);
    saleReturn.Approve(Now);
    await scenario.Returns.AddAsync(saleReturn);
    var useCase = new RestoreInventoryFromSaleReturnUseCase(
      scenario.Inventory,
      scenario.Outbox,
      scenario.Clock,
      scenario.UnitOfWork);
    var message = ApprovedEvent(scenario, sale, saleReturn);

    var first = await useCase.ExecuteAsync(message);
    var second = await useCase.ExecuteAsync(message);

    first.IsSuccess.Should().BeTrue();
    second.IsSuccess.Should().BeTrue();
    scenario.Inventory.Movements.Should().ContainSingle(movement =>
      movement.Reason == InventoryMovementReason.Return &&
      movement.ReturnId == saleReturn.Id);
    scenario.Outbox.Events.Should().ContainSingle(@event => @event is InventoryRestoredFromReturnEventV1);
  }

  [Fact]
  public async Task GenerateCreditNoteForReturn_ShouldCreateOneCreditNote()
  {
    var scenario = Scenario.Create();
    var sale = CreateCompletedSale(scenario);
    var saleReturn = CreateReturn(sale);
    saleReturn.Approve(Now);
    await scenario.Sales.AddAsync(sale);
    await scenario.Returns.AddAsync(saleReturn);
    var useCase = new GenerateCreditNoteForReturnUseCase(
      scenario.Sales,
      scenario.Returns,
      scenario.CustomerCredits,
      scenario.Outbox,
      scenario.Clock,
      scenario.UnitOfWork);
    var message = ApprovedEvent(scenario, sale, saleReturn);

    var first = await useCase.ExecuteAsync(message);
    var second = await useCase.ExecuteAsync(message);

    first.IsSuccess.Should().BeTrue();
    second.IsSuccess.Should().BeTrue();
    scenario.Returns.CreditNotes.Should().ContainSingle(note => note.SaleReturnId == saleReturn.Id);
    scenario.Outbox.Events.Should().ContainSingle(@event => @event is CreditNoteGeneratedEventV1);
  }

  [Fact]
  public async Task GenerateCreditNoteForReturn_ShouldAdjustCreditBalance_WhenSaleWasCredit()
  {
    var scenario = Scenario.Create();
    var customerId = Guid.NewGuid();
    var sale = CreateCompletedSale(scenario, "Credit", customerId);
    var saleReturn = CreateReturn(sale);
    saleReturn.Approve(Now);
    var account = new CustomerCreditAccount(Guid.NewGuid(), new BusinessId(scenario.BusinessId), customerId, 500, Now);
    account.ApplyDebit(Guid.NewGuid(), sale.Id, 200, "Venta a credito", scenario.UserId, Now);
    await scenario.Sales.AddAsync(sale);
    await scenario.Returns.AddAsync(saleReturn);
    await scenario.CustomerCredits.AddAccountAsync(account);
    var useCase = new GenerateCreditNoteForReturnUseCase(
      scenario.Sales,
      scenario.Returns,
      scenario.CustomerCredits,
      scenario.Outbox,
      scenario.Clock,
      scenario.UnitOfWork);

    var result = await useCase.ExecuteAsync(ApprovedEvent(scenario, sale, saleReturn));

    result.IsSuccess.Should().BeTrue();
    account.CurrentBalance.Should().Be(100);
    scenario.CustomerCredits.Movements.Should().ContainSingle(movement =>
      movement.Type == CustomerCreditMovementType.Cancellation &&
      movement.Amount == 100);
  }

  [Fact]
  public async Task ApproveSaleReturn_ShouldFailReturn_WhenItCannotBeApproved()
  {
    var scenario = Scenario.Create();
    var sale = CreateCompletedSale(scenario);
    var saleReturn = CreateReturn(sale);
    saleReturn.Fail("Validacion externa fallida", Now);
    await scenario.Returns.AddAsync(saleReturn);
    var useCase = new ApproveSaleReturnUseCase(
      scenario.Returns,
      scenario.Outbox,
      scenario.Clock,
      scenario.UnitOfWork);

    var result = await useCase.ExecuteAsync(RequestedEvent(scenario, sale, saleReturn));

    result.IsFailure.Should().BeTrue();
    scenario.Outbox.Events.Should().ContainSingle(@event => @event is SaleReturnFailedEventV1);
  }

  [Fact]
  public void CustomerCreditAccount_ShouldApplyCancellation()
  {
    var account = new CustomerCreditAccount(Guid.NewGuid(), new BusinessId(Guid.NewGuid()), Guid.NewGuid(), 500, Now);
    var saleId = Guid.NewGuid();
    account.ApplyDebit(Guid.NewGuid(), saleId, 150, null, null, Now);

    var movement = account.ApplyCancellation(Guid.NewGuid(), saleId, 50, "Nota de credito", null, Now);
    var excess = () => account.ApplyCancellation(Guid.NewGuid(), saleId, 150, null, null, Now);

    account.CurrentBalance.Should().Be(100);
    movement.Type.Should().Be(CustomerCreditMovementType.Cancellation);
    movement.PreviousBalance.Should().Be(150);
    movement.NewBalance.Should().Be(100);
    excess.Should().Throw<InvalidOperationException>();
  }

  [Fact]
  public async Task EfSaleReturnRepositories_ShouldPersistReadAndFilterByBusiness()
  {
    await using var dbContext = CreateDbContext();
    var scenario = Scenario.Create();
    var sale = CreateCompletedSale(scenario);
    var saleReturn = CreateReturn(sale);
    saleReturn.Approve(Now);
    var creditNote = CreditNote.Generate(Guid.NewGuid(), sale, saleReturn, "NC-20260526-ABC123", Now);
    dbContext.Add(Product(sale.Items.Single().ProductId, new BusinessId(scenario.BusinessId)));
    var writeRepository = new EfSaleReturnRepository(dbContext);
    var readRepository = new EfSaleReturnReadRepository(dbContext);

    await writeRepository.AddAsync(saleReturn);
    await writeRepository.AddCreditNoteAsync(creditNote);
    await dbContext.SaveChangesAsync();

    var loaded = await writeRepository.GetBySaleAsync(new BusinessId(scenario.BusinessId), sale.Id, saleReturn.Id);
    var returned = await writeRepository.GetReturnedQuantityBySaleItemAsync(new BusinessId(scenario.BusinessId), sale.Id);
    var response = await readRepository.GetAsync(new BusinessId(scenario.BusinessId), saleReturn.Id);
    var wrongTenant = await readRepository.GetAsync(new BusinessId(Guid.NewGuid()), saleReturn.Id);

    loaded.Should().NotBeNull();
    returned[sale.Items.Single().Id].Should().Be(1);
    response.Should().NotBeNull();
    response!.Items.Single().ProductName.Should().Be("Cafe molido");
    response.CreditNote!.Code.Should().Be("NC-20260526-ABC123");
    wrongTenant.Should().BeNull();
  }

  [Fact]
  public async Task EfInventoryRepository_ShouldDetectSaleReturnMovement()
  {
    await using var dbContext = CreateDbContext();
    var scenario = Scenario.Create();
    var businessId = new BusinessId(scenario.BusinessId);
    var branchId = new BranchId(scenario.BranchId);
    var returnId = Guid.NewGuid();
    var stockItem = new StockItem(Guid.NewGuid(), businessId, branchId, Guid.NewGuid(), Now);
    var movement = stockItem.ApplyAdjustment(
      1,
      InventoryMovementReason.Return,
      scenario.UserId,
      false,
      Now,
      new InventoryMovementSource(ReturnId: returnId));
    var repository = new SaasCommerce.Modules.Inventory.Infrastructure.Persistence.EfInventoryRepository(dbContext);

    await repository.AddStockItemAsync(stockItem);
    await repository.AddMovementAsync(movement);
    await dbContext.SaveChangesAsync();

    var found = await repository.HasSaleReturnMovementAsync(businessId, branchId, returnId);
    var missing = await repository.HasSaleReturnMovementAsync(businessId, branchId, Guid.NewGuid());

    found.Should().BeTrue();
    missing.Should().BeFalse();
  }

  private static Sale CreateSale(
    Scenario? scenario = null,
    string paymentMethod = "Cash",
    Guid? customerId = null)
  {
    var businessId = scenario?.BusinessId ?? Guid.NewGuid();
    var branchId = scenario?.BranchId ?? Guid.NewGuid();
    var userId = scenario?.UserId ?? Guid.NewGuid();
    var sale = Sale.Create(
      Guid.NewGuid(),
      new BusinessId(businessId),
      new BranchId(branchId),
      userId,
      [new SaleLine(Guid.NewGuid(), 2, 100, 65)],
      paymentMethod,
      Now);

    if (customerId is Guid value)
    {
      sale.AssignCustomer(value);
    }

    return sale;
  }

  private static Sale CreateCompletedSale(
    Scenario? scenario = null,
    string paymentMethod = "Cash",
    Guid? customerId = null)
  {
    var sale = CreateSale(scenario, paymentMethod, customerId);
    sale.MarkAsProcessing(Now.AddMinutes(1));
    sale.Complete(Now.AddMinutes(2));
    return sale;
  }

  private static SaleReturn CreateReturn(Sale sale)
    => SaleReturn.Request(
      Guid.NewGuid(),
      sale,
      sale.UserId,
      "Cliente devuelve producto",
      [new SaleReturnLine(sale.Items.Single().Id, 1)],
      Now);

  private static void CreateCreditNoteItemForValidation(string invalidParameter)
  {
    var constructor = typeof(CreditNoteItem).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
      .Single(ctor => ctor.GetParameters().Length == 6);

    var id = invalidParameter == "id" ? Guid.Empty : Guid.NewGuid();
    var creditNoteId = invalidParameter == "creditNoteId" ? Guid.Empty : Guid.NewGuid();
    var saleReturnItemId = invalidParameter == "saleReturnItemId" ? Guid.Empty : Guid.NewGuid();
    var productId = invalidParameter == "productId" ? Guid.Empty : Guid.NewGuid();
    var quantity = invalidParameter == "quantity" ? 0m : 1m;
    var unitPrice = invalidParameter == "unitPrice" ? -1m : 10m;

    constructor.Invoke([id, creditNoteId, saleReturnItemId, productId, quantity, unitPrice]);
  }

  private static SaleReturnRequestedEventV1 RequestedEvent(Scenario scenario, Sale sale, SaleReturn saleReturn)
    => new(
      Guid.NewGuid(),
      scenario.CorrelationId,
      scenario.BusinessId,
      scenario.BranchId,
      sale.Id,
      saleReturn.Id,
      scenario.UserId,
      SaleReturnItems(saleReturn),
      saleReturn.Total,
      saleReturn.Reason,
      Now);

  private static SaleReturnApprovedEventV1 ApprovedEvent(Scenario scenario, Sale sale, SaleReturn saleReturn)
    => new(
      Guid.NewGuid(),
      scenario.CorrelationId,
      scenario.BusinessId,
      scenario.BranchId,
      sale.Id,
      saleReturn.Id,
      scenario.UserId,
      SaleReturnItems(saleReturn),
      saleReturn.Total,
      Now);

  private static SaleReturnItemV1[] SaleReturnItems(SaleReturn saleReturn)
    => saleReturn.Items
      .Select(item => new SaleReturnItemV1(
        item.SaleItemId,
        item.ProductId,
        item.Quantity,
        item.UnitPrice,
        item.LineTotal))
      .ToArray();

  private static AppDbContext CreateDbContext()
  {
    var options = new DbContextOptionsBuilder<AppDbContext>()
      .UseInMemoryDatabase(Guid.NewGuid().ToString("D"))
      .Options;

    return new AppDbContext(options);
  }

  private static Product Product(Guid id, BusinessId businessId)
    => new(
      new ProductCreationContext(id, businessId, Now),
      new ProductIdentity(ProductType.Simple, "Cafe molido", null, null, null, UnitOfMeasure.Unit),
      new ProductCodes("SKU-001", null, null, null),
      new ProductPricing(100, 65, null, null, TaxCategory.Itbis18, 18, true),
      new ProductInventorySettings(true, null, null, null, false),
      new ProductOptions(true, null, null, null));

  private sealed class Scenario
  {
    private Scenario()
    {
      CurrentUser = new FixedCurrentUser(BusinessId, BranchId, UserId);
    }

    public Guid BusinessId { get; } = Guid.NewGuid();
    public Guid BranchId { get; } = Guid.NewGuid();
    public Guid UserId { get; } = Guid.NewGuid();
    public Guid CorrelationId { get; } = Guid.NewGuid();
    public FixedCurrentUser CurrentUser { get; }
    public InMemorySaleRepository Sales { get; } = new();
    public InMemorySaleReturnRepository Returns { get; } = new();
    public InMemorySaleReturnReadRepository ReturnReads { get; } = new();
    public InMemoryInventoryRepository Inventory { get; } = new();
    public InMemoryCustomerCreditRepository CustomerCredits { get; } = new();
    public RecordingOutboxWriter Outbox { get; } = new();
    public FixedClock Clock { get; } = new();
    public NoopUnitOfWork UnitOfWork { get; } = new();
    public FixedCorrelationIdProvider Correlation => new(CorrelationId);
    public RequestSaleReturnDependencies ReturnDependencies => new()
    {
      Sales = Sales,
      Returns = Returns,
      ReturnReads = ReturnReads,
      CurrentUser = CurrentUser,
      Outbox = Outbox,
      CorrelationIdProvider = Correlation,
      Clock = Clock,
      UnitOfWork = UnitOfWork
    };

    public static Scenario Create() => new();
  }

  private sealed record FixedCurrentUser(Guid CurrentBusinessId, Guid CurrentBranchId, Guid CurrentUserId)
    : ICurrentUserService
  {
    public Guid? UserId => CurrentUserId;
    public Guid? BusinessId => CurrentBusinessId;
    public Guid? BranchId => CurrentBranchId;
    public IReadOnlyCollection<string> Roles => ["Admin"];
    public bool IsAuthenticated => true;
  }

  private sealed record FixedCorrelationIdProvider(Guid Value) : ICorrelationIdProvider
  {
    public string CorrelationId => Value.ToString("D");
  }

  private sealed class FixedClock : IClock
  {
    public DateTimeOffset UtcNow => Now;
  }

  private sealed class NoopUnitOfWork : IUnitOfWork
  {
    public int SaveChangesCount { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
      SaveChangesCount++;
      return Task.FromResult(1);
    }
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

  private sealed class InMemorySaleRepository : ISaleRepository
  {
    private readonly List<Sale> sales = [];

    public Task<Sale?> GetAsync(BusinessId businessId, Guid saleId, CancellationToken cancellationToken = default)
      => Task.FromResult(sales.SingleOrDefault(sale => sale.BusinessId == businessId && sale.Id == saleId));

    public Task AddAsync(Sale sale, CancellationToken cancellationToken = default)
    {
      sales.Add(sale);
      return Task.CompletedTask;
    }

    public Task<int> CountAsync(BusinessId businessId, SaleSearchCriteria criteria, CancellationToken cancellationToken = default)
      => Task.FromResult(sales.Count(sale => sale.BusinessId == businessId));

    public Task<IReadOnlyCollection<Sale>> ListAsync(BusinessId businessId, SaleSearchCriteria criteria, CancellationToken cancellationToken = default)
      => Task.FromResult<IReadOnlyCollection<Sale>>(sales.Where(sale => sale.BusinessId == businessId).ToArray());
  }

  private sealed class InMemorySaleReturnRepository : ISaleReturnRepository
  {
    public List<SaleReturn> Items { get; } = [];
    public List<CreditNote> CreditNotes { get; } = [];

    public Task<SaleReturn?> GetAsync(BusinessId businessId, Guid saleReturnId, CancellationToken cancellationToken = default)
      => Task.FromResult(Items.SingleOrDefault(item => item.BusinessId == businessId && item.Id == saleReturnId));

    public Task<SaleReturn?> GetBySaleAsync(BusinessId businessId, Guid saleId, Guid saleReturnId, CancellationToken cancellationToken = default)
      => Task.FromResult(Items.SingleOrDefault(item =>
        item.BusinessId == businessId &&
        item.SaleId == saleId &&
        item.Id == saleReturnId));

    public Task<CreditNote?> GetCreditNoteByReturnAsync(BusinessId businessId, Guid saleReturnId, CancellationToken cancellationToken = default)
      => Task.FromResult(CreditNotes.SingleOrDefault(note => note.BusinessId == businessId && note.SaleReturnId == saleReturnId));

    public Task<IReadOnlyDictionary<Guid, decimal>> GetReturnedQuantityBySaleItemAsync(
      BusinessId businessId,
      Guid saleId,
      CancellationToken cancellationToken = default)
      => Task.FromResult<IReadOnlyDictionary<Guid, decimal>>(
        Items
          .Where(item => item.BusinessId == businessId && item.SaleId == saleId && item.Status != SaleReturnStatus.Failed)
          .SelectMany(item => item.Items)
          .GroupBy(item => item.SaleItemId)
          .ToDictionary(group => group.Key, group => group.Sum(item => item.Quantity)));

    public Task AddAsync(SaleReturn saleReturn, CancellationToken cancellationToken = default)
    {
      Items.Add(saleReturn);
      return Task.CompletedTask;
    }

    public Task AddCreditNoteAsync(CreditNote creditNote, CancellationToken cancellationToken = default)
    {
      CreditNotes.Add(creditNote);
      return Task.CompletedTask;
    }
  }

  private sealed class InMemorySaleReturnReadRepository : ISaleReturnReadRepository
  {
    public Task<SaleReturnResponse?> GetAsync(BusinessId businessId, Guid saleReturnId, CancellationToken cancellationToken = default)
      => Task.FromResult<SaleReturnResponse?>(
        new SaleReturnResponse(
          saleReturnId,
          Guid.NewGuid(),
          businessId.Value,
          Guid.NewGuid(),
          Guid.NewGuid(),
          SaleReturnStatus.Requested.ToString(),
          "Cliente devuelve producto",
          100,
          [],
          null,
          Now,
          null,
          null,
          null));

    public Task<IReadOnlyCollection<SaleReturnResponse>> ListBySaleAsync(BusinessId businessId, Guid saleId, CancellationToken cancellationToken = default)
      => Task.FromResult<IReadOnlyCollection<SaleReturnResponse>>([]);
  }

  private sealed class InMemoryInventoryRepository : IInventoryRepository
  {
    private readonly Dictionary<(Guid BusinessId, Guid BranchId, Guid ProductId), StockItem> stock = [];

    public List<InventoryMovement> Movements { get; } = [];

    public Task<StockItem?> GetStockItemAsync(BusinessId businessId, BranchId branchId, Guid productId, CancellationToken cancellationToken = default)
      => Task.FromResult(stock.TryGetValue((businessId.Value, branchId.Value, productId), out var item) ? item : null);

    public Task AddStockItemAsync(StockItem stockItem, CancellationToken cancellationToken = default)
    {
      stock[(stockItem.BusinessId.Value, stockItem.BranchId.Value, stockItem.ProductId)] = stockItem;
      return Task.CompletedTask;
    }

    public Task AddMovementAsync(InventoryMovement movement, CancellationToken cancellationToken = default)
    {
      Movements.Add(movement);
      return Task.CompletedTask;
    }

    public Task<bool> HasSaleMovementAsync(BusinessId businessId, BranchId branchId, Guid saleId, CancellationToken cancellationToken = default)
      => Task.FromResult(false);

    public Task<bool> HasPurchaseMovementAsync(BusinessId businessId, BranchId branchId, Guid purchaseId, CancellationToken cancellationToken = default)
      => Task.FromResult(false);

    public Task<bool> HasSaleReturnMovementAsync(BusinessId businessId, BranchId branchId, Guid saleReturnId, CancellationToken cancellationToken = default)
      => Task.FromResult(Movements.Any(movement =>
        movement.BusinessId == businessId &&
        movement.BranchId == branchId &&
        movement.ReturnId == saleReturnId));

    public Task<int> CountStockAsync(BusinessId businessId, BranchId? branchId, StockSearchCriteria criteria, CancellationToken cancellationToken = default)
      => Task.FromResult(0);

    public Task<IReadOnlyCollection<StockItem>> ListStockAsync(BusinessId businessId, BranchId? branchId, StockSearchCriteria criteria, CancellationToken cancellationToken = default)
      => Task.FromResult<IReadOnlyCollection<StockItem>>([]);

    public Task<int> CountMovementsAsync(BusinessId businessId, BranchId branchId, InventoryMovementSearchCriteria criteria, CancellationToken cancellationToken = default)
      => Task.FromResult(Movements.Count);

    public Task<IReadOnlyCollection<InventoryMovement>> ListMovementsAsync(BusinessId businessId, BranchId branchId, InventoryMovementSearchCriteria criteria, CancellationToken cancellationToken = default)
      => Task.FromResult<IReadOnlyCollection<InventoryMovement>>(Movements);
  }

  private sealed class InMemoryCustomerCreditRepository : ICustomerCreditRepository
  {
    private readonly List<CustomerCreditAccount> accounts = [];

    public List<CustomerCreditMovement> Movements { get; } = [];

    public Task<CustomerCreditAccount?> GetAccountAsync(BusinessId businessId, Guid customerId, CancellationToken cancellationToken = default)
      => Task.FromResult(accounts.SingleOrDefault(account =>
        account.BusinessId == businessId &&
        account.CustomerId == customerId));

    public Task<IReadOnlyDictionary<Guid, CustomerCreditAccount>> ListAccountsAsync(BusinessId businessId, IReadOnlyCollection<Guid> customerIds, CancellationToken cancellationToken = default)
      => Task.FromResult<IReadOnlyDictionary<Guid, CustomerCreditAccount>>(new Dictionary<Guid, CustomerCreditAccount>());

    public Task AddAccountAsync(CustomerCreditAccount account, CancellationToken cancellationToken = default)
    {
      accounts.Add(account);
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
      => Task.FromResult(0);

    public Task<IReadOnlyCollection<CustomerCreditMovement>> ListMovementsAsync(BusinessId businessId, Guid customerId, int page, int pageSize, CancellationToken cancellationToken = default)
      => Task.FromResult<IReadOnlyCollection<CustomerCreditMovement>>([]);

    public Task<IReadOnlyCollection<CustomerCreditAccount>> ExportAllAccountsAsync(BusinessId businessId, CancellationToken cancellationToken = default)
      => Task.FromResult<IReadOnlyCollection<CustomerCreditAccount>>([]);
  }
}
