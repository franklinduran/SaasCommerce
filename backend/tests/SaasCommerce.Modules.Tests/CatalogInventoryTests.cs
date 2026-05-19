#pragma warning disable CA1707

using FluentAssertions;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Audit;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SaasCommerce.BuildingBlocks.Contracts.Events;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Catalog.Application.Categories;
using SaasCommerce.Modules.Catalog.Application.Products;
using SaasCommerce.Modules.Catalog.Contracts.Inventory;
using SaasCommerce.Modules.Catalog.Contracts.Sales;
using SaasCommerce.Modules.Catalog.Domain;
using SaasCommerce.Modules.Catalog.Infrastructure.Persistence;
using SaasCommerce.Modules.Catalog.Infrastructure.Inventory;
using SaasCommerce.Modules.Inventory.Application.Abstractions;
using SaasCommerce.Modules.Inventory.Application.Stock;
using SaasCommerce.Modules.Inventory.Contracts.Availability;
using SaasCommerce.Modules.Inventory.Contracts.Events.V1;
using SaasCommerce.Modules.Inventory.Domain;
using SaasCommerce.Modules.Inventory.Infrastructure.Availability;
using SaasCommerce.Modules.Inventory.Infrastructure.Persistence;
using SaasCommerce.Modules.Tenancy.Domain;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Tests;

public sealed class CatalogInventoryTests
{
  [Fact]
  public async Task CreateProductShouldPersistProductForCurrentBusiness()
  {
    await using var dbContext = CreateDbContext();
    var currentUser = TestCurrentUser.Create();
    var handler = CreateProductHandler(dbContext, currentUser);

    var result = await handler.Handle(CreateProductCommand("Cafe molido", "sku-001", "1234567890"));

    result.IsSuccess.Should().BeTrue();
    result.Value.BusinessId.Should().Be(currentUser.BusinessId!.Value);
    result.Value.Sku.Should().Be("SKU-001");
    result.Value.Barcode.Should().Be("1234567890");
    result.Value.SalePrice.Should().Be(250);
    result.Value.CostPrice.Should().Be(150);
    result.Value.ProfitMargin.Should().Be(40);
  }

  [Fact]
  public async Task CreateProductShouldRejectDuplicateSkuInsideSameBusiness()
  {
    await using var dbContext = CreateDbContext();
    var currentUser = TestCurrentUser.Create();
    var handler = CreateProductHandler(dbContext, currentUser);

    await handler.Handle(CreateProductCommand("Cafe", "SKU-001"));
    var duplicate = await handler.Handle(CreateProductCommand("Cafe premium", "sku-001"));

    duplicate.IsFailure.Should().BeTrue();
    duplicate.Error.Should().Be(CatalogErrors.DuplicateSku);
  }

  [Fact]
  public async Task CreateProductShouldRejectDuplicateBarcodeInsideSameBusiness()
  {
    await using var dbContext = CreateDbContext();
    var currentUser = TestCurrentUser.Create();
    var handler = CreateProductHandler(dbContext, currentUser);

    await handler.Handle(CreateProductCommand("Cafe", "SKU-001", "789"));
    var duplicate = await handler.Handle(CreateProductCommand("Cafe premium", "SKU-002", "789"));

    duplicate.IsFailure.Should().BeTrue();
    duplicate.Error.Should().Be(CatalogErrors.DuplicateBarcode);
  }

  [Fact]
  public async Task CreateProductShouldRejectServiceWithInventoryTracking()
  {
    await using var dbContext = CreateDbContext();
    var currentUser = TestCurrentUser.Create();
    var handler = CreateProductHandler(dbContext, currentUser);

    var result = await handler.Handle(CreateProductCommand(
      "Delivery",
      "SRV-001",
      productType: "Service",
      unitOfMeasure: "Service",
      trackInventory: true));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(CatalogErrors.InvalidProduct);
  }

  [Fact]
  public async Task CreateProductShouldRejectWeighedProductWithUnitMeasure()
  {
    await using var dbContext = CreateDbContext();
    var currentUser = TestCurrentUser.Create();
    var handler = CreateProductHandler(dbContext, currentUser);

    var result = await handler.Handle(CreateProductCommand(
      "Arroz suelto",
      "ARR-001",
      productType: "Weighed",
      unitOfMeasure: "Unit"));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(CatalogErrors.InvalidProduct);
  }

