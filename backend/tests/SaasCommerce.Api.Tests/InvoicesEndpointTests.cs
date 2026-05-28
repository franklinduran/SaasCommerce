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
using SaasCommerce.Modules.Billing.Contracts.Responses;
using SaasCommerce.Modules.Billing.Domain;
using SaasCommerce.Modules.Identity.Contracts.Requests;
using SaasCommerce.Modules.Identity.Contracts.Responses;
using SaasCommerce.SharedKernel.Tenancy;

#pragma warning disable CA1707

namespace SaasCommerce.Api.Tests;

public sealed class InvoicesEndpointTests
{
  private static int sequence = 2000000;

  [Fact]
  public async Task GetInvoices_ShouldFilterByBusinessId()
  {
    using var factory = CreateFactory();
    var tenantA = await RegisterBusinessAsync(factory, "invoices-a");
    var tenantB = await RegisterBusinessAsync(factory, "invoices-b");
    var invoiceA = await SeedInvoiceAsync(factory, tenantA.BusinessId, tenantA.BranchId, saleId: Guid.NewGuid());
    await SeedInvoiceAsync(factory, tenantB.BusinessId, tenantB.BranchId, saleId: Guid.NewGuid());

    var response = await tenantA.Client.GetAsync("/api/invoices?pageSize=50");
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<InvoiceListResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeTrue();
    payload.Data!.Items.Should().ContainSingle(invoice => invoice.InvoiceId == invoiceA.Id);
    payload.Data.Items.Should().OnlyContain(invoice => invoice.BusinessId == tenantA.BusinessId);
  }

  [Fact]
  public async Task GetInvoiceBySale_ShouldReturnCurrentBusinessInvoice()
  {
    using var factory = CreateFactory();
    var tenant = await RegisterBusinessAsync(factory, "invoice-sale");
    var saleId = Guid.NewGuid();
    var invoice = await SeedInvoiceAsync(factory, tenant.BusinessId, tenant.BranchId, saleId);

    var response = await tenant.Client.GetAsync($"/api/sales/{saleId}/invoice");
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<InvoiceResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeTrue();
    payload.Data!.InvoiceId.Should().Be(invoice.Id);
  }

  [Fact]
  public async Task CancelInvoice_ShouldChangeStatus_WhenInvoiceIsIssued()
  {
    using var factory = CreateFactory();
    var tenant = await RegisterBusinessAsync(factory, "invoice-cancel");
    var invoice = await SeedInvoiceAsync(factory, tenant.BusinessId, tenant.BranchId, saleId: Guid.NewGuid());

    var response = await tenant.Client.PostAsync($"/api/invoices/{invoice.Id}/cancel", null);
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<InvoiceResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeTrue();
    payload.Data!.Status.Should().Be(InvoiceStatus.Cancelled.ToString());
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
        "Principal",
        Guid.Parse("11111111-1111-1111-1111-111111111111")));
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

  private static async Task<Invoice> SeedInvoiceAsync(
    WebApplicationFactory<Program> factory,
    Guid businessId,
    Guid branchId,
    Guid saleId)
  {
    using var scope = factory.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var sequenceNumber = await dbContext.Set<Invoice>()
      .Where(invoice => invoice.BusinessId == new BusinessId(businessId))
      .Select(invoice => (int?)invoice.Sequence)
      .MaxAsync(CancellationToken.None) ?? 0;
    var invoice = Invoice.Issue(
      Guid.NewGuid(),
      saleId,
      sequenceNumber + 1,
      new InvoiceContext(new BusinessId(businessId), new BranchId(branchId), null),
      new InvoiceFinancials(300, 0, 0, 300),
      DateTimeOffset.UtcNow);

    dbContext.Set<Invoice>().Add(invoice);
    await dbContext.SaveChangesAsync(CancellationToken.None);

    return invoice;
  }

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
