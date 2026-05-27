#pragma warning disable CA1707

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SaasCommerce.BuildingBlocks.Contracts.Common;
using SaasCommerce.BuildingBlocks.Infrastructure.Persistence;
using SaasCommerce.Modules.Catalog.Contracts.Requests;
using SaasCommerce.Modules.Catalog.Contracts.Responses;
using SaasCommerce.Modules.Identity.Contracts.Requests;
using SaasCommerce.Modules.Identity.Contracts.Responses;
using SaasCommerce.Modules.Inventory.Contracts.Events.V1;
using SaasCommerce.Modules.Inventory.Contracts.Responses;
using SaasCommerce.Modules.Inventory.Domain;
using SaasCommerce.Modules.Purchasing.Application.Purchases;
using SaasCommerce.Modules.Purchasing.Contracts.Events.V1;
using SaasCommerce.Modules.Purchasing.Contracts.Requests;
using SaasCommerce.Modules.Purchasing.Contracts.Responses;
using SaasCommerce.Modules.Sales.Application.Sales;
using SaasCommerce.Modules.Sales.Contracts.Events.V1;
using SaasCommerce.Modules.Sales.Contracts.Requests;
using SaasCommerce.Modules.Sales.Contracts.Responses;

namespace SaasCommerce.Api.Tests;

/// <summary>
/// E2E MVP smoke tests. Covers HU-37.2 (POS sale → inventory deduction idempotency)
/// and HU-37.3 (purchase → inventory increase idempotency).
/// All tests run against the full seeded Development environment using in-memory EF Core.
/// </summary>
public sealed class MvpSmokeTests
{
  private static readonly Guid SeedBusinessId = Guid.Parse("11111111-1111-1111-1111-111111111111");
  private static readonly Guid SeedBranchId = Guid.Parse("22222222-2222-2222-2222-222222222222");


  // ── HU-37.0: basic connectivity ─────────────────────────────────────────

  [Fact]
  public async Task Smoke_Login_ShouldSucceed_WithDemoCredentials()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();

