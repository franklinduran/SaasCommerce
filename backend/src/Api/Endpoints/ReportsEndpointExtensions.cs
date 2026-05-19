using SaasCommerce.Api;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Observability;
using SaasCommerce.Modules.Identity.Contracts;
using SaasCommerce.Modules.Reporting.Application.Abstractions;
using SaasCommerce.Modules.Reporting.Application.Dashboard;
using SaasCommerce.Modules.Reporting.Application.Reports;

namespace SaasCommerce.Api.Endpoints;

internal static class ReportsEndpointExtensions
{
  private const string DashboardTag = "Dashboard";
  private const string ReportsTag = "Reports";
  private const string ContentTypeCsv = "text/csv";

  internal static WebApplication MapReportsEndpoints(this WebApplication app)
  {
    app.MapGet(
      "/api/dashboard/summary",
      async (
        GetDashboardSummaryHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(new GetDashboardSummaryQuery(), cancellationToken);

        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.DashboardView}")
      .WithTags(DashboardTag);

    app.MapGet(
      "/api/reports/sales",
      async (
        [AsParameters] ReportDateRangeRequest request,
        [AsParameters] SalesReportEndpointRequest salesRequest,
        GetSalesReportHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(
          new GetSalesReportQuery(
            request.DateFrom,
            request.DateTo,
            salesRequest.BranchId,
            salesRequest.Status,
            salesRequest.PaymentMethod,
            salesRequest.Search,
            request.Page ?? 1,
            request.PageSize ?? 25),
          cancellationToken);

        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.ReportsView}")
      .WithTags(ReportsTag);

    app.MapGet(
      "/api/reports/sales/export",
      async (
        [AsParameters] ReportDateRangeRequest request,
        [AsParameters] SalesReportEndpointRequest salesRequest,
        IReportExportService exportService,
        ICurrentUserService currentUser,
        CancellationToken cancellationToken) =>
      {
        if (!currentUser.IsAuthenticated || currentUser.BusinessId is not { } businessId)
        {
          return Results.Unauthorized();
        }

        var criteria = new SalesReportCriteria(
          request.DateFrom,
          request.DateTo,
          salesRequest.BranchId,
          salesRequest.Status,
          salesRequest.PaymentMethod,
          salesRequest.Search,
          1,
          5000);

        var csv = await exportService.ExportSalesAsync(businessId, criteria, cancellationToken);

        return Results.File(csv, ContentTypeCsv, "ventas.csv");
      })
      .RequireAuthorization($"Permission:{SystemPermissions.ReportsExport}")
      .WithTags(ReportsTag);

    app.MapGet(
      "/api/reports/invoices",
      async (
        [AsParameters] ReportDateRangeRequest request,
        [AsParameters] InvoiceReportEndpointRequest invoiceRequest,
        GetInvoiceReportHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(
          new GetInvoiceReportQuery(
            request.DateFrom,
            request.DateTo,
            invoiceRequest.Status,
            invoiceRequest.CustomerId,
            invoiceRequest.Search,
            request.Page ?? 1,
            request.PageSize ?? 25),
          cancellationToken);

        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.ReportsView}")
      .WithTags(ReportsTag);

    app.MapGet(
      "/api/reports/invoices/export",
      async (
        [AsParameters] ReportDateRangeRequest request,
        [AsParameters] InvoiceReportEndpointRequest invoiceRequest,
        IReportExportService exportService,
        ICurrentUserService currentUser,
        CancellationToken cancellationToken) =>
      {
        if (!currentUser.IsAuthenticated || currentUser.BusinessId is not { } businessId)
        {
          return Results.Unauthorized();
        }

        var criteria = new InvoiceReportCriteria(
          request.DateFrom,
          request.DateTo,
          invoiceRequest.Status,
          invoiceRequest.CustomerId,
          invoiceRequest.Search,
          1,
          5000);

        var csv = await exportService.ExportInvoicesAsync(businessId, criteria, cancellationToken);

        return Results.File(csv, ContentTypeCsv, "facturas.csv");
      })
      .RequireAuthorization($"Permission:{SystemPermissions.ReportsExport}")
      .WithTags(ReportsTag);

    app.MapGet(
      "/api/reports/accounts-receivable",
      async (
        [AsParameters] ReportDateRangeRequest request,
        [AsParameters] AccountsReceivableEndpointRequest arRequest,
        GetAccountsReceivableReportHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(
          new GetAccountsReceivableReportQuery(
            arRequest.CustomerId,
            arRequest.Status,
            request.DateFrom,
            request.DateTo,
            request.Page ?? 1,
            request.PageSize ?? 25),
          cancellationToken);

        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.ReportsView}")
      .WithTags(ReportsTag);

    app.MapGet(
      "/api/reports/accounts-receivable/export",
      async (
        [AsParameters] ReportDateRangeRequest request,
        [AsParameters] AccountsReceivableEndpointRequest arRequest,
        IReportExportService exportService,
        ICurrentUserService currentUser,
        CancellationToken cancellationToken) =>
      {
        if (!currentUser.IsAuthenticated || currentUser.BusinessId is not { } businessId)
        {
          return Results.Unauthorized();
        }

        var criteria = new AccountsReceivableCriteria(
          arRequest.CustomerId,
          arRequest.Status,
          request.DateFrom,
          request.DateTo,
          1,
          5000);

        var csv = await exportService.ExportAccountsReceivableAsync(businessId, criteria, cancellationToken);

        return Results.File(csv, ContentTypeCsv, "cuentas-por-cobrar.csv");
      })
      .RequireAuthorization($"Permission:{SystemPermissions.ReportsExport}")
      .WithTags(ReportsTag);

    app.MapGet(
      "/api/reports/inventory-low-stock",
      async (
        [AsParameters] LowStockEndpointRequest lowStockRequest,
        GetLowStockReportHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(
          new GetLowStockReportQuery(
            lowStockRequest.BranchId,
            lowStockRequest.CategoryId,
            lowStockRequest.Search,
            lowStockRequest.Page ?? 1,
            lowStockRequest.PageSize ?? 25),
          cancellationToken);

        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.ReportsView}")
      .WithTags(ReportsTag);

    app.MapGet(
      "/api/reports/inventory-low-stock/export",
      async (
        [AsParameters] LowStockEndpointRequest lowStockRequest,
        IReportExportService exportService,
        ICurrentUserService currentUser,
        CancellationToken cancellationToken) =>
      {
        if (!currentUser.IsAuthenticated || currentUser.BusinessId is not { } businessId)
        {
          return Results.Unauthorized();
        }

        var criteria = new LowStockCriteria(
          lowStockRequest.BranchId,
          lowStockRequest.CategoryId,
          lowStockRequest.Search,
          1,
          5000);

        var csv = await exportService.ExportLowStockAsync(businessId, criteria, cancellationToken);

        return Results.File(csv, ContentTypeCsv, "inventario-bajo.csv");
      })
      .RequireAuthorization($"Permission:{SystemPermissions.ReportsExport}")
      .WithTags(ReportsTag);

    app.MapGet(
      "/api/reports/purchases",
      async (
        [AsParameters] ReportDateRangeRequest request,
        [AsParameters] PurchaseReportEndpointRequest purchaseRequest,
        GetPurchaseReportHandler handler,
        ICorrelationIdProvider correlationIdProvider,
        CancellationToken cancellationToken) =>
      {
        var result = await handler.Handle(
          new GetPurchaseReportQuery(
            request.DateFrom,
            request.DateTo,
            purchaseRequest.SupplierId,
            purchaseRequest.Status,
            purchaseRequest.BranchId,
            request.Page ?? 1,
            request.PageSize ?? 25),
          cancellationToken);

        return ApiHelpers.ToApiResult(result, correlationIdProvider);
      })
      .RequireAuthorization($"Permission:{SystemPermissions.ReportsView}")
      .WithTags(ReportsTag);

    app.MapGet(
      "/api/reports/purchases/export",
      async (
        [AsParameters] ReportDateRangeRequest request,
        [AsParameters] PurchaseReportEndpointRequest purchaseRequest,
        IReportExportService exportService,
        ICurrentUserService currentUser,
        CancellationToken cancellationToken) =>
      {
        if (!currentUser.IsAuthenticated || currentUser.BusinessId is not { } businessId)
        {
          return Results.Unauthorized();
        }

        var criteria = new PurchaseReportCriteria(
          request.DateFrom,
          request.DateTo,
          purchaseRequest.SupplierId,
          purchaseRequest.Status,
          purchaseRequest.BranchId,
          1,
          5000);

        var csv = await exportService.ExportPurchasesAsync(businessId, criteria, cancellationToken);

        return Results.File(csv, ContentTypeCsv, "compras.csv");
      })
      .RequireAuthorization($"Permission:{SystemPermissions.ReportsExport}")
      .WithTags(ReportsTag);

    return app;
  }
}
