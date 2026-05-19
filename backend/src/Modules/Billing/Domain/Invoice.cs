using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Billing.Domain;

public sealed record InvoiceContext(BusinessId BusinessId, BranchId BranchId, Guid? CustomerId);

public sealed record InvoiceFinancials(decimal Subtotal, decimal DiscountTotal, decimal TaxTotal, decimal Total);

public sealed class Invoice
{
  private Invoice()
  {
  }

  private Invoice(
    Guid id,
    Guid saleId,
    int sequence,
    InvoiceContext context,
    InvoiceFinancials financials,
    DateTimeOffset createdAt)
  {
    if (id == Guid.Empty)
    {
      throw new ArgumentException("Invoice id is required.", nameof(id));
    }

    if (saleId == Guid.Empty)
    {
      throw new ArgumentException("Sale id is required.", nameof(saleId));
    }

    if (sequence <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(sequence), "Invoice sequence must be greater than zero.");
    }

    EnsureNonNegative(financials.Subtotal, nameof(financials.Subtotal));
    EnsureNonNegative(financials.DiscountTotal, nameof(financials.DiscountTotal));
    EnsureNonNegative(financials.TaxTotal, nameof(financials.TaxTotal));
    EnsureNonNegative(financials.Total, nameof(financials.Total));

    Id = id;
    BusinessId = context.BusinessId;
    BranchId = context.BranchId;
    SaleId = saleId;
    CustomerId = context.CustomerId;
    Sequence = sequence;
    InvoiceNumber = CreateInvoiceNumber(sequence);
    Subtotal = financials.Subtotal;
    DiscountTotal = financials.DiscountTotal;
    TaxTotal = financials.TaxTotal;
    Total = financials.Total;
    Status = InvoiceStatus.Issued;
    CreatedAt = createdAt;
    UpdatedAt = createdAt;
  }

  public Guid Id { get; private set; }

  public BusinessId BusinessId { get; private set; }

  public BranchId BranchId { get; private set; }

  public Guid SaleId { get; private set; }

  public Guid? CustomerId { get; private set; }

  public int Sequence { get; private set; }

  public string InvoiceNumber { get; private set; } = string.Empty;

  public decimal Subtotal { get; private set; }

  public decimal DiscountTotal { get; private set; }

  public decimal TaxTotal { get; private set; }

  public decimal Total { get; private set; }

  public InvoiceStatus Status { get; private set; }

  public DateTimeOffset CreatedAt { get; private set; }

  public DateTimeOffset UpdatedAt { get; private set; }

  public DateTimeOffset? CancelledAt { get; private set; }

  public static Invoice Issue(
    Guid id,
    Guid saleId,
    int sequence,
    InvoiceContext context,
    InvoiceFinancials financials,
    DateTimeOffset createdAt)
    => new(id, saleId, sequence, context, financials, createdAt);

  public void Cancel(DateTimeOffset cancelledAt)
  {
    if (Status == InvoiceStatus.Cancelled)
    {
      return;
    }

    if (Status != InvoiceStatus.Issued)
    {
      throw new InvalidOperationException("Only issued invoices can be cancelled.");
    }

    Status = InvoiceStatus.Cancelled;
    CancelledAt = cancelledAt;
    UpdatedAt = cancelledAt;
  }

  private static string CreateInvoiceNumber(int sequence)
    => $"RI-{sequence:D8}";

  private static void EnsureNonNegative(decimal value, string parameterName)
  {
    if (value < 0)
    {
      throw new ArgumentOutOfRangeException(parameterName, "Amount cannot be negative.");
    }
  }
}