  [Fact]
  public async Task GetProductsShouldReturnPagedResultWithTotalPages()
  {
    await using var dbContext = CreateDbContext();
    var currentUser = TestCurrentUser.Create();
    var createHandler = CreateProductHandler(dbContext, currentUser);
    var listHandler = new GetProductsHandler(
      new EfCatalogProductRepository(dbContext),
      currentUser);

    for (var index = 1; index <= 12; index++)
    {
      await createHandler.Handle(CreateProductCommand($"Producto {index:00}", $"SKU-{index:00}"));
    }

    var result = await listHandler.Handle(new GetProductsQuery(
      "Producto",
      "Simple",
      null,
      true,
      2,
      10,
      "name",
      "asc"));

    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(2);
    result.Value.Page.Should().Be(2);
    result.Value.PageSize.Should().Be(10);
    result.Value.TotalItems.Should().Be(12);
    result.Value.TotalPages.Should().Be(2);
    result.Value.HasPreviousPage.Should().BeTrue();
    result.Value.HasNextPage.Should().BeFalse();
  }

  [Fact]
  public async Task CreateCategoryShouldPersistCategoryForCurrentBusiness()
  {
    await using var dbContext = CreateDbContext();
    var currentUser = TestCurrentUser.Create();
    var handler = CreateCategoryHandler(dbContext, currentUser);

    var result = await handler.Handle(new CreateCategoryCommand("Bebidas", "Productos liquidos"));

    result.IsSuccess.Should().BeTrue();
    result.Value.BusinessId.Should().Be(currentUser.BusinessId!.Value);
    result.Value.Name.Should().Be("Bebidas");
    result.Value.Description.Should().Be("Productos liquidos");
    result.Value.IsActive.Should().BeTrue();
  }

  [Fact]
  public async Task CreateCategoryShouldRejectDuplicateNameInsideSameBusiness()
  {
    await using var dbContext = CreateDbContext();
    var currentUser = TestCurrentUser.Create();
    var handler = CreateCategoryHandler(dbContext, currentUser);

    await handler.Handle(new CreateCategoryCommand("Bebidas", null));
    var duplicate = await handler.Handle(new CreateCategoryCommand("Bebidas", null));

    duplicate.IsFailure.Should().BeTrue();
    duplicate.Error.Should().Be(CatalogErrors.DuplicateCategory);
  }

  [Fact]
  public async Task UpdateCategoryShouldUpdateOnlyCurrentBusinessCategory()
  {
    await using var dbContext = CreateDbContext();
    var currentUser = TestCurrentUser.Create();
    var createHandler = CreateCategoryHandler(dbContext, currentUser);
    var updateHandler = UpdateCategoryHandler(dbContext, currentUser);

    var created = await createHandler.Handle(new CreateCategoryCommand("Bebidas", null));
    var result = await updateHandler.Handle(new UpdateCategoryCommand(
      created.Value.Id,
      "Bebidas frias",
      "Nevera",
      false));

    result.IsSuccess.Should().BeTrue();
    result.Value.Name.Should().Be("Bebidas frias");
    result.Value.Description.Should().Be("Nevera");
    result.Value.IsActive.Should().BeFalse();
  }

  [Fact]
  public async Task GetCategoriesShouldReturnOnlyCurrentBusinessCategories()
  {
    await using var dbContext = CreateDbContext();
    var currentUser = TestCurrentUser.Create();
    var otherUser = TestCurrentUser.Create();

    await CreateCategoryHandler(dbContext, currentUser)
      .Handle(new CreateCategoryCommand("Bebidas", null));
    await CreateCategoryHandler(dbContext, otherUser)
      .Handle(new CreateCategoryCommand("Ferreteria", null));

    var result = await new GetCategoriesHandler(
        new EfCatalogCategoryRepository(dbContext),
        currentUser)
      .Handle();

    result.IsSuccess.Should().BeTrue();
    result.Value.Should().ContainSingle();
    result.Value.Single().Name.Should().Be("Bebidas");
  }

  [Fact]
  public async Task GetProductsShouldRejectUnsupportedPageSize()
  {
    await using var dbContext = CreateDbContext();
    var currentUser = TestCurrentUser.Create();
    var handler = new GetProductsHandler(new EfCatalogProductRepository(dbContext), currentUser);

    var result = await handler.Handle(new GetProductsQuery(null, null, null, true, 1, 20, "name", "asc"));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(CatalogErrors.InvalidProduct);
  }

  [Fact]
  public async Task GetProductsShouldSortBySalePriceDescending()
  {
    await using var dbContext = CreateDbContext();
    var currentUser = TestCurrentUser.Create();
    var createHandler = CreateProductHandler(dbContext, currentUser);
    var listHandler = new GetProductsHandler(new EfCatalogProductRepository(dbContext), currentUser);

    await createHandler.Handle(CreateProductCommand("Producto barato", "SKU-LOW", salePrice: 100));
    await createHandler.Handle(CreateProductCommand("Producto caro", "SKU-HIGH", salePrice: 300));

    var result = await listHandler.Handle(new GetProductsQuery(null, null, null, true, 1, 10, "salePrice", "desc"));

    result.IsSuccess.Should().BeTrue();
    result.Value.Items.First().Name.Should().Be("Producto caro");
  }

