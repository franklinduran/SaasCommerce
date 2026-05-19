using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Purchasing.Domain;

public sealed record PurchaseCreationData(
    Guid Id,
    BusinessId BusinessId,
    BranchId BranchId,
    Guid SupplierId,
    Guid UserId,
    string? SupplierInvoiceNumber,
    DateTimeOffset PurchaseDate,
    string? Notes,
    DateTimeOffset CreatedAt);

public sealed class Purchase
{
  private readonly List<PurchaseItem> items = [];

  private Purchase()
  {
  }

  private Purchase(PurchaseCreationData data)
  {
    ArgumentNullException.ThrowIfNull(data);

    if (data.Id == Guid.Empty)
    {
      throw new ArgumentException("Purchase id is required.", nameof(data));
    }

    if (data.SupplierId == Guid.Empty)
    {
      throw new ArgumentException("Supplier id is required.", nameof(data));
    }

    if (data.UserId == Guid.Empty)
    {
      throw new ArgumentException("User id is required.", nameof(data));
    }

    Id = data.Id;
    BusinessId = data.BusinessId;
    BranchId = data.BranchId;
    SupplierId = data.SupplierId;
    UserId = data.UserId;
    Status = PurchaseStatus.Draft;
    SupplierInvoiceNumber = NormalizeOptional(data.SupplierInvoiceNumber);
    PurchaseDate = data.PurchaseDate;
    Notes = NormalizeOptional(data.Notes);
    CreatedAt = data.CreatedAt;
    UpdatedAt = data.CreatedAt;
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

  public static Purchase Create(PurchaseCreationData data, IReadOnlyCollection<PurchaseLine> lines)
  {
    ArgumentNullException.ThrowIfNull(lines);

    if (lines.Count == 0)
    {
      throw new InvalidOperationException("A purchase requires at least one item.");
    }

    var purchase = new Purchase(data);

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