    var response = await client.PostAsJsonAsync(
      "/api/auth/login",
      new LoginRequest("admin@test.com", "Admin123!"));
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeTrue();
    payload.Data!.AccessToken.Should().NotBeNullOrWhiteSpace();
    payload.Data.User.Email.Should().Be("admin@test.com");
  }

  // ── HU-37.1: seeded catalog ──────────────────────────────────────────────

  [Fact]
  public async Task Smoke_Catalog_ShouldContainAllSeededProducts()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    await AuthenticateAsync(client);

    var response = await client.GetAsync("/api/catalog/products?pageSize=25");
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<ProductListResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeTrue();
    payload.Data!.Items.Should().HaveCountGreaterThanOrEqualTo(20);
    payload.Data.Items.Should().Contain(p => p.Name.Contains("Café", StringComparison.OrdinalIgnoreCase));
  }

  [Fact]
  public async Task Smoke_Inventory_ShouldHaveSeededStockForAllProducts()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    await AuthenticateAsync(client);

    var response = await client.GetAsync("/api/inventory/stock?pageSize=25");
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<StockListResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeTrue();
    payload.Data!.Items.Should().HaveCountGreaterThanOrEqualTo(19,
      "all non-weighed products should have initial stock");
    payload.Data.Items.Should().AllSatisfy(item =>
      item.Quantity.Should().BeGreaterThan(0));
  }

  // ── HU-37.2: POS sale → inventory deduction idempotency ─────────────────

  [Fact]
  public async Task Smoke_PosSale_ShouldCreateSale_WithSeededProduct()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    await AuthenticateAsync(client);

    var product = await FindSeededProductAsync(client, "Café Santo Domingo");

    var response = await client.PostAsJsonAsync(
      "/api/sales",
      new CreateSaleRequest(
        SeedBranchId,
        null,
        "Cash",
        [new CreateSaleItemRequest(product.Id, 1)]));
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<SaleResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.Created);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeTrue();
    payload.Data!.Status.Should().Be("Received");
    payload.Data.Total.Should().Be(product.SalePrice);
    payload.Data.BusinessId.Should().Be(SeedBusinessId);

    using var scope = factory.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var outboxExists = await dbContext.OutboxMessages
      .AnyAsync(m => m.BusinessId == SeedBusinessId &&
                     m.Payload.Contains(payload.Data.SaleId.ToString(), StringComparison.Ordinal));
    outboxExists.Should().BeTrue("a SaleCreatedEventV1 outbox message must be queued for the Worker");
  }

  /// <summary>
  /// HU-37.2: Simulates the Worker calling <see cref="IDeductSaleInventoryUseCase"/> twice
  /// with the same sale event and verifies inventory is only deducted once.
  /// </summary>
  [Fact]
  public async Task Smoke_PosSale_InventoryDeduction_ShouldBeIdempotent()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    var login = await AuthenticateAsync(client);

    var product = await FindSeededProductAsync(client, "Arroz El Gallo");
    var initialStock = await GetProductStockAsync(client, product.Id);

    var saleResponse = await client.PostAsJsonAsync(
      "/api/sales",
      new CreateSaleRequest(
        SeedBranchId,
        null,
        "Cash",
        [new CreateSaleItemRequest(product.Id, 5)]));
    var sale = (await saleResponse.Content.ReadFromJsonAsync<ApiResponse<SaleResponse>>())!.Data!;

    var deductionEvent = new InventoryDeductionRequestedEventV1(
      Guid.NewGuid(),
      Guid.NewGuid(),
      sale.SaleId,
      sale.BusinessId,
      sale.BranchId,
      login.User.Id,
      sale.Items
        .Select(item => new SaleItemV1(item.ProductId, item.Quantity, item.UnitPrice))
        .ToArray(),
      sale.Total,
      sale.PaymentMethod,
      DateTimeOffset.UtcNow);

    using var scope = factory.Services.CreateScope();
    var useCase = scope.ServiceProvider.GetRequiredService<IDeductSaleInventoryUseCase>();

    // First call: Worker processes inventory deduction
    var firstResult = await useCase.ExecuteAsync(deductionEvent);
    firstResult.IsSuccess.Should().BeTrue();

    // Second call: duplicate event — must be a no-op
    var secondResult = await useCase.ExecuteAsync(deductionEvent);
    secondResult.IsSuccess.Should().BeTrue();

    var stockAfter = await GetProductStockAsync(client, product.Id);
    stockAfter.Should().Be(
      initialStock - 5,
      "inventory must be deducted exactly once, even if the Worker retries the event");
  }

  // ── HU-37.3: purchase → inventory increase idempotency ──────────────────

  [Fact]
  public async Task Smoke_Purchase_ShouldSucceed_WithSeededSupplier()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    await AuthenticateAsync(client);

    var supplier = await FindSeededSupplierAsync(client);
    var product = await FindSeededProductAsync(client, "Azúcar Morena");

    // Create a draft purchase order
    var createResponse = await client.PostAsJsonAsync(
      "/api/purchases",
      new CreatePurchaseRequest(
        supplier.Id,
        SeedBranchId,
        [new CreatePurchaseItemRequest(product.Id, 10, 40m)],
        null,
        null,
        null,
        ReceiveNow: false));
    var created = await createResponse.Content.ReadFromJsonAsync<ApiResponse<PurchaseResponse>>();

    createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
    created.Should().NotBeNull();
    created!.IsSuccess.Should().BeTrue();
    created.Data!.Status.Should().Be("Draft");

    // Receive the purchase
    var receiveResponse = await client.PostAsync($"/api/purchases/{created.Data.PurchaseId}/receive", null);
    var received = await receiveResponse.Content.ReadFromJsonAsync<ApiResponse<PurchaseResponse>>();

    receiveResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    received!.Data!.Status.Should().Be("Received");

    // Verify outbox has the PurchaseReceivedEventV1 for the Worker to process
    using var scope = factory.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var outboxExists = await dbContext.OutboxMessages.AnyAsync(
      m => m.BusinessId == SeedBusinessId &&
           m.Payload.Contains(created.Data.PurchaseId.ToString(), StringComparison.Ordinal));
    outboxExists.Should().BeTrue("a PurchaseReceivedEventV1 outbox message must be queued for the Worker");
  }

  /// <summary>
  /// HU-37.3: Simulates the Worker calling <see cref="IProcessPurchaseReceivedEventUseCase"/> twice
  /// with the same purchase event and verifies inventory is only increased once.
  /// </summary>
  [Fact]
  public async Task Smoke_Purchase_InventoryIncrease_ShouldBeIdempotent()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    var login = await AuthenticateAsync(client);

    var supplier = await FindSeededSupplierAsync(client);
    var product = await FindSeededProductAsync(client, "Habichuelas Rojas");
    var initialStock = await GetProductStockAsync(client, product.Id);

    // Create and receive a draft purchase (Worker is NOT running — stock not yet updated)
    var purchaseResponse = await client.PostAsJsonAsync(
      "/api/purchases",
      new CreatePurchaseRequest(
        supplier.Id,
        SeedBranchId,
        [new CreatePurchaseItemRequest(product.Id, 8, 32m)],
        null,
        null,
        null,
        ReceiveNow: false));
    var purchase = (await purchaseResponse.Content.ReadFromJsonAsync<ApiResponse<PurchaseResponse>>())!.Data!;

    await client.PostAsync($"/api/purchases/{purchase.PurchaseId}/receive", null);

    // Construct the PurchaseReceivedEventV1 that the Worker would receive from the Outbox
    var receivedEvent = new PurchaseReceivedEventV1(
      Guid.NewGuid(),
      Guid.NewGuid(),
      purchase.PurchaseId,
      purchase.BusinessId,
      purchase.BranchId,
      purchase.SupplierId,
      login.User.Id,
      purchase.Items
        .Select(i => new PurchaseItemV1(i.ProductId, i.Quantity, i.UnitCost, i.Subtotal))
        .ToArray(),
      purchase.Total,
      DateTimeOffset.UtcNow);

    using var scope = factory.Services.CreateScope();
    var useCase = scope.ServiceProvider.GetRequiredService<IProcessPurchaseReceivedEventUseCase>();

    // First call: Worker processes inventory increase
    var firstResult = await useCase.ExecuteAsync(receivedEvent);
    firstResult.IsSuccess.Should().BeTrue();

    // Second call: duplicate event — must be a no-op
    var secondResult = await useCase.ExecuteAsync(receivedEvent);
    secondResult.IsSuccess.Should().BeTrue();

    var stockAfter = await GetProductStockAsync(client, product.Id);
    stockAfter.Should().Be(
      initialStock + 8,
      "inventory must be increased exactly once, even if the Worker retries the event");
  }

  // ── Helpers ──────────────────────────────────────────────────────────────

  private static async Task<LoginResponse> AuthenticateAsync(HttpClient client)
  {
    var response = await client.PostAsJsonAsync(
      "/api/auth/login",
      new LoginRequest("admin@test.com", "Admin123!"));
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    payload!.Data.Should().NotBeNull();

    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
      "Bearer",
      payload.Data!.AccessToken);

    return payload.Data;
  }

  private static async Task<ProductResponse> FindSeededProductAsync(HttpClient client, string nameFragment)
  {
    var response = await client.GetAsync(
      $"/api/catalog/products?query={Uri.EscapeDataString(nameFragment)}&pageSize=10");
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<ProductListResponse>>();

    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeTrue();
    payload.Data!.Items.Should().NotBeEmpty(
      $"seeded product matching '{nameFragment}' must exist in catalog");

    return payload.Data.Items.First();
  }

  private static async Task<SupplierResponse> FindSeededSupplierAsync(HttpClient client)
  {
    var response = await client.GetAsync("/api/suppliers?pageSize=10");
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<SupplierListResponse>>();

    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeTrue();
    payload.Data!.Items.Should().NotBeEmpty("seeded suppliers must exist");

    return payload.Data.Items.First();
  }

  private static async Task<decimal> GetProductStockAsync(HttpClient client, Guid productId)
  {
    var response = await client.GetAsync(
      $"/api/inventory/stock?productId={productId}&pageSize=10");
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<StockListResponse>>();

    return payload?.Data?.Items?
      .Where(i => i.ProductId == productId)
      .Sum(i => i.Quantity) ?? 0m;
  }

  private static WebApplicationFactory<Program> CreateFactory()
    => new WebApplicationFactory<Program>()
      .WithWebHostBuilder(builder =>
      {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
          configuration.AddInMemoryCollection(new Dictionary<string, string?>
          {
            ["ConnectionStrings:DefaultConnection"] = "",
            ["Database:InMemoryName"] = Guid.NewGuid().ToString("D"),
            ["RabbitMq:UseInMemory"] = "true",
            ["Jwt:Secret"] = "test-secret-with-at-least-32-characters",
            ["Jwt:Issuer"] = "SaasCommerce.Tests",
            ["Jwt:Audience"] = "SaasCommerce.Tests",
            ["Jwt:AccessTokenMinutes"] = "30",
            ["SeedDemoData:Enabled"] = "true"
          });
        });
      });
}

#pragma warning restore CA1707
