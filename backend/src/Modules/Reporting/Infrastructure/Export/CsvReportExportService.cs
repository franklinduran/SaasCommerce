using System.Globalization;
using System.Text;
using SaasCommerce.Modules.Reporting.Application.Abstractions;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Reporting.Infrastructure.Export;

public sealed class CsvReportExportService(IReportsReadRepository reports) : IReportExportService
{
  private const int ExportPageSize = 5000;

  public async Task<byte[]> ExportSalesAsync(
    Guid businessId,
    SalesReportCriteria criteria,
    CancellationToken cancellationToken = default)
  {
    var exportCriteria = criteria with { Page = 1, PageSize = ExportPageSize };
    var result = await reports.GetSalesReportAsync(new BusinessId(businessId), exportCriteria, cancellationToken);

    var sb = new StringBuilder();
    sb.AppendLine("Venta ID,Codigo,Cliente,Sucursal,Estado,Metodo de Pago,Total,Fecha,Completada");

    foreach (var item in result.Items)
    {
      sb.AppendLine(string.Join(",",
        item.SaleId,
        Quote(item.Code),
        Quote(item.CustomerName ?? string.Empty),
        Quote(item.BranchName ?? string.Empty),
        Quote(item.Status),
        Quote(item.PaymentMethod),
        item.Total.ToString("F2", CultureInfo.InvariantCulture),
        item.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
        item.CompletedAt?.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) ?? string.Empty));
    }

    return Encoding.UTF8.GetBytes(sb.ToString());
  }

  public async Task<byte[]> ExportInvoicesAsync(
    Guid businessId,
    InvoiceReportCriteria criteria,
    CancellationToken cancellationToken = default)
  {
    var exportCriteria = criteria with { Page = 1, PageSize = ExportPageSize };
    var result = await reports.GetInvoiceReportAsync(new BusinessId(businessId), exportCriteria, cancellationToken);

    var sb = new StringBuilder();
    sb.AppendLine("Factura ID,Numero,Venta ID,Cliente,Estado,Subtotal,Impuesto,Total,Fecha");

    foreach (var item in result.Items)
    {
      sb.AppendLine(string.Join(",",
        item.InvoiceId,
        Quote(item.InvoiceNumber),
        item.SaleId,
        Quote(item.CustomerName ?? string.Empty),
        Quote(item.Status),
        item.Subtotal.ToString("F2", CultureInfo.InvariantCulture),
        item.TaxTotal.ToString("F2", CultureInfo.InvariantCulture),
        item.Total.ToString("F2", CultureInfo.InvariantCulture),
        item.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)));
    }

    return Encoding.UTF8.GetBytes(sb.ToString());
  }

  public async Task<byte[]> ExportAccountsReceivableAsync(
    Guid businessId,
    AccountsReceivableCriteria criteria,
    CancellationToken cancellationToken = default)
  {
    var exportCriteria = criteria with { Page = 1, PageSize = ExportPageSize };
    var result = await reports.GetAccountsReceivableReportAsync(
      new BusinessId(businessId), exportCriteria, cancellationToken);

    var sb = new StringBuilder();
    sb.AppendLine("Cliente ID,Nombre,Telefono,Limite de Credito,Balance Actual,Estado,Desde");

    foreach (var item in result.Items)
    {
      sb.AppendLine(string.Join(",",
        item.CustomerId,
        Quote(item.CustomerName),
        Quote(item.Phone ?? string.Empty),
        item.CreditLimit.ToString("F2", CultureInfo.InvariantCulture),
        item.CurrentBalance.ToString("F2", CultureInfo.InvariantCulture),
        Quote(item.Status),
        item.CreatedAt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)));
    }

    return Encoding.UTF8.GetBytes(sb.ToString());
  }

  public async Task<byte[]> ExportLowStockAsync(
    Guid businessId,
    LowStockCriteria criteria,
    CancellationToken cancellationToken = default)
  {
    var exportCriteria = criteria with { Page = 1, PageSize = ExportPageSize };
    var result = await reports.GetLowStockReportAsync(new BusinessId(businessId), exportCriteria, cancellationToken);

    var sb = new StringBuilder();
    sb.AppendLine("Producto ID,Nombre,SKU,Categoria,Sucursal,Stock Actual,Stock Minimo,Sugerido Reponer,Precio Venta");

    foreach (var item in result.Items)
    {
      sb.AppendLine(string.Join(",",
        item.ProductId,
        Quote(item.ProductName),
        Quote(item.Sku ?? string.Empty),
        Quote(item.CategoryName ?? string.Empty),
        Quote(item.BranchName ?? string.Empty),
        item.CurrentStock.ToString("F2", CultureInfo.InvariantCulture),
        item.MinimumStock.ToString("F2", CultureInfo.InvariantCulture),
        item.SuggestedRestock.ToString("F2", CultureInfo.InvariantCulture),
        item.SalePrice.ToString("F2", CultureInfo.InvariantCulture)));
    }

    return Encoding.UTF8.GetBytes(sb.ToString());
  }

  public async Task<byte[]> ExportPurchasesAsync(
    Guid businessId,
    PurchaseReportCriteria criteria,
    CancellationToken cancellationToken = default)
  {
    var exportCriteria = criteria with { Page = 1, PageSize = ExportPageSize };
    var result = await reports.GetPurchaseReportAsync(new BusinessId(businessId), exportCriteria, cancellationToken);

    var sb = new StringBuilder();
    sb.AppendLine("Compra ID,Proveedor,No. Factura Proveedor,Estado,Productos,Total,Fecha Compra");

    foreach (var item in result.Items)
    {
      sb.AppendLine(string.Join(",",
        item.PurchaseId,
        Quote(item.SupplierName ?? string.Empty),
        Quote(item.SupplierInvoiceNumber ?? string.Empty),
        Quote(item.Status),
        item.ItemCount,
        item.Total.ToString("F2", CultureInfo.InvariantCulture),
        item.PurchaseDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)));
    }

    return Encoding.UTF8.GetBytes(sb.ToString());
  }

  private static string Quote(string value)
    => $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
}
