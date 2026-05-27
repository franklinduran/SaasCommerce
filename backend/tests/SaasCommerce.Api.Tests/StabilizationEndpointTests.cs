#pragma warning disable CA1707

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using SaasCommerce.BuildingBlocks.Contracts.Common;
using SaasCommerce.Modules.Catalog.Contracts.Requests;
using SaasCommerce.Modules.Catalog.Contracts.Responses;
using SaasCommerce.Modules.Identity.Contracts.Requests;
using SaasCommerce.Modules.Identity.Contracts.Responses;
using SaasCommerce.Modules.Inventory.Contracts.Requests;
using SaasCommerce.Modules.Inventory.Contracts.Responses;

namespace SaasCommerce.Api.Tests;

public sealed class StabilizationEndpointTests
{
  [Fact]
  public async Task HealthLiveShouldReturn200WithLiveMessage()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();

    var response = await client.GetAsync("/health/live");
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<string>>();

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeTrue();
    payload.Data.Should().Be("Live");
    payload.Error.Should().BeNull();
  }

  [Fact]
  public async Task HealthReadyShouldReturnStandardHealthyResponse()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();

    var response = await client.GetAsync("/health/ready");
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>();

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeTrue();
    payload.Data.GetProperty("status").GetString().Should().Be("Healthy");
    payload.Data.GetProperty("postgreSql").GetProperty("status").GetString().Should().Be("Healthy");
    payload.Data.GetProperty("rabbitMq").GetProperty("status").GetString().Should().Be("Healthy");
    payload.Data.GetProperty("outbox").GetProperty("status").GetString().Should().Be("Healthy");
    payload.Error.Should().BeNull();
  }

  [Fact]
  public async Task HealthReadyShouldReturn503WhenRabbitMqUnavailable()
  {
    using var factory = new WebApplicationFactory<Program>()
      .WithWebHostBuilder(builder =>
      {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
          configuration.AddInMemoryCollection(new Dictionary<string, string?>
          {
            ["ConnectionStrings:DefaultConnection"] = "",
            ["Database:InMemoryName"] = Guid.NewGuid().ToString("D"),
            ["RabbitMq:UseInMemory"] = "false",
            ["RabbitMq:Host"] = "127.0.0.1",
            ["RabbitMq:Port"] = "65432",
            ["Jwt:Secret"] = "test-secret-with-at-least-32-characters",
            ["Jwt:Issuer"] = "SaasCommerce.Tests",
            ["Jwt:Audience"] = "SaasCommerce.Tests",
            ["Jwt:AccessTokenMinutes"] = "30"
          });
        });
      });
    using var client = factory.CreateClient();

    var response = await client.GetAsync("/health/ready");
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<JsonElement>>();

    response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeFalse();
    payload.Error!.Code.Should().Be("SERVICE_UNAVAILABLE");
  }

  [Fact]
  public async Task UnknownApiRouteShouldReturnStandardNotFoundResponse()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();

    var response = await client.GetAsync("/api/does-not-exist");
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();

    response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeFalse();
    payload.Data.Should().BeNull();
    payload.Error!.Code.Should().Be("NOT_FOUND");
    payload.CorrelationId.Should().NotBeNullOrWhiteSpace();
  }

  [Fact]
  public async Task InvalidLoginShouldReturnStandardUnauthorizedResponse()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();

    var response = await client.PostAsJsonAsync(
      "/api/auth/login",
      new LoginRequest("admin@test.com", "wrong-password"));
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeFalse();
    payload.Data.Should().BeNull();
    payload.Error!.Code.Should().Be("UNAUTHORIZED");
  }

  [Fact]
  public async Task InvalidRefreshTokenShouldReturnStandardUnauthorizedResponse()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();

    var response = await client.PostAsJsonAsync(
      "/api/auth/refresh",
      new RefreshTokenRequest("invalid-refresh-token"));
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeFalse();
    payload.Data.Should().BeNull();
    payload.Error!.Code.Should().Be("UNAUTHORIZED");
  }

  [Fact]
  public async Task DuplicateSkuShouldReturnProductConflictCode()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    await AuthenticateAsync(client);
    var sku = $"SKU-{Guid.NewGuid():N}";

    await CreateProductAsync(
      client,
      ProductRequest("Cafe", sku, $"BAR-{Guid.NewGuid():N}"));
    var response = await client.PostAsJsonAsync(
      "/api/catalog/products",
      ProductRequest("Cafe premium", sku.ToLowerInvariant(), $"BAR-{Guid.NewGuid():N}"));
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<ProductResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeFalse();
    payload.Data.Should().BeNull();
    payload.Error!.Code.Should().Be("PRODUCT_SKU_ALREADY_EXISTS");
  }

  [Fact]
  public async Task DuplicateBarcodeShouldReturnProductConflictCode()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    await AuthenticateAsync(client);
    var barcode = $"BAR-{Guid.NewGuid():N}";

    await CreateProductAsync(
      client,
      ProductRequest("Cafe", $"SKU-{Guid.NewGuid():N}", barcode));
    var response = await client.PostAsJsonAsync(
      "/api/catalog/products",
      ProductRequest("Cafe premium", $"SKU-{Guid.NewGuid():N}", barcode));
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<ProductResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeFalse();
    payload.Data.Should().BeNull();
    payload.Error!.Code.Should().Be("PRODUCT_BARCODE_ALREADY_EXISTS");
  }

  [Fact]
  public async Task InventoryAdjustmentForServiceShouldReturnTrackedProductCode()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    await AuthenticateAsync(client);
    var created = await CreateProductAsync(
      client,
      ProductRequest(
        "Delivery",
        $"SRV-{Guid.NewGuid():N}",
        null,
        productType: "Service",
        unitOfMeasure: "Service",
        trackInventory: false));

    var response = await client.PostAsJsonAsync(
      "/api/inventory/adjustments",
      new CreateInventoryAdjustmentRequest(created.Id, 1, "Adjustment"));
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<InventoryAdjustmentResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeFalse();
    payload.Data.Should().BeNull();
    payload.Error!.Code.Should().Be("INVENTORY_PRODUCT_NOT_TRACKED");
  }

  [Fact]
  public async Task InventoryNegativeStockShouldReturnInsufficientStockCode()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    await AuthenticateAsync(client);
    var created = await CreateProductAsync(
      client,
      ProductRequest("Cafe", $"SKU-{Guid.NewGuid():N}", null));

    var response = await client.PostAsJsonAsync(
      "/api/inventory/adjustments",
      new CreateInventoryAdjustmentRequest(created.Id, -1, "Adjustment"));
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<InventoryAdjustmentResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeFalse();
    payload.Data.Should().BeNull();
    payload.Error!.Code.Should().Be("INVENTORY_STOCK_INSUFFICIENT");
  }

  [Fact]
  public async Task GetInventory_ShouldReturnAdjustedStockRows()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    await AuthenticateAsync(client);
    var created = await CreateProductAsync(
      client,
      ProductRequest("Cafe", $"SKU-{Guid.NewGuid():N}", null));
    await client.PostAsJsonAsync(
      "/api/inventory/adjustments",
      new CreateInventoryAdjustmentRequest(created.Id, 1, "InitialStock"));

    var response = await client.GetAsync("/api/inventory?lowStockOnly=true");
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<InventoryListResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeTrue();
    payload.Data!.Items.Should().Contain(item => item.ProductId == created.Id && item.IsLowStock);
  }

  [Fact]
  public async Task GetInventory_ShouldFilterByProductAndBranch()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    var login = await AuthenticateAsync(client);
    var branchId = login.User.BranchId!.Value;
    var created = await CreateProductAsync(
      client,
      ProductRequest("Cafe filtrado", $"SKU-{Guid.NewGuid():N}", null));
    await client.PostAsJsonAsync(
      "/api/inventory/adjustments",
      new CreateInventoryAdjustmentRequest(created.Id, 7, "InitialStock", branchId));

    var response = await client.GetAsync($"/api/inventory?productId={created.Id}&branchId={branchId}&pageSize=50");
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<InventoryListResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeTrue();
    payload.Data!.Items.Should().ContainSingle(item =>
      item.ProductId == created.Id &&
      item.BranchId == branchId &&
      item.Quantity == 7);
  }

  [Fact]
  public async Task GetInventoryProductDetail_ShouldReturnStockAndMovements()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    await AuthenticateAsync(client);
    var created = await CreateProductAsync(
      client,
      ProductRequest("Cafe detalle", $"SKU-{Guid.NewGuid():N}", null));
    await client.PostAsJsonAsync(
      "/api/inventory/adjustments",
      new CreateInventoryAdjustmentRequest(created.Id, 3, "InitialStock"));

    var response = await client.GetAsync($"/api/inventory/products/{created.Id}");
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<InventoryProductDetailResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeTrue();
    payload.Data!.ProductId.Should().Be(created.Id);
    payload.Data.Branches.Should().Contain(branch => branch.CurrentStock == 3);
    payload.Data.RecentMovements.Should().Contain(movement => movement.NewStock == 3);
  }

  private static async Task<LoginResponse> AuthenticateAsync(HttpClient client)
  {
    var loginResponse = await client.PostAsJsonAsync(
      "/api/auth/login",
      new LoginRequest("admin@test.com", "Admin123!"));
    var login = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();

    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
      "Bearer",
      login!.Data!.AccessToken);

    return login.Data;
  }

  private static async Task<ProductResponse> CreateProductAsync(
    HttpClient client,
    CreateProductRequest request)
  {
    var response = await client.PostAsJsonAsync("/api/catalog/products", request);
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<ProductResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeTrue();
    payload.Data.Should().NotBeNull();

    return payload.Data!;
  }

  private static CreateProductRequest ProductRequest(
    string name,
    string sku,
    string? barcode,
    string productType = "Simple",
    string unitOfMeasure = "Unit",
    bool trackInventory = true)
    => new(
      productType,
      name,
      "Producto de prueba",
      sku,
      barcode,
      null,
      null,
      unitOfMeasure,
      250,
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
            ["Jwt:AccessTokenMinutes"] = "30"
          });
        });
      });
}

#pragma warning restore CA1707
