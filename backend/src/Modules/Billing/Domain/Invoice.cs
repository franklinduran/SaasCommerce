using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Billing.Domain;

public sealed class Invoice
{
  private Invoice()
  {
  }

  private Invoice(
    Guid id,
    BusinessId businessId,
    BranchId branchId,
    Guid saleId,
    Guid? customerId,
    int sequence,
    decimal subtotal,
    decimal discountTotal,
    decimal taxTotal,
    decimal total,
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

    EnsureNonNegative(subtotal, nameof(subtotal));
    EnsureNonNegative(discountTotal, nameof(discountTotal));
    EnsureNonNegative(taxTotal, nameof(taxTotal));
    EnsureNonNegative(total, nameof(total));

    Id = id;
    BusinessId = businessId;
    BranchId = branchId;
    SaleId = saleId;
    CustomerId = customerId;
    Sequence = sequence;
    InvoiceNumber = CreateInvoiceNumber(sequence);
    Subtotal = subtotal;
    DiscountTotal = discountTotal;
    TaxTotal = taxTotal;
    Total = total;
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
    BusinessId businessId,
    BranchId branchId,
    Guid saleId,
    Guid? customerId,
    int sequence,
    decimal subtotal,
    decimal discountTotal,
    decimal taxTotal,
    decimal total,
    DateTimeOffset createdAt)
    => new(
      id,
      businessId,
      branchId,
      saleId,
      customerId,
      sequence,
      subtotal,
      discountTotal,
      taxTotal,
      total,
      createdAt);

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
