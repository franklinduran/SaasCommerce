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
using SaasCommerce.Modules.Inventory.Domain;
using SaasCommerce.Modules.Purchasing.Contracts.Requests;
using SaasCommerce.Modules.Purchasing.Contracts.Responses;

#pragma warning disable CA1707

namespace SaasCommerce.Api.Tests;

public sealed class PurchasingEndpointTests
{
  private static readonly Guid SeedBranchId = Guid.Parse("22222222-2222-2222-2222-222222222222");
  private static int sequence = 2000000;

  [Fact]
  public async Task PostSuppliers_ShouldRequireAuthentication()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();

    var response = await client.PostAsJsonAsync(
      "/api/suppliers",
      new CreateSupplierRequest("Proveedor sin auth", null, null, null, null));
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();

    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeFalse();
    payload.Error!.Code.Should().Be("UNAUTHORIZED");
  }

  [Fact]
  public async Task PostPurchases_ShouldCreatePurchase_WhenRequestIsValid()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    await AuthenticateAsync(client);
    var supplier = await CreateSupplierAsync(client, "Distribuidora Cafe");
    var product = await CreateProductAsync(client, "Cafe molido compra", 250);

    var response = await client.PostAsJsonAsync(
      "/api/purchases",
      PurchaseRequest(supplier.Id, product.Id, quantity: 2, unitCost: 120));
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<PurchaseResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.Created);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeTrue();
    payload.Data.Should().NotBeNull();
    payload.Data!.Status.Should().Be("Received");
    payload.Data.Total.Should().Be(240);
    payload.Data.Items.Should().ContainSingle().Which.ProductId.Should().Be(product.Id);
  }

  [Fact]
  public async Task ReceivePurchase_ShouldQueueInventoryProcessing_WhenPurchaseIsDraft()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    await AuthenticateAsync(client);
    var supplier = await CreateSupplierAsync(client, "Distribuidora Stock");
    var product = await CreateProductAsync(client, "Arroz compra", 175);
    var purchase = await CreatePurchaseAsync(
      client,
      supplier.Id,
      product.Id,
      quantity: 3,
      unitCost: 90,
      receiveNow: false);

    var response = await client.PostAsync($"/api/purchases/{purchase.PurchaseId}/receive", null);
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<PurchaseResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeTrue();
    payload.Data!.Status.Should().Be("Received");

    using var scope = factory.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Set<StockItem>()
      .Should()
      .NotContain(item => item.ProductId == product.Id && item.BranchId.Value == SeedBranchId);
    dbContext.Set<InventoryMovement>()
      .Should()
      .NotContain(item => item.ProductId == product.Id && item.PurchaseId == purchase.PurchaseId);
  }

  [Fact]
  public async Task GetPurchases_ShouldNotExposeAnotherBusiness()
  {
    using var factory = CreateFactory();
    using var adminClient = factory.CreateClient();
    await AuthenticateAsync(adminClient);
    var adminSupplier = await CreateSupplierAsync(adminClient, "Proveedor principal");
    var adminProduct = await CreateProductAsync(adminClient, "Producto principal", 100);
    var adminPurchase = await CreatePurchaseAsync(adminClient, adminSupplier.Id, adminProduct.Id, 1, 50);

    var secondTenant = await RegisterBusinessAsync(factory, "purchase-foreign");
    var foreignSupplier = await CreateSupplierAsync(secondTenant.Client, "Proveedor externo");
    var foreignProduct = await CreateProductAsync(secondTenant.Client, "Producto externo", 100);
    await CreatePurchaseAsync(secondTenant.Client, foreignSupplier.Id, foreignProduct.Id, 1, 45);

    var response = await adminClient.GetAsync("/api/purchases?pageSize=50");
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<PurchaseListResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeTrue();
    payload.Data!.Items.Should().Contain(item => item.PurchaseId == adminPurchase.PurchaseId);
    payload.Data.Items.Should().OnlyContain(item => item.BusinessId == adminPurchase.BusinessId);
  }

  [Fact]
  public async Task PostPurchases_ShouldReturnApiResponse_WhenValidationFails()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    await AuthenticateAsync(client);

    var response = await client.PostAsJsonAsync(
      "/api/purchases",
      new CreatePurchaseRequest(
        Guid.Empty,
        SeedBranchId,
        [],
        null,
        DateTimeOffset.UtcNow,
        null,
        ReceiveNow: true));
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<PurchaseResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeFalse();
    payload.Data.Should().BeNull();
    payload.Error!.Code.Should().Be("VALIDATION_ERROR");
  }

  private static async Task AuthenticateAsync(HttpClient client)
  {
    var loginResponse = await client.PostAsJsonAsync(
      "/api/auth/login",
      new LoginRequest("admin@test.com", "Admin123!"));
    var login = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();

    loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    login!.Data.Should().NotBeNull();

    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
      "Bearer",
      login.Data!.AccessToken);
  }

  private static async Task<RegisteredTenant> RegisterBusinessAsync(
    WebApplicationFactory<Program> factory,
    string prefix)
  {
    var client = factory.CreateClient();
    var response = await client.PostAsJsonAsync(
      "/api/account/register-business",
      new RegisterBusinessRequest(
        $"Colmado {prefix}",
        "Duenio Local",
        $"{prefix}-{Guid.NewGuid():N}@example.com",
        "Admin123!",
        "Rnc",
        NextIdentificationNumber(),
        [new RegisterBusinessPhoneRequest(NextPhoneNumber(), "Principal", true)],
        "Principal"));
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<RegisterBusinessResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeTrue();
    payload.Data.Should().NotBeNull();

    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
      "Bearer",
      payload.Data!.AccessToken);

    return new RegisteredTenant(client, payload.Data.BusinessId, payload.Data.BranchId);
  }

  private static async Task<SupplierResponse> CreateSupplierAsync(HttpClient client, string name)
  {
    var response = await client.PostAsJsonAsync(
      "/api/suppliers",
      new CreateSupplierRequest(
        name,
        NextIdentificationNumber(),
        NextPhoneNumber(),
        $"{Guid.NewGuid():N}@supplier.test",
        "Santiago"));
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<SupplierResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.Created);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeTrue();
    payload.Data.Should().NotBeNull();

    return payload.Data!;
  }

  private static async Task<ProductResponse> CreateProductAsync(
    HttpClient client,
    string name,
    decimal salePrice)
  {
    var response = await client.PostAsJsonAsync(
      "/api/catalog/products",
      ProductRequest(name, salePrice));
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<ProductResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeTrue();
    payload.Data.Should().NotBeNull();

    return payload.Data!;
  }

  private static async Task<PurchaseResponse> CreatePurchaseAsync(
    HttpClient client,
    Guid supplierId,
    Guid productId,
    decimal quantity,
    decimal unitCost,
    bool receiveNow = true)
  {
    var response = await client.PostAsJsonAsync(
      "/api/purchases",
      PurchaseRequest(supplierId, productId, quantity, unitCost, receiveNow));
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<PurchaseResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.Created);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeTrue();
    payload.Data.Should().NotBeNull();

    return payload.Data!;
  }

  private static CreatePurchaseRequest PurchaseRequest(
    Guid supplierId,
    Guid productId,
    decimal quantity,
    decimal unitCost,
    bool receiveNow = true)
    => new(
      supplierId,
      SeedBranchId,
      [new CreatePurchaseItemRequest(productId, quantity, unitCost)],
      $"FAC-{Interlocked.Increment(ref sequence)}",
      DateTimeOffset.UtcNow,
      "Compra de prueba",
      receiveNow);

  private static CreateProductRequest ProductRequest(string name, decimal salePrice)
    => new(
      "Simple",
      name,
      "Producto de prueba",
      $"SKU-{Guid.NewGuid():N}",
      $"BAR-{Guid.NewGuid():N}",
      null,
      null,
      "Unit",
      salePrice,
      salePrice / 2,
      null,
      null,
      "Itbis18",
      18,
      true,
      true,
      true,
      1,
      100,
      5,
      false,
      null,
      null,
      null,
      null,
      null);

  private static string NextPhoneNumber()
    => $"829{Interlocked.Increment(ref sequence):D7}";

  private static string NextIdentificationNumber()
    => $"1{Interlocked.Increment(ref sequence):D8}";

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

  private sealed record RegisteredTenant(HttpClient Client, Guid BusinessId, Guid BranchId);
}

#pragma warning restore CA1707
