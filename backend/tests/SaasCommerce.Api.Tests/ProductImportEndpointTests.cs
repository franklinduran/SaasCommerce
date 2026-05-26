using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using SaasCommerce.BuildingBlocks.Contracts.Common;
using SaasCommerce.Modules.Catalog.Contracts.Responses;
using SaasCommerce.Modules.Identity.Contracts.Requests;
using SaasCommerce.Modules.Identity.Contracts.Responses;

namespace SaasCommerce.Api.Tests;

public sealed class ProductImportEndpointTests
{
  [Fact]
  public async Task GetImportTemplateShouldReturn401WhenNotAuthenticated()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();

    var response = await client.GetAsync("/api/products/import/template");

    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task GetImportTemplateShouldReturn200WithCsvContentWhenAuthenticated()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    await AuthenticateAsync(client);

    var response = await client.GetAsync("/api/products/import/template");

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    response.Content.Headers.ContentType!.MediaType.Should().Be("text/csv");
    var body = await response.Content.ReadAsStringAsync();
    body.Should().Contain("Name,Sku,CategoryName,SalePrice,CostPrice,StockQuantity");
  }

  [Fact]
  public async Task PostImportProductsShouldReturn401WhenNotAuthenticated()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();

    using var content = BuildCsvFile(ValidCsv());
    var response = await client.PostAsync("/api/products/import", content);

    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task PostImportProductsShouldReturn201WhenCsvIsValid()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    await AuthenticateAsync(client);

    var csv = """
      Name,Sku,CategoryName,SalePrice,CostPrice,StockQuantity
      Producto Test Uno,IMP-TEST-001,Víveres,175.00,140.00,100
      Producto Test Dos,IMP-TEST-002,Bebidas,65.00,47.00,120
      """;

    using var content = BuildCsvFile(csv);
    var response = await client.PostAsync("/api/products/import", content);

    response.StatusCode.Should().Be(HttpStatusCode.Created);
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<ImportProductsResponse>>();
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeTrue();
    payload.Data!.ImportedCount.Should().Be(2);
    payload.Data.SkippedCount.Should().Be(0);
  }

  [Fact]
  public async Task PostImportProductsShouldReturn400WhenFileIsEmpty()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    await AuthenticateAsync(client);

    using var content = BuildCsvFile(string.Empty);
    var response = await client.PostAsync("/api/products/import", content);

    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
    payload!.Error!.Code.Should().Be("IMPORT_EMPTY_FILE");
  }

  [Fact]
  public async Task PostImportProductsShouldReturn201WithSkippedRowsWhenSomeRowsAreInvalid()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    await AuthenticateAsync(client);

    var csv = """
      Name,Sku,CategoryName,SalePrice,CostPrice,StockQuantity
      ,MISSING-NAME,Bebidas,65.00,47.00,10
      Valid Product,VALID-SKU,,50.00,35.00,5
      """;

    using var content = BuildCsvFile(csv);
    var response = await client.PostAsync("/api/products/import", content);

    response.StatusCode.Should().Be(HttpStatusCode.Created);
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<ImportProductsResponse>>();
    payload!.IsSuccess.Should().BeTrue();
    payload.Data!.ImportedCount.Should().Be(1);
    payload.Data.SkippedCount.Should().Be(1);
  }

  [Fact]
  public async Task PostImportProductsShouldReturn201WithSkippedRowWhenSkuIsDuplicated()
  {
    using var factory = CreateFactory();
    using var client = factory.CreateClient();
    await AuthenticateAsync(client);

    var csv = """
      Name,Sku,CategoryName,SalePrice,CostPrice,StockQuantity
      Producto Duplicado,DUP-SKU-001,Bebidas,100.00,70.00,10
      """;

    // First import
    using var content1 = BuildCsvFile(csv);
    await client.PostAsync("/api/products/import", content1);

    // Second import with the same SKU
    using var content2 = BuildCsvFile(csv);
    var response = await client.PostAsync("/api/products/import", content2);

    response.StatusCode.Should().Be(HttpStatusCode.Created);
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<ImportProductsResponse>>();
    payload!.Data!.SkippedCount.Should().Be(1);
    payload.Data.Errors.Should().ContainSingle(e =>
      e.Messages.Any(m => m.Contains("already exists")));
  }

  // ── Helpers ───────────────────────────────────────────────────────────────

  private static string ValidCsv() =>
    "Name,Sku,CategoryName,SalePrice,CostPrice,StockQuantity\r\n" +
    "Leche Parmalat 1L,LEC-PARM-1L,Lácteos,95.00,75.00,60\r\n";

  private static MultipartFormDataContent BuildCsvFile(string csv)
  {
    var bytes = Encoding.UTF8.GetBytes(csv);
    var fileContent = new ByteArrayContent(bytes);
    fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/csv");

    return new MultipartFormDataContent
    {
      { fileContent, "file", "products.csv" }
    };
  }

  private static async Task AuthenticateAsync(HttpClient client)
  {
    var loginResponse = await client.PostAsJsonAsync(
      "/api/auth/login",
      new LoginRequest("admin@test.com", "Admin123!"));
    var login = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
    client.DefaultRequestHeaders.Authorization =
      new AuthenticationHeaderValue("Bearer", login!.Data!.AccessToken);
  }

  private static WebApplicationFactory<Program> CreateFactory()
    => new WebApplicationFactory<Program>()
      .WithWebHostBuilder(builder =>
      {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, cfg) =>
        {
          cfg.AddInMemoryCollection(
          [
            new("ConnectionStrings:DefaultConnection", ""),
            new("Database:InMemoryName", Guid.NewGuid().ToString("D")),
            new("RabbitMq:UseInMemory", "true"),
            new("Jwt:Secret", "test-secret-with-at-least-32-characters"),
            new("Jwt:Issuer", "SaasCommerce.Tests"),
            new("Jwt:Audience", "SaasCommerce.Tests"),
            new("Jwt:AccessTokenMinutes", "30")
          ]);
        });
      });
}