  [Fact]
  public async Task DeactivateProductShouldSetProductInactive()
  {
    await using var dbContext = CreateDbContext();
    var currentUser = TestCurrentUser.Create();
    var createHandler = CreateProductHandler(dbContext, currentUser);
    var created = await createHandler.Handle(CreateProductCommand("Cafe", "SKU-001"));
    var handler = new DeactivateProductHandler(
      new EfCatalogProductRepository(dbContext),
      currentUser,
      new FixedClock(),
      new EfUnitOfWork(dbContext));

    var result = await handler.Handle(new DeactivateProductCommand(created.Value.Id));

    result.IsSuccess.Should().BeTrue();
    result.Value.IsActive.Should().BeFalse();
  }

  [Fact]
  public async Task ActivateProductShouldSetProductActive()
  {
    await using var dbContext = CreateDbContext();
    var currentUser = TestCurrentUser.Create();
    var createHandler = CreateProductHandler(dbContext, currentUser);
    var created = await createHandler.Handle(CreateProductCommand("Cafe", "SKU-001"));
    var repository = new EfCatalogProductRepository(dbContext);
    await new DeactivateProductHandler(repository, currentUser, new FixedClock(), new EfUnitOfWork(dbContext))
      .Handle(new DeactivateProductCommand(created.Value.Id));

    var result = await new ActivateProductHandler(repository, currentUser, new FixedClock(), new EfUnitOfWork(dbContext))
      .Handle(new ActivateProductCommand(created.Value.Id));

    result.IsSuccess.Should().BeTrue();
    result.Value.IsActive.Should().BeTrue();
  }

  [Fact]
  public async Task DeactivateProductShouldNotUpdateOtherTenantProduct()
  {
    await using var dbContext = CreateDbContext();
    var currentUser = TestCurrentUser.Create();
    var otherUser = TestCurrentUser.Create();
    var created = await CreateProductHandler(dbContext, otherUser)
      .Handle(CreateProductCommand("Cafe", "SKU-001"));
    var handler = new DeactivateProductHandler(
      new EfCatalogProductRepository(dbContext),
      currentUser,
      new FixedClock(),
      new EfUnitOfWork(dbContext));

    var result = await handler.Handle(new DeactivateProductCommand(created.Value.Id));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(CatalogErrors.ProductNotFound);
  }

  [Fact]
  public async Task AdjustInventoryShouldCreateStockItemAndMovement()
  {
    await using var dbContext = CreateDbContext();
    var currentUser = TestCurrentUser.Create();
    var productId = Guid.NewGuid();
    var handler = CreateInventoryHandler(
      dbContext,
      currentUser,
      ProductInventoryPolicy(productId, currentUser.BusinessId!.Value));

    var result = await handler.Handle(new AdjustInventoryCommand(
      productId,
      12,
      "InitialLoad"));

    result.IsSuccess.Should().BeTrue();
    result.Value.StockItem.ProductId.Should().Be(productId);
    result.Value.StockItem.BranchId.Should().Be(currentUser.BranchId!.Value);
    result.Value.StockItem.Quantity.Should().Be(12);
    result.Value.Movement.PreviousStock.Should().Be(0);
    result.Value.Movement.NewStock.Should().Be(12);
    result.Value.Movement.BranchId.Should().Be(currentUser.BranchId!.Value);
    result.Value.Movement.UserId.Should().Be(currentUser.UserId!.Value);
  }

  [Fact]
  public async Task AdjustInventoryShouldRejectNegativeResultingStock()
  {
    await using var dbContext = CreateDbContext();
    var currentUser = TestCurrentUser.Create();
    var productId = Guid.NewGuid();
    var handler = CreateInventoryHandler(
      dbContext,
      currentUser,
      ProductInventoryPolicy(productId, currentUser.BusinessId!.Value));

    var result = await handler.Handle(new AdjustInventoryCommand(
      productId,
      -1,
      "Adjustment"));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(InventoryErrors.NegativeStock);
  }

  [Fact]
  public async Task AdjustInventoryShouldRejectProductsWithoutInventoryTracking()
  {
    await using var dbContext = CreateDbContext();
    var currentUser = TestCurrentUser.Create();
    var productId = Guid.NewGuid();
    var handler = CreateInventoryHandler(
      dbContext,
      currentUser,
      ProductInventoryPolicy(
        productId,
        currentUser.BusinessId!.Value,
        productType: "Service",
        trackInventory: false,
        unitOfMeasure: "Service"));

    var result = await handler.Handle(new AdjustInventoryCommand(productId, 1, "Adjustment"));

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(InventoryErrors.ProductDoesNotTrackInventory);
  }

