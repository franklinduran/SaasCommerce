using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Purchasing.Domain;

public sealed class Purchase
{
  private readonly List<PurchaseItem> items = [];

  private Purchase()
  {
  }

  private Purchase(
    Guid id,
    BusinessId businessId,
    BranchId branchId,
    Guid supplierId,
    Guid userId,
    string? supplierInvoiceNumber,
    DateTimeOffset purchaseDate,
    string? notes,
    DateTimeOffset createdAt)
  {
    if (id == Guid.Empty)
    {
      throw new ArgumentException("Purchase id is required.", nameof(id));
    }

    if (supplierId == Guid.Empty)
    {
      throw new ArgumentException("Supplier id is required.", nameof(supplierId));
    }

    if (userId == Guid.Empty)
    {
      throw new ArgumentException("User id is required.", nameof(userId));
    }

    Id = id;
    BusinessId = businessId;
    BranchId = branchId;
    SupplierId = supplierId;
    UserId = userId;
    Status = PurchaseStatus.Draft;
    SupplierInvoiceNumber = NormalizeOptional(supplierInvoiceNumber);
    PurchaseDate = purchaseDate;
    Notes = NormalizeOptional(notes);
    CreatedAt = createdAt;
    UpdatedAt = createdAt;
  }

  public Guid Id { get; private set; }

  public BusinessId BusinessId { get; private set; }

  public BranchId BranchId { get; private set; }

  public Guid SupplierId { get; private set; }

  public Guid UserId { get; private set; }

  public PurchaseStatus Status { get; private set; }

  public string? SupplierInvoiceNumber { get; private set; }

  public DateTimeOffset PurchaseDate { get; private set; }

  public string? Notes { get; private set; }

  public decimal Total { get; private set; }

  public DateTimeOffset CreatedAt { get; private set; }

  public DateTimeOffset UpdatedAt { get; private set; }

  public DateTimeOffset? ReceivedAt { get; private set; }

  public DateTimeOffset? CancelledAt { get; private set; }

  public IReadOnlyCollection<PurchaseItem> Items => items.AsReadOnly();

  public static Purchase Create(
    Guid id,
    BusinessId businessId,
    BranchId branchId,
    Guid supplierId,
    Guid userId,
    IReadOnlyCollection<PurchaseLine> lines,
    string? supplierInvoiceNumber,
    DateTimeOffset purchaseDate,
    string? notes,
    DateTimeOffset createdAt)
  {
    ArgumentNullException.ThrowIfNull(lines);

    if (lines.Count == 0)
    {
      throw new InvalidOperationException("A purchase requires at least one item.");
    }

    var purchase = new Purchase(
      id,
      businessId,
      branchId,
      supplierId,
      userId,
      supplierInvoiceNumber,
      purchaseDate,
      notes,
      createdAt);

    foreach (var line in lines)
    {
      var item = new PurchaseItem(
        Guid.NewGuid(),
        purchase.Id,
        line.ProductId,
        line.Quantity,
        line.UnitCost);

      purchase.items.Add(item);
      purchase.Total += item.Subtotal;
    }

    return purchase;
  }

  public void Receive(DateTimeOffset receivedAt)
  {
    if (Status == PurchaseStatus.Cancelled)
    {
      throw new InvalidOperationException("Cancelled purchases cannot be received.");
    }

    if (Status == PurchaseStatus.Received)
    {
      throw new InvalidOperationException("Received purchases cannot be received twice.");
    }

    Status = PurchaseStatus.Received;
    ReceivedAt = receivedAt;
    UpdatedAt = receivedAt;
  }

  public void Cancel(DateTimeOffset cancelledAt)
  {
    if (Status == PurchaseStatus.Received)
    {
      throw new InvalidOperationException("Received purchases cannot be cancelled.");
    }

    if (Status == PurchaseStatus.Cancelled)
    {
      return;
    }

    Status = PurchaseStatus.Cancelled;
    CancelledAt = cancelledAt;
    UpdatedAt = cancelledAt;
  }

  private static string? NormalizeOptional(string? value)
    => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed record PurchaseLine(Guid ProductId, decimal Quantity, decimal UnitCost);
