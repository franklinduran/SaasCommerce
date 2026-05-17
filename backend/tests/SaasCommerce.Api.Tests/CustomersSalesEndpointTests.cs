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
using SaasCommerce.Modules.Customers.Contracts.Requests;
using SaasCommerce.Modules.Customers.Contracts.Responses;
using SaasCommerce.Modules.Identity.Contracts.Requests;
using SaasCommerce.Modules.Identity.Contracts.Responses;
using SaasCommerce.Modules.Sales.Contracts.Requests;
using SaasCommerce.Modules.Sales.Contracts.Responses;
using SaasCommerce.Modules.Sales.Domain;

#pragma warning disable CA1707

namespace SaasCommerce.Api.Tests;

public sealed class CustomersSalesEndpointTests
{
  private static readonly Guid SeedBranchId = Guid.Parse("22222222-2222-2222-2222-222222222222");

  private static int phoneSequence = 1000000;

  [Fact]
  public async Task CustomersEndpointShouldRequireAuthentication()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();

    var response = await client.GetAsync("/api/customers");
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();

    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeFalse();
    payload.Data.Should().BeNull();
    payload.Error!.Code.Should().Be("UNAUTHORIZED");
  }

  [Fact]
  public async Task PostCustomers_ShouldReturnCreated_WhenRequestIsValid()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    await AuthenticateAsync(client);

    var response = await client.PostAsJsonAsync(
      "/api/customers",
      new CreateCustomerRequest("Maria Perez", "8095551234", "maria@example.com"));
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<CustomerResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.Created);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeTrue();
    payload.Data.Should().NotBeNull();
    payload.Data!.FullName.Should().Be("Maria Perez");
    payload.Data.Email.Should().Be("maria@example.com");
    payload.Error.Should().BeNull();
  }

  [Fact]
  public async Task PostCustomers_ShouldReturnBadRequest_WhenNameIsEmpty()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    await AuthenticateAsync(client);

    var response = await client.PostAsJsonAsync(
      "/api/customers",
      new CreateCustomerRequest("", "8095551234", null));
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<CustomerResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeFalse();
    payload.Data.Should().BeNull();
    payload.Error!.Code.Should().Be("VALIDATION_ERROR");
  }

  [Fact]
  public async Task GetCustomers_ShouldReturnOnlyCurrentBusinessCustomers()
  {
    using var factory = CreateFactory();
    using var adminClient = factory.CreateClient();
    await AuthenticateAsync(adminClient);
    var adminCustomer = await CreateCustomerAsync(adminClient, "Cliente negocio principal");

    var secondTenant = await RegisterBusinessAsync(factory, "clientes");
    await CreateCustomerAsync(secondTenant.Client, "Cliente otro negocio");

    var response = await adminClient.GetAsync("/api/customers?pageSize=50");
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<CustomerListResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeTrue();
    payload.Data!.Items.Should().Contain(customer => customer.Id == adminCustomer.Id);
    payload.Data.Items.Should().OnlyContain(customer => customer.BusinessId != secondTenant.BusinessId);
  }

  [Fact]
  public async Task GetCustomer_ShouldReturnNotFound_WhenCustomerBelongsToAnotherBusiness()
  {
    using var factory = CreateFactory();
    using var adminClient = factory.CreateClient();
    await AuthenticateAsync(adminClient);

    var secondTenant = await RegisterBusinessAsync(factory, "customer-foreign");
    var foreignCustomer = await CreateCustomerAsync(secondTenant.Client, "Cliente aislado");

    var response = await adminClient.GetAsync($"/api/customers/{foreignCustomer.Id}");
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<CustomerResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeFalse();
    payload.Data.Should().BeNull();
    payload.Error!.Code.Should().Be("NOT_FOUND");
  }

  [Fact]
  public async Task PutCustomer_ShouldUpdateCustomer_WhenValid()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    await AuthenticateAsync(client);
    var customer = await CreateCustomerAsync(client, "Cliente antiguo");

    var response = await client.PutAsJsonAsync(
      $"/api/customers/{customer.Id}",
      new UpdateCustomerRequest("Cliente actualizado", "8095550000", "nuevo@example.com", true));
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<CustomerResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeTrue();
    payload.Data!.FullName.Should().Be("Cliente actualizado");
    payload.Data.Phone.Should().Be("8095550000");
  }

  [Fact]
  public async Task DeleteCustomer_ShouldDeactivateCustomer_WhenValid()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    await AuthenticateAsync(client);
    var customer = await CreateCustomerAsync(client, "Cliente a desactivar");

    var response = await client.DeleteAsync($"/api/customers/{customer.Id}");
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<CustomerResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeTrue();
    payload.Data!.IsActive.Should().BeFalse();
    payload.Data.DeactivatedAt.Should().NotBeNull();
  }

  [Fact]
  public async Task SalesEndpointShouldRequireAuthentication()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();

    var response = await client.GetAsync("/api/sales");
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();

    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeFalse();
    payload.Data.Should().BeNull();
    payload.Error!.Code.Should().Be("UNAUTHORIZED");
  }

  [Fact]
  public async Task PostSales_ShouldReturnCreated_WhenRequestIsValid()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    await AuthenticateAsync(client);
    var product = await CreateProductAsync(client, "Aceite 16 oz", 125);

    var response = await client.PostAsJsonAsync(
      "/api/sales",
      SaleRequest(product.Id, quantity: 2));
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<SaleResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.Created);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeTrue();
    payload.Data!.Status.Should().Be("Received");
    payload.Data.Total.Should().Be(250);
    payload.Data.Items.Should().ContainSingle().Which.UnitPrice.Should().Be(125);
    payload.Error.Should().BeNull();
  }

  [Fact]
  public async Task PostSales_ShouldCreateOutboxMessage_WhenRequestIsValid()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    await AuthenticateAsync(client);
    var product = await CreateProductAsync(client, "Arroz selecto 5 lb", 250);

    var response = await client.PostAsJsonAsync(
      "/api/sales",
      SaleRequest(product.Id, quantity: 1));
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<SaleResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.Created);
    payload!.Data.Should().NotBeNull();

    using var scope = factory.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var outbox = await dbContext.OutboxMessages.SingleAsync(
      message => message.BusinessId == payload.Data!.BusinessId &&
        message.Payload.Contains(payload.Data.SaleId.ToString(), StringComparison.Ordinal),
      CancellationToken.None);

    outbox.EventType.Should().Contain("SaleCreatedEventV1");
    outbox.Payload.Should().Contain("\"unitPrice\":250");
  }

  [Fact]
  public async Task PostSales_ShouldReturnBadRequest_WhenItemsAreEmpty()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    await AuthenticateAsync(client);

    var response = await client.PostAsJsonAsync(
      "/api/sales",
      new CreateSaleRequest(SeedBranchId, null, "Cash", []));
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<SaleResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeFalse();
    payload.Data.Should().BeNull();
    payload.Error!.Code.Should().Be("VALIDATION_ERROR");
  }

  [Fact]
  public async Task PostSales_ShouldReturnBadRequest_WhenQuantityIsInvalid()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    await AuthenticateAsync(client);
    var product = await CreateProductAsync(client, "Pan sobao", 20);

    var response = await client.PostAsJsonAsync(
      "/api/sales",
      SaleRequest(product.Id, quantity: 0));
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<SaleResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeFalse();
    payload.Error!.Code.Should().Be("VALIDATION_ERROR");
  }

  [Fact]
  public async Task PostSales_ShouldReturnNotFound_WhenProductDoesNotExist()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    await AuthenticateAsync(client);

    var response = await client.PostAsJsonAsync(
      "/api/sales",
      SaleRequest(Guid.NewGuid(), quantity: 1));
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<SaleResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeFalse();
    payload.Data.Should().BeNull();
    payload.Error!.Code.Should().Be("PRODUCT_NOT_FOUND");
  }

  [Fact]
  public async Task PostSales_ShouldReturnNotFound_WhenProductBelongsToAnotherBusiness()
  {
    using var factory = CreateFactory();
    using var adminClient = factory.CreateClient();
    await AuthenticateAsync(adminClient);

    var secondTenant = await RegisterBusinessAsync(factory, "sale-foreign-product");
    var foreignProduct = await CreateProductAsync(secondTenant.Client, "Producto otro negocio", 50);

    var response = await adminClient.PostAsJsonAsync(
      "/api/sales",
      SaleRequest(foreignProduct.Id, quantity: 1));
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<SaleResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeFalse();
    payload.Error!.Code.Should().Be("PRODUCT_NOT_FOUND");
  }

  [Fact]
  public async Task GetSales_ShouldReturnOnlyCurrentBusinessSales()
  {
    using var factory = CreateFactory();
    using var adminClient = factory.CreateClient();
    await AuthenticateAsync(adminClient);
    var adminProduct = await CreateProductAsync(adminClient, "Agua 16 oz", 15);
    var adminSale = await CreateSaleAsync(adminClient, adminProduct.Id);

    var secondTenant = await RegisterBusinessAsync(factory, "sales-list");
    var secondProduct = await CreateProductAsync(secondTenant.Client, "Venta otro negocio", 30);
    await CreateSaleAsync(secondTenant.Client, secondProduct.Id, secondTenant.BranchId);

    var response = await adminClient.GetAsync("/api/sales?pageSize=50");
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<SaleListResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeTrue();
    payload.Data!.Items.Should().Contain(sale => sale.SaleId == adminSale.SaleId);
    payload.Data.Items.Should().OnlyContain(sale => sale.BusinessId == adminSale.BusinessId);
  }

  [Fact]
  public async Task GetSaleById_ShouldReturnNotFound_WhenSaleBelongsToAnotherBusiness()
  {
    using var factory = CreateFactory();
    using var adminClient = factory.CreateClient();
    await AuthenticateAsync(adminClient);

    var secondTenant = await RegisterBusinessAsync(factory, "sale-foreign");
    var secondProduct = await CreateProductAsync(secondTenant.Client, "Venta aislada", 30);
    var foreignSale = await CreateSaleAsync(secondTenant.Client, secondProduct.Id, secondTenant.BranchId);

    var response = await adminClient.GetAsync($"/api/sales/{foreignSale.SaleId}");
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<SaleResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeFalse();
    payload.Data.Should().BeNull();
    payload.Error!.Code.Should().Be("NOT_FOUND");
  }

  [Fact]
  public async Task CancelSale_ShouldReturnOk_WhenSaleCanBeCancelled()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    await AuthenticateAsync(client);
    var product = await CreateProductAsync(client, "Leche evaporada", 55);
    var sale = await CreateSaleAsync(client, product.Id);

    var response = await client.PostAsJsonAsync(
      $"/api/sales/{sale.SaleId}/cancel",
      new CancelSaleRequest("Cliente cambio la compra"));
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<SaleResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeTrue();
    payload.Data!.Status.Should().Be("Cancelled");
    payload.Data.CancellationReason.Should().Be("Cliente cambio la compra");
  }

  [Fact]
  public async Task CancelSale_ShouldReturnBadRequest_WhenSaleIsCompleted()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    await AuthenticateAsync(client);
    var product = await CreateProductAsync(client, "Cafe molido", 180);
    var sale = await CreateSaleAsync(client, product.Id);
    await CompleteSaleAsync(factory, sale.SaleId);

    var response = await client.PostAsJsonAsync(
      $"/api/sales/{sale.SaleId}/cancel",
      new CancelSaleRequest("No debe cancelar"));
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<SaleResponse>>();

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
    var email = $"{prefix}-{Guid.NewGuid():N}@example.com";
    var response = await client.PostAsJsonAsync(
      "/api/account/register-business",
      new RegisterBusinessRequest(
        $"Colmado {prefix}",
        "Duenio Local",
        email,
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

  private static async Task<CustomerResponse> CreateCustomerAsync(HttpClient client, string name)
  {
    var response = await client.PostAsJsonAsync(
      "/api/customers",
      new CreateCustomerRequest(name, NextPhoneNumber(), $"{Guid.NewGuid():N}@example.com"));
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<CustomerResponse>>();

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

  private static async Task<SaleResponse> CreateSaleAsync(
    HttpClient client,
    Guid productId,
    Guid? branchId = null)
  {
    var response = await client.PostAsJsonAsync(
      "/api/sales",
      SaleRequest(productId, quantity: 1, branchId));
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<SaleResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.Created);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeTrue();
    payload.Data.Should().NotBeNull();

    return payload.Data!;
  }

  private static async Task CompleteSaleAsync(WebApplicationFactory<Program> factory, Guid saleId)
  {
    using var scope = factory.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var sale = await dbContext.Set<Sale>().SingleAsync(
      candidate => candidate.Id == saleId,
      CancellationToken.None);

    var now = DateTimeOffset.UtcNow;
    sale.MarkAsProcessing(now);
    sale.Complete(now.AddSeconds(1));
    await dbContext.SaveChangesAsync(CancellationToken.None);
  }

  private static CreateSaleRequest SaleRequest(
    Guid productId,
    decimal quantity,
    Guid? branchId = null)
    => new(
      branchId ?? SeedBranchId,
      null,
      "Cash",
      [new CreateSaleItemRequest(productId, quantity)]);

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
    => $"829{Interlocked.Increment(ref phoneSequence):D7}";

  private static string NextIdentificationNumber()
    => $"1{Interlocked.Increment(ref phoneSequence):D8}";

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
