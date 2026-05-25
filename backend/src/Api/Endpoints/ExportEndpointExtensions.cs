using SaasCommerce.Modules.Catalog.Application.Export;
using SaasCommerce.Modules.Customers.Application.Credits;
using SaasCommerce.Modules.Customers.Application.Customers;
using SaasCommerce.Modules.Identity.Contracts;
using SaasCommerce.Modules.Inventory.Application.Stock;
using SaasCommerce.Modules.Sales.Application.Cash;
using SaasCommerce.Modules.Sales.Application.DailyClosings;
using SaasCommerce.Modules.Sales.Application.Sales;

namespace SaasCommerce.Api.Endpoints;

internal static class ExportEndpointExtensions
{
  private const string ExportTag = "Exports";
  private const string CsvContentType = "text/csv";

  internal static WebApplication MapExportEndpoints(this WebApplication app)
  {
    // Products export
    app.MapGet(
      "/api/products/export",
      async (
        ExportProductsCsvHandler handler,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(new ExportProductsCsvQuery(), cancellationToken);

        return result.IsSuccess
          ? Results.File(result.Value, CsvContentType, $"productos_{DateTime.UtcNow:yyyyMMdd}.csv")
          : Results.BadRequest();
      })
      .RequireAuthorization($"Permission:{SystemPermissions.ProductsExport}")
      .WithTags(ExportTag);

    // Sales export
    app.MapGet(
      "/api/sales/export",
      async (
        DateTimeOffset? dateFrom,
        DateTimeOffset? dateTo,
        ExportSalesCsvHandler handler,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(
          new ExportSalesCsvQuery(dateFrom, dateTo),
          cancellationToken);

        return result.IsSuccess
          ? Results.File(result.Value, CsvContentType, $"ventas_{DateTime.UtcNow:yyyyMMdd}.csv")
          : Results.BadRequest();
      })
      .RequireAuthorization($"Permission:{SystemPermissions.SalesExport}")
      .WithTags(ExportTag);

    // Inventory export
    app.MapGet(
      "/api/inventory/export",
      async (
        ExportInventoryCsvHandler handler,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(new ExportInventoryCsvQuery(), cancellationToken);

        return result.IsSuccess
          ? Results.File(result.Value, CsvContentType, $"inventario_{DateTime.UtcNow:yyyyMMdd}.csv")
          : Results.BadRequest();
      })
      .RequireAuthorization($"Permission:{SystemPermissions.InventoryExport}")
      .WithTags(ExportTag);

    // Customers export
    app.MapGet(
      "/api/customers/export",
      async (
        ExportCustomersCsvHandler handler,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(new ExportCustomersCsvQuery(), cancellationToken);

        return result.IsSuccess
          ? Results.File(result.Value, CsvContentType, $"clientes_{DateTime.UtcNow:yyyyMMdd}.csv")
          : Results.BadRequest();
      })
      .RequireAuthorization($"Permission:{SystemPermissions.CustomersExport}")
      .WithTags(ExportTag);

    // Customer credits export
    app.MapGet(
      "/api/customer-credits/export",
      async (
        ExportCustomerCreditsCsvHandler handler,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(new ExportCustomerCreditsCsvQuery(), cancellationToken);

        return result.IsSuccess
          ? Results.File(result.Value, CsvContentType, $"creditos_{DateTime.UtcNow:yyyyMMdd}.csv")
          : Results.BadRequest();
      })
      .RequireAuthorization($"Permission:{SystemPermissions.CreditsExport}")
      .WithTags(ExportTag);

    // Cash sessions export
    app.MapGet(
      "/api/cash-registers/export",
      async (
        DateTimeOffset? dateFrom,
        DateTimeOffset? dateTo,
        ExportCashSessionsCsvHandler handler,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(
          new ExportCashSessionsCsvQuery(dateFrom, dateTo),
          cancellationToken);

        return result.IsSuccess
          ? Results.File(result.Value, CsvContentType, $"caja_{DateTime.UtcNow:yyyyMMdd}.csv")
          : Results.BadRequest();
      })
      .RequireAuthorization($"Permission:{SystemPermissions.CashExport}")
      .WithTags(ExportTag);

    // Daily closings export
    app.MapGet(
      "/api/reports/daily/export",
      async (
        DateOnly? dateFrom,
        DateOnly? dateTo,
        ExportDailyClosingsCsvHandler handler,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(
          new ExportDailyClosingsCsvQuery(dateFrom, dateTo),
          cancellationToken);

        return result.IsSuccess
          ? Results.File(result.Value, CsvContentType, $"cierres_{DateTime.UtcNow:yyyyMMdd}.csv")
          : Results.BadRequest();
      })
      .RequireAuthorization($"Permission:{SystemPermissions.ReportsExport}")
      .WithTags(ExportTag);

    return app;
  }
}