  [Fact]
  public async Task AdjustInventory_ShouldPublishLowStockDetectedEvent_WhenStockFallsBelowMinimum()
  {
    await using var dbContext = CreateDbContext();
    var currentUser = TestCurrentUser.Create();
    var productId = Guid.NewGuid();
    var outbox = new RecordingOutboxWriter();
    var handler = new AdjustInventoryHandler(
      new EfInventoryRepository(dbContext),
      new TestProductInventoryPolicyReader(
        ProductInventoryPolicy(productId, currentUser.BusinessId!.Value, minimumStock: 5)),
      currentUser,
      outbox,
      new NoopAuditLogWriter(),
      new FixedClock(),
      new EfUnitOfWork(dbContext));

    var result = await handler.Handle(new AdjustInventoryCommand(productId, 3, "InitialStock"));

    result.IsSuccess.Should().BeTrue();
    outbox.Events
      .OfType<LowStockDetectedEventV1>()
      .Should()
      .ContainSingle(@event => @event.ProductId == productId && @event.CurrentStock == 3 && @event.MinimumStock == 5);
  }

  [Fact]
  public async Task GetStockShouldFilterByCurrentBranch()
  {
    await using var dbContext = CreateDbContext();
    var currentUser = TestCurrentUser.Create();
    var otherBranchUser = currentUser with { BranchId = Guid.NewGuid() };
    var firstProductId = Guid.NewGuid();
    var secondProductId = Guid.NewGuid();

    await CreateInventoryHandler(
        dbContext,
        currentUser,
        ProductInventoryPolicy(firstProductId, currentUser.BusinessId!.Value))
      .Handle(new AdjustInventoryCommand(firstProductId, 5, "InitialLoad"));
    await CreateInventoryHandler(
        dbContext,
        otherBranchUser,
        ProductInventoryPolicy(secondProductId, currentUser.BusinessId!.Value))
      .Handle(new AdjustInventoryCommand(secondProductId, 8, "InitialLoad"));

    var result = await new GetStockHandler(
        new EfInventoryRepository(dbContext),
        new TestProductInventoryPolicyReader(ProductInventoryPolicy(firstProductId, currentUser.BusinessId!.Value)),
        currentUser)
      .Handle(new GetStockQuery(null, null, null, false, false, null, null, 1, 10, null, null));

    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().ContainSingle();
    result.Value.Items.Single().ProductId.Should().Be(firstProductId);
  }

  [Fact]
  public async Task GetStockShouldFilterLowStockOnly()
  {
    await using var dbContext = CreateDbContext();
    var currentUser = TestCurrentUser.Create();
    var productId = Guid.NewGuid();
    var productPolicy = ProductInventoryPolicy(productId, currentUser.BusinessId!.Value);

    await CreateInventoryHandler(dbContext, currentUser, productPolicy)
      .Handle(new AdjustInventoryCommand(productId, 5, "InitialLoad"));

    var result = await new GetStockHandler(
        new EfInventoryRepository(dbContext),
        new TestProductInventoryPolicyReader(productPolicy, minimumStock: 10),
        currentUser)
      .Handle(new GetStockQuery(null, null, null, true, false, null, null, 1, 10, null, null));

    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().ContainSingle();
    result.Value.Items.Single().IsLowStock.Should().BeTrue();
  }

  [Fact]
  public async Task GetInventoryMovementsShouldFilterByReason()
  {
    await using var dbContext = CreateDbContext();
    var currentUser = TestCurrentUser.Create();
    var productId = Guid.NewGuid();
    var productPolicy = ProductInventoryPolicy(productId, currentUser.BusinessId!.Value);
    var handler = CreateInventoryHandler(dbContext, currentUser, productPolicy);

    await handler.Handle(new AdjustInventoryCommand(productId, 5, "InitialLoad"));
    await handler.Handle(new AdjustInventoryCommand(productId, 8, "Purchase"));

    var result = await new GetInventoryMovementsHandler(new EfInventoryRepository(dbContext), currentUser)
      .Handle(new GetInventoryMovementsQuery(null, "Purchase", null, null, 1, 10, null, null));

    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().ContainSingle();
    result.Value.Items.Single().Reason.Should().Be("PurchaseEntry");
  }

  [Fact]
  public async Task ProductSalesPolicyShouldBlockInactiveProducts()
  {
    await using var dbContext = CreateDbContext();
    var currentUser = TestCurrentUser.Create();
    var created = await CreateProductHandler(dbContext, currentUser)
      .Handle(CreateProductCommand("Cafe", "SKU-001"));
    await new DeactivateProductHandler(
        new EfCatalogProductRepository(dbContext),
        currentUser,
        new FixedClock(),
        new EfUnitOfWork(dbContext))
      .Handle(new DeactivateProductCommand(created.Value.Id));

    var policy = await new EfProductInventoryPolicyReader(dbContext)
      .GetSalesPolicyAsync(currentUser.BusinessId!.Value, created.Value.Id);

    policy.Should().NotBeNull();
    policy!.CanBeSold.Should().BeFalse();
    policy.ReasonIfCannotBeSold.Should().Be("Product is inactive.");
  }

