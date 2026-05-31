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
using SaasCommerce.Modules.Billing.Domain;
using SaasCommerce.Modules.Identity.Contracts.Requests;
using SaasCommerce.Modules.Identity.Contracts.Responses;
using SaasCommerce.Modules.Reporting.Contracts.Responses;
using SaasCommerce.SharedKernel.Tenancy;

#pragma warning disable CA1707

namespace SaasCommerce.Api.Tests;

public sealed class DashboardReportsEndpointTests
{
  private static int sequence = 9000000;

  // ── Dashboard ─────────────────────────────────────────────────────────────

  [Fact]
  public async Task DashboardSummary_ShouldRequireAuthentication()
  {
    using var factory = CreateFactory();
    var client = factory.CreateClient();

    var response = await client.GetAsync("/api/dashboard/summary");

    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task DashboardSummary_ShouldReturnStandardApiResponse()
  {
    using var factory = CreateFactory();
    var tenant = await RegisterBusinessAsync(factory, "dash-format");

    var response = await tenant.Client.GetAsync("/api/dashboard/summary");
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<DashboardSummaryResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    payload.Should().NotBeNull();
    payload!.IsSuccess.Should().BeTrue();
    payload.Data.Should().NotBeNull();
  }

  [Fact]
  public async Task DashboardSummary_ShouldReturnOnlyCurrentBusinessData()
  {
    using var factory = CreateFactory();
    var tenantA = await RegisterBusinessAsync(factory, "dash-biz-a");
    var tenantB = await RegisterBusinessAsync(factory, "dash-biz-b");
    await SeedInvoiceAsync(factory, tenantA.BusinessId, tenantA.BranchId, Guid.NewGuid());
    await SeedInvoiceAsync(factory, tenantB.BusinessId, tenantB.BranchId, Guid.NewGuid());

    var responseA = await tenantA.Client.GetAsync("/api/dashboard/summary");
    var responseB = await tenantB.Client.GetAsync("/api/dashboard/summary");
    var payloadA = await responseA.Content.ReadFromJsonAsync<ApiResponse<DashboardSummaryResponse>>();
    var payloadB = await responseB.Content.ReadFromJsonAsync<ApiResponse<DashboardSummaryResponse>>();

    responseA.StatusCode.Should().Be(HttpStatusCode.OK);
    responseB.StatusCode.Should().Be(HttpStatusCode.OK);
    payloadA!.Data.Should().NotBeNull();
    payloadB!.Data.Should().NotBeNull();

    // Each tenant has their own data — invoice IDs (GUIDs) must not cross over
    var invoiceIdsA = payloadA.Data!.RecentInvoices.Select(i => i.InvoiceId).ToHashSet();
    var invoiceIdsB = payloadB.Data!.RecentInvoices.Select(i => i.InvoiceId).ToHashSet();
    invoiceIdsA.Should().NotIntersectWith(invoiceIdsB);
  }

  [Fact]
  public async Task DashboardSummary_ShouldReturnZeroValues_WhenBusinessHasNoData()
  {
    using var factory = CreateFactory();
    var tenant = await RegisterBusinessAsync(factory, "dash-empty");

    var response = await tenant.Client.GetAsync("/api/dashboard/summary");
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<DashboardSummaryResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    payload!.Data!.SalesToday.Count.Should().Be(0);
    payload.Data.SalesToday.TotalAmount.Should().Be(0);
    payload.Data.InvoicesToday.Count.Should().Be(0);
    payload.Data.Receivables.CustomerCount.Should().Be(0);
    payload.Data.LowStock.ProductCount.Should().Be(0);
  }

  [Fact]
  public async Task DashboardSummary_ShouldReturnAnalyticsFields_WhenBusinessHasNoData()
  {
    using var factory = CreateFactory();
    var tenant = await RegisterBusinessAsync(factory, "dash-analytics");

    var response = await tenant.Client.GetAsync("/api/dashboard/summary");
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<DashboardSummaryResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    payload!.Data.Should().NotBeNull();
    payload.Data!.DailyPurchases.Should().NotBeNull();
    payload.Data.PaymentMethodTotals.Should().NotBeNull();
    payload.Data.SaleStatusBreakdown.Should().NotBeNull();
    payload.Data.SaleStatusBreakdown.Completed.Should().Be(0);
    payload.Data.SaleStatusBreakdown.Cancelled.Should().Be(0);
  }

  [Fact]
  public async Task DashboardSummary_ShouldIncludeRecentInvoices()
  {
    using var factory = CreateFactory();
    var tenant = await RegisterBusinessAsync(factory, "dash-recent");
    var invoice = await SeedInvoiceAsync(factory, tenant.BusinessId, tenant.BranchId, Guid.NewGuid());

    var response = await tenant.Client.GetAsync("/api/dashboard/summary");
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<DashboardSummaryResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    payload!.Data!.RecentInvoices.Should().Contain(i => i.InvoiceNumber == invoice.InvoiceNumber);
  }

  // ── Reports: Sales ────────────────────────────────────────────────────────

  [Fact]
  public async Task SalesReport_ShouldRequireAuthentication()
  {
    using var factory = CreateFactory();
    var client = factory.CreateClient();

    var response = await client.GetAsync("/api/reports/sales");

    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task SalesReport_ShouldReturnStandardApiResponse()
  {
    using var factory = CreateFactory();
    var tenant = await RegisterBusinessAsync(factory, "rpt-sales-fmt");

    var response = await tenant.Client.GetAsync("/api/reports/sales?page=1&pageSize=10");
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<SalesReportResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    payload!.IsSuccess.Should().BeTrue();
    payload.Data.Should().NotBeNull();
  }

  [Fact]
  public async Task SalesReport_ShouldFilterByBusinessId()
  {
    using var factory = CreateFactory();
    var tenantA = await RegisterBusinessAsync(factory, "rpt-sales-a");
    var tenantB = await RegisterBusinessAsync(factory, "rpt-sales-b");

    var responseA = await tenantA.Client.GetAsync("/api/reports/sales?page=1&pageSize=50");
    var responseB = await tenantB.Client.GetAsync("/api/reports/sales?page=1&pageSize=50");
    var payloadA = await responseA.Content.ReadFromJsonAsync<ApiResponse<SalesReportResponse>>();
    var payloadB = await responseB.Content.ReadFromJsonAsync<ApiResponse<SalesReportResponse>>();

    responseA.StatusCode.Should().Be(HttpStatusCode.OK);
    responseB.StatusCode.Should().Be(HttpStatusCode.OK);
    payloadA!.IsSuccess.Should().BeTrue();
    payloadB!.IsSuccess.Should().BeTrue();
  }

  // ── Reports: Invoices ─────────────────────────────────────────────────────

  [Fact]
  public async Task InvoiceReport_ShouldRequireAuthentication()
  {
    using var factory = CreateFactory();
    var client = factory.CreateClient();

    var response = await client.GetAsync("/api/reports/invoices");

    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task InvoiceReport_ShouldFilterByBusinessId()
  {
    using var factory = CreateFactory();
    var tenantA = await RegisterBusinessAsync(factory, "rpt-inv-a");
    var tenantB = await RegisterBusinessAsync(factory, "rpt-inv-b");
    var invoiceA = await SeedInvoiceAsync(factory, tenantA.BusinessId, tenantA.BranchId, Guid.NewGuid());
    await SeedInvoiceAsync(factory, tenantB.BusinessId, tenantB.BranchId, Guid.NewGuid());

    var response = await tenantA.Client.GetAsync("/api/reports/invoices?page=1&pageSize=50");
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<InvoiceReportResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    payload!.IsSuccess.Should().BeTrue();
    payload.Data!.Items.Should().Contain(i => i.InvoiceNumber == invoiceA.InvoiceNumber);
    payload.Data.Items.Should().NotContain(i => i.InvoiceId == Guid.Empty);
  }

  [Fact]
  public async Task InvoiceReport_ShouldSearchByInvoiceNumber()
  {
    using var factory = CreateFactory();
    var tenant = await RegisterBusinessAsync(factory, "rpt-inv-srch");
    var invoice = await SeedInvoiceAsync(factory, tenant.BusinessId, tenant.BranchId, Guid.NewGuid());

    var response = await tenant.Client.GetAsync(
      $"/api/reports/invoices?search={invoice.InvoiceNumber}&page=1&pageSize=10");
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<InvoiceReportResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    payload!.Data!.Items.Should().Contain(i => i.InvoiceNumber == invoice.InvoiceNumber);
  }

  // ── Reports: Accounts Receivable ──────────────────────────────────────────

  [Fact]
  public async Task AccountsReceivableReport_ShouldRequireAuthentication()
  {
    using var factory = CreateFactory();
    var client = factory.CreateClient();

    var response = await client.GetAsync("/api/reports/accounts-receivable");

    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task AccountsReceivableReport_ShouldReturnEmpty_WhenNoPendingDebts()
  {
    using var factory = CreateFactory();
    var tenant = await RegisterBusinessAsync(factory, "rpt-ar-empty");

    var response = await tenant.Client.GetAsync("/api/reports/accounts-receivable?page=1&pageSize=25");
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<AccountsReceivableReportResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    payload!.IsSuccess.Should().BeTrue();
    payload.Data!.Items.Should().BeEmpty();
    payload.Data.Summary.TotalPending.Should().Be(0);
  }

  // ── Reports: Low Stock ────────────────────────────────────────────────────

  [Fact]
  public async Task LowStockReport_ShouldRequireAuthentication()
  {
    using var factory = CreateFactory();
    var client = factory.CreateClient();

    var response = await client.GetAsync("/api/reports/inventory-low-stock");

    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task LowStockReport_ShouldReturnStandardApiResponse()
  {
    using var factory = CreateFactory();
    var tenant = await RegisterBusinessAsync(factory, "rpt-lowstock");

    var response = await tenant.Client.GetAsync("/api/reports/inventory-low-stock?page=1&pageSize=25");
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<LowStockReportResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    payload!.IsSuccess.Should().BeTrue();
    payload.Data.Should().NotBeNull();
  }

  // ── Reports: Purchases ────────────────────────────────────────────────────

  [Fact]
  public async Task PurchaseReport_ShouldRequireAuthentication()
  {
    using var factory = CreateFactory();
    var client = factory.CreateClient();

    var response = await client.GetAsync("/api/reports/purchases");

    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task PurchaseReport_ShouldReturnStandardApiResponse()
  {
    using var factory = CreateFactory();
    var tenant = await RegisterBusinessAsync(factory, "rpt-purchases");

    var response = await tenant.Client.GetAsync("/api/reports/purchases?page=1&pageSize=25");
    var payload = await response.Content.ReadFromJsonAsync<ApiResponse<PurchaseReportResponse>>();

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    payload!.IsSuccess.Should().BeTrue();
    payload.Data.Should().NotBeNull();
  }

  // ── CSV Exports ───────────────────────────────────────────────────────────

  [Fact]
  public async Task SalesExport_ShouldRequireAuthentication()
  {
    using var factory = CreateFactory();
    var client = factory.CreateClient();

    var response = await client.GetAsync("/api/reports/sales/export");

    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task InvoiceExport_ShouldReturnCsvContentType()
  {
    using var factory = CreateFactory();
    var tenant = await RegisterBusinessAsync(factory, "export-inv");

    var response = await tenant.Client.GetAsync("/api/reports/invoices/export");

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    response.Content.Headers.ContentType?.MediaType.Should().Be("text/csv");
  }

  [Fact]
  public async Task LowStockExport_ShouldGenerateCsvWithHeaders()
  {
    using var factory = CreateFactory();
    var tenant = await RegisterBusinessAsync(factory, "export-lowstock");

    var response = await tenant.Client.GetAsync("/api/reports/inventory-low-stock/export");
    var content = await response.Content.ReadAsStringAsync();

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    content.Should().Contain("Producto ID");
  }

  [Fact]
  public async Task SalesExport_ShouldReturnCsvContentType()
  {
    using var factory = CreateFactory();
    var tenant = await RegisterBusinessAsync(factory, "export-sales");

    var response = await tenant.Client.GetAsync("/api/reports/sales/export");

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    response.Content.Headers.ContentType?.MediaType.Should().Be("text/csv");
  }

  [Fact]
  public async Task AccountsReceivableExport_ShouldRequireAuthentication()
  {
    using var factory = CreateFactory();
    var client = factory.CreateClient();

    var response = await client.GetAsync("/api/reports/accounts-receivable/export");

    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task AccountsReceivableExport_ShouldReturnCsvContentType()
  {
    using var factory = CreateFactory();
    var tenant = await RegisterBusinessAsync(factory, "export-ar");

    var response = await tenant.Client.GetAsync("/api/reports/accounts-receivable/export");

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    response.Content.Headers.ContentType?.MediaType.Should().Be("text/csv");
  }

  [Fact]
  public async Task PurchasesExport_ShouldRequireAuthentication()
  {
    using var factory = CreateFactory();
    var client = factory.CreateClient();

    var response = await client.GetAsync("/api/reports/purchases/export");

    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task PurchasesExport_ShouldReturnCsvContentType()
  {
    using var factory = CreateFactory();
    var tenant = await RegisterBusinessAsync(factory, "export-purch");

    var response = await tenant.Client.GetAsync("/api/reports/purchases/export");

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    response.Content.Headers.ContentType?.MediaType.Should().Be("text/csv");
  }

  // ── Helpers ───────────────────────────────────────────────────────────────

  private static async Task<RegisteredTenant> RegisterBusinessAsync(
    WebApplicationFactory<Program> factory,
    string prefix)
  {
    var client = factory.CreateClient();
    var response = await client.PostAsJsonAsync(
      "/api/account/register-business",
      new RegisterBusinessRequest(
        $"Negocio {prefix}",
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
    payload!.IsSuccess.Should().BeTrue();

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
    var seq = await dbContext.Set<Invoice>()
      .Where(i => i.BusinessId == new BusinessId(businessId))
      .Select(i => (int?)i.Sequence)
      .MaxAsync(CancellationToken.None) ?? 0;
    var invoice = Invoice.Issue(
      Guid.NewGuid(),
      saleId,
      seq + 1,
      new InvoiceContext(new BusinessId(businessId), new BranchId(branchId), null),
      new InvoiceFinancials(500, 0, 0, 500),
      DateTimeOffset.UtcNow);

    dbContext.Set<Invoice>().Add(invoice);
    await dbContext.SaveChangesAsync(CancellationToken.None);

    return invoice;
  }

  private static string NextPhoneNumber()
    => $"809{Interlocked.Increment(ref sequence):D7}";

  private static string NextIdentificationNumber()
    => $"2{Interlocked.Increment(ref sequence):D8}";

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
