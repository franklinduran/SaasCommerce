namespace SaasCommerce.Api.Endpoints;

internal sealed class ReportDateRangeRequest
{
  public DateTimeOffset? DateFrom { get; init; }

  public DateTimeOffset? DateTo { get; init; }

  public int? Page { get; init; }

  public int? PageSize { get; init; }
}

internal sealed class SalesReportEndpointRequest
{
  public Guid? BranchId { get; init; }

  public string? Status { get; init; }

  public string? PaymentMethod { get; init; }

  public string? Search { get; init; }
}

internal sealed class InvoiceReportEndpointRequest
{
  public string? Status { get; init; }

  public Guid? CustomerId { get; init; }

  public string? Search { get; init; }
}

internal sealed class AccountsReceivableEndpointRequest
{
  public Guid? CustomerId { get; init; }

  public string? Status { get; init; }
}

internal sealed class LowStockEndpointRequest
{
  public Guid? BranchId { get; init; }

  public Guid? CategoryId { get; init; }

  public string? Search { get; init; }

  public int? Page { get; init; }

  public int? PageSize { get; init; }
}

internal sealed class PurchaseReportEndpointRequest
{
  public Guid? SupplierId { get; init; }

  public string? Status { get; init; }

  public Guid? BranchId { get; init; }
}