  [Fact]
  public async Task ProductSalesPolicyShouldAllowActiveServicesWithoutInventory()
  {
    await using var dbContext = CreateDbContext();
    var currentUser = TestCurrentUser.Create();
    var created = await CreateProductHandler(dbContext, currentUser)
      .Handle(CreateProductCommand(
        "Delivery",
        "SRV-001",
        productType: "Service",
        unitOfMeasure: "Service",
        trackInventory: false,
        salePrice: 100));

    var policy = await new EfProductInventoryPolicyReader(dbContext)
      .GetSalesPolicyAsync(currentUser.BusinessId!.Value, created.Value.Id);

    policy.Should().NotBeNull();
    policy!.CanBeSold.Should().BeTrue();
    policy.TrackInventory.Should().BeFalse();
    policy.ProductType.Should().Be("Service");
  }

  [Fact]
  public async Task InventoryAvailabilityShouldRejectInsufficientStockWhenNegativeStockIsNotAllowed()
  {
    await using var dbContext = CreateDbContext();
    var currentUser = TestCurrentUser.Create();
    var productId = Guid.NewGuid();
    await CreateInventoryHandler(
        dbContext,
        currentUser,
        ProductInventoryPolicy(productId, currentUser.BusinessId!.Value))
      .Handle(new AdjustInventoryCommand(productId, 3, "InitialLoad"));

    var result = await new EfInventoryAvailabilityService(new EfInventoryRepository(dbContext))
      .ValidateStockAsync(new InventoryAvailabilityRequest(
        currentUser.BusinessId.Value,
        currentUser.BranchId!.Value,
        productId,
        5,
        true,
        false));

    result.IsAvailable.Should().BeFalse();
    result.AvailableQuantity.Should().Be(3);
    result.ReasonIfUnavailable.Should().Be("Insufficient stock.");
  }

  [Fact]
  public async Task InventoryAvailabilityShouldAllowNonInventoryProducts()
  {
    await using var dbContext = CreateDbContext();
    var currentUser = TestCurrentUser.Create();

    var result = await new EfInventoryAvailabilityService(new EfInventoryRepository(dbContext))
      .ValidateStockAsync(new InventoryAvailabilityRequest(
        currentUser.BusinessId!.Value,
        currentUser.BranchId!.Value,
        Guid.NewGuid(),
        100,
        false,
        false));

    result.IsAvailable.Should().BeTrue();
    result.AvailableQuantity.Should().Be(0);
  }

  [Fact]
  public void InventoryItem_ShouldIncreaseStock_WhenMovementIsEntry()
  {
    var stockItem = new StockItem(
      Guid.NewGuid(),
      new BusinessId(Guid.NewGuid()),
      new BranchId(Guid.NewGuid()),
      Guid.NewGuid(),
      new FixedClock().UtcNow);

    var movement = stockItem.ApplyAdjustment(
      7,
      InventoryMovementReason.PurchaseEntry,
      Guid.NewGuid(),
      false,
      new FixedClock().UtcNow);

    stockItem.Quantity.Should().Be(7);
    movement.PreviousStock.Should().Be(0);
    movement.NewStock.Should().Be(7);
  }

  [Fact]
  public void InventoryItem_ShouldDecreaseStock_WhenMovementIsSaleDeduction()
  {
    var stockItem = new StockItem(
      Guid.NewGuid(),
      new BusinessId(Guid.NewGuid()),
      new BranchId(Guid.NewGuid()),
      Guid.NewGuid(),
      new FixedClock().UtcNow);
    stockItem.ApplyAdjustment(10, InventoryMovementReason.InitialStock, Guid.NewGuid(), false, new FixedClock().UtcNow);

    var movement = stockItem.ApplyAdjustment(
      -4,
      InventoryMovementReason.SaleDeduction,
      Guid.NewGuid(),
      false,
      new FixedClock().UtcNow,
      new InventoryMovementSource(SaleId: Guid.NewGuid()));

    stockItem.Quantity.Should().Be(6);
    movement.PreviousStock.Should().Be(10);
    movement.NewStock.Should().Be(6);
    movement.SaleId.Should().NotBeNull();
  }

  [Fact]
  public void InventoryItem_ShouldThrow_WhenStockWouldBeNegative()
  {
    var stockItem = new StockItem(
      Guid.NewGuid(),
      new BusinessId(Guid.NewGuid()),
      new BranchId(Guid.NewGuid()),
      Guid.NewGuid(),
      new FixedClock().UtcNow);

    var action = () => stockItem.ApplyAdjustment(
      -1,
      InventoryMovementReason.ManualAdjustment,
      Guid.NewGuid(),
      false,
      new FixedClock().UtcNow);

    action.Should().Throw<InvalidOperationException>();
  }

  [Fact]
  public void InventoryItem_ShouldDetectLowStock_WhenCurrentStockIsBelowMinimum()
  {
    var stockItem = new StockItem(
      Guid.NewGuid(),
      new BusinessId(Guid.NewGuid()),
      new BranchId(Guid.NewGuid()),
      Guid.NewGuid(),
      new FixedClock().UtcNow);
    stockItem.ApplyAdjustment(3, InventoryMovementReason.InitialStock, Guid.NewGuid(), false, new FixedClock().UtcNow);

    stockItem.IsLowStock(5).Should().BeTrue();
  }

  [Fact]
  public async Task InventoryReadRepository_ShouldReturnLowStockItems()
  {
    await using var dbContext = CreateDbContext();
    var businessId = new BusinessId(Guid.NewGuid());
    var branchId = new BranchId(Guid.NewGuid());
    var productId = Guid.NewGuid();
    dbContext.Add(new Branch(branchId, businessId, "Principal", new FixedClock().UtcNow, isMain: true));
    dbContext.Add(Product(productId, businessId, "Cafe", "SKU-LOW", minimumStock: 10));
    var stockItem = new StockItem(Guid.NewGuid(), businessId, branchId, productId, new FixedClock().UtcNow);
    stockItem.ApplyAdjustment(4, InventoryMovementReason.InitialStock, Guid.NewGuid(), false, new FixedClock().UtcNow);
    dbContext.Add(stockItem);
    await dbContext.SaveChangesAsync();

    var result = await new EfInventoryReadRepository(dbContext)
      .ListAsync(
        businessId,
        new InventoryReadCriteria(null, null, null, true, false, 1, 10, StockSortOption.ProductId, InventorySortDirection.Asc));

    result.Should().ContainSingle();
    result.Single().Status.Should().Be("LowStock");
  }

  [Fact]
  public async Task InventoryRepository_ShouldFilterByBusinessId()
  {
    await using var dbContext = CreateDbContext();
    var businessId = new BusinessId(Guid.NewGuid());
    var otherBusinessId = new BusinessId(Guid.NewGuid());
    var branchId = new BranchId(Guid.NewGuid());
    var productId = Guid.NewGuid();
    var otherProductId = Guid.NewGuid();
    var otherBranchId = new BranchId(Guid.NewGuid());
    dbContext.Add(new Branch(branchId, businessId, "Principal", new FixedClock().UtcNow, isMain: true));
    dbContext.Add(new Branch(otherBranchId, otherBusinessId, "Otra", new FixedClock().UtcNow, isMain: true));
    dbContext.Add(Product(productId, businessId, "Cafe", "SKU-001", minimumStock: 1));
    dbContext.Add(Product(otherProductId, otherBusinessId, "Otro cafe", "SKU-002", minimumStock: 1));
    dbContext.Add(new StockItem(Guid.NewGuid(), businessId, branchId, productId, new FixedClock().UtcNow));
    dbContext.Add(new StockItem(Guid.NewGuid(), otherBusinessId, otherBranchId, otherProductId, new FixedClock().UtcNow));
    await dbContext.SaveChangesAsync();

    var result = await new EfInventoryReadRepository(dbContext)
      .ListAsync(
        businessId,
        new InventoryReadCriteria(null, null, null, false, false, 1, 10, StockSortOption.ProductId, InventorySortDirection.Asc));

    result.Should().ContainSingle();
    result.Single().BusinessId.Should().Be(businessId.Value);
  }

  [Fact]
  public async Task InventoryMovementRepository_ShouldNotReturnOtherBusinessMovements()
  {
    await using var dbContext = CreateDbContext();
    var currentUser = TestCurrentUser.Create();
    var otherUser = TestCurrentUser.Create();
    var productId = Guid.NewGuid();
    await CreateInventoryHandler(
        dbContext,
        currentUser,
        ProductInventoryPolicy(productId, currentUser.BusinessId!.Value))
      .Handle(new AdjustInventoryCommand(productId, 5, "InitialStock"));
    await CreateInventoryHandler(
        dbContext,
        otherUser,
        ProductInventoryPolicy(productId, otherUser.BusinessId!.Value))
      .Handle(new AdjustInventoryCommand(productId, 8, "InitialStock"));

    var result = await new EfInventoryRepository(dbContext)
      .ListMovementsAsync(
        new BusinessId(currentUser.BusinessId.Value),
        new BranchId(currentUser.BranchId!.Value),
        new InventoryMovementSearchCriteria(null, null, null, null, 1, 10, InventoryMovementSortOption.CreatedAt, InventorySortDirection.Desc));

    result.Should().ContainSingle();
    result.Single().BusinessId.Value.Should().Be(currentUser.BusinessId.Value);
  }

  [Fact]
  public async Task InventoryReadRepositoryShouldReturnProductDetailWithBranchesMovementsAndAlerts()
  {
    await using var dbContext = CreateDbContext();
    var businessId = new BusinessId(Guid.NewGuid());
    var firstBranchId = new BranchId(Guid.NewGuid());
    var secondBranchId = new BranchId(Guid.NewGuid());
    var productId = Guid.NewGuid();
    var userId = Guid.NewGuid();
    dbContext.Add(new Branch(firstBranchId, businessId, "Principal", new FixedClock().UtcNow, isMain: true));
    dbContext.Add(new Branch(secondBranchId, businessId, "Secundaria", new FixedClock().UtcNow, isMain: false));
    dbContext.Add(Product(productId, businessId, "Cafe", "SKU-DETAIL", minimumStock: 5));
    var stockItem = new StockItem(Guid.NewGuid(), businessId, firstBranchId, productId, new FixedClock().UtcNow);
    var movement = stockItem.ApplyAdjustment(3, InventoryMovementReason.InitialStock, userId, false, new FixedClock().UtcNow);
    dbContext.Add(stockItem);
    dbContext.Add(movement);
    await dbContext.SaveChangesAsync();

    var detail = await new EfInventoryReadRepository(dbContext)
      .GetProductDetailAsync(businessId, productId);

    detail.Should().NotBeNull();
    detail!.Branches.Should().HaveCount(2);
    detail.Branches.Should().Contain(branch => branch.BranchName == "Secundaria" && branch.CurrentStock == 0);
    detail.RecentMovements.Should().ContainSingle(item => item.NewStock == 3);
    detail.Alerts.Should().Contain(alert => alert.BranchName == "Principal" && alert.CurrentStock == 3);
  }

  [Fact]
  public async Task InventoryReadRepositoryShouldReturnNullWhenProductBelongsToAnotherBusiness()
  {
    await using var dbContext = CreateDbContext();
    var businessId = new BusinessId(Guid.NewGuid());
    var otherBusinessId = new BusinessId(Guid.NewGuid());
    var productId = Guid.NewGuid();
    dbContext.Add(Product(productId, otherBusinessId, "Cafe", "SKU-OTHER", minimumStock: 5));
    await dbContext.SaveChangesAsync();

    var detail = await new EfInventoryReadRepository(dbContext)
      .GetProductDetailAsync(businessId, productId);

    detail.Should().BeNull();
  }

  [Fact]
  public async Task InventoryReadRepositoryShouldApplySearchOutOfStockAndQuantitySorting()
  {
    await using var dbContext = CreateDbContext();
    var businessId = new BusinessId(Guid.NewGuid());
    var branchId = new BranchId(Guid.NewGuid());
    var cafeId = Guid.NewGuid();
    var teaId = Guid.NewGuid();
    dbContext.Add(new Branch(branchId, businessId, "Principal", new FixedClock().UtcNow, isMain: true));
    dbContext.Add(Product(cafeId, businessId, "Cafe", "SKU-CAFE", minimumStock: 5));
    dbContext.Add(Product(teaId, businessId, "Te", "SKU-TE", minimumStock: 5));
    var cafeStock = new StockItem(Guid.NewGuid(), businessId, branchId, cafeId, new FixedClock().UtcNow);
    cafeStock.ApplyAdjustment(4, InventoryMovementReason.InitialStock, Guid.NewGuid(), false, new FixedClock().UtcNow);
    dbContext.Add(cafeStock);
    dbContext.Add(new StockItem(Guid.NewGuid(), businessId, branchId, teaId, new FixedClock().UtcNow));
    await dbContext.SaveChangesAsync();
    var repository = new EfInventoryReadRepository(dbContext);

    var outOfStock = await repository.ListAsync(
      businessId,
      new InventoryReadCriteria(null, null, "SKU-TE", false, true, 1, 10, StockSortOption.Quantity, InventorySortDirection.Desc));
    var count = await repository.CountAsync(
      businessId,
      new InventoryReadCriteria(null, null, "SKU", false, false, 1, 10, StockSortOption.Quantity, InventorySortDirection.Desc));

    outOfStock.Should().ContainSingle(item => item.ProductId == teaId && item.Status == "OutOfStock");
    count.Should().Be(2);
  }

  private static CreateProductHandler CreateProductHandler(
    AppDbContext dbContext,
    ICurrentUserService currentUser)
    => new(
      new EfCatalogProductRepository(dbContext),
      currentUser,
      new FixedClock(),
      new EfUnitOfWork(dbContext));

  private static CreateCategoryHandler CreateCategoryHandler(
    AppDbContext dbContext,
    ICurrentUserService currentUser)
    => new(
      new EfCatalogCategoryRepository(dbContext),
      currentUser,
      new FixedClock(),
      new EfUnitOfWork(dbContext));

  private static UpdateCategoryHandler UpdateCategoryHandler(
    AppDbContext dbContext,
    ICurrentUserService currentUser)
    => new(
      new EfCatalogCategoryRepository(dbContext),
      currentUser,
      new FixedClock(),
      new EfUnitOfWork(dbContext));

  private static AdjustInventoryHandler CreateInventoryHandler(
    AppDbContext dbContext,
    ICurrentUserService currentUser,
    ProductInventoryPolicy productPolicy)
    => new(
      new EfInventoryRepository(dbContext),
      new TestProductInventoryPolicyReader(productPolicy),
      currentUser,
      new NoopOutboxWriter(),
      new NoopAuditLogWriter(),
      new FixedClock(),
      new EfUnitOfWork(dbContext));

  private static CreateProductCommand CreateProductCommand(
    string name,
    string sku,
    string? barcode = null,
    string productType = "Simple",
    string unitOfMeasure = "Unit",
    bool trackInventory = true,
    decimal salePrice = 250)
    => new(
      productType,
      name,
      "Producto de prueba",
      sku,
      barcode,
      null,
      null,
      unitOfMeasure,
      salePrice,
      150,
      null,
      null,
      "Itbis18",
      18,
      true,
      true,
      trackInventory,
      1,
      100,
      5,
      false,
      null,
      null,
      null,
      null,
      null);

  private static ProductInventoryPolicy ProductInventoryPolicy(
    Guid productId,
    Guid businessId,
    string productType = "Simple",
    bool trackInventory = true,
    bool allowNegativeStock = false,
    string unitOfMeasure = "Unit",
    decimal? minimumStock = null)
    => new(
      productId,
      businessId,
      productType,
      trackInventory,
      allowNegativeStock,
      unitOfMeasure,
      minimumStock);

  private static Product Product(
    Guid productId,
    BusinessId businessId,
    string name,
    string sku,
    decimal? minimumStock)
    => new(
      new ProductCreationContext(productId, businessId, new FixedClock().UtcNow),
      new ProductIdentity(ProductType.Simple, name, null, null, null, UnitOfMeasure.Unit),
      new ProductCodes(sku, null, null, null),
      new ProductPricing(100, 50, null, null, TaxCategory.Itbis18, 18, true),
      new ProductInventorySettings(true, minimumStock, null, null, false),
      new ProductOptions(true, null, null, null));

  private static AppDbContext CreateDbContext()
  {
    var options = new DbContextOptionsBuilder<AppDbContext>()
      .UseInMemoryDatabase(Guid.NewGuid().ToString("D"))
      .Options;

    return new AppDbContext(options);
  }

  private sealed class FixedClock : IClock
  {
    public DateTimeOffset UtcNow { get; } =
      new(2026, 5, 15, 12, 0, 0, TimeSpan.Zero);
  }

  private sealed class NoopAuditLogWriter : IAuditLogWriter
  {
    public Task WriteAsync(AuditEntry entry, CancellationToken cancellationToken = default)
      => Task.CompletedTask;
  }

  private sealed class NoopOutboxWriter : IOutboxWriter
  {
    public Task AddAsync<TEvent>(
      TEvent integrationEvent,
      CancellationToken cancellationToken = default)
      where TEvent : class, IIntegrationEvent
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

  private sealed class TestProductInventoryPolicyReader(
    ProductInventoryPolicy productPolicy,
    decimal? minimumStock = 1)
    : IProductInventoryPolicyReader,
      IInventoryProductLookupReader
  {
    public Task<ProductInventoryPolicy?> GetAsync(
      Guid businessId,
      Guid productId,
      CancellationToken cancellationToken = default)
      => Task.FromResult(
        productPolicy.BusinessId == businessId && productPolicy.ProductId == productId
          ? productPolicy
          : null);

    public Task<IReadOnlyCollection<InventoryProductLookup>> SearchAsync(
      InventoryProductLookupQuery query,
      CancellationToken cancellationToken = default)
    {
      IReadOnlyCollection<InventoryProductLookup> products =
        productPolicy.BusinessId == query.BusinessId
          ? [new InventoryProductLookup(
              productPolicy.ProductId,
              productPolicy.BusinessId,
              "Producto de prueba",
              "SKU-TEST",
              null,
              productPolicy.ProductType,
              null,
              productPolicy.UnitOfMeasure,
              minimumStock,
              5)]
          : [];

      return Task.FromResult(products);
    }
  }

  private sealed record TestCurrentUser : ICurrentUserService
  {
    public Guid? UserId { get; init; }

    public Guid? BusinessId { get; init; }

    public Guid? BranchId { get; init; }

    public IReadOnlyCollection<string> Roles { get; init; } = [];

    public bool IsAuthenticated { get; init; }

    public static TestCurrentUser Create()
      => new()
      {
        UserId = Guid.NewGuid(),
        BusinessId = Guid.NewGuid(),
        BranchId = Guid.NewGuid(),
        Roles = ["Admin"],
        IsAuthenticated = true
      };
  }
}

#pragma warning restore CA1707
