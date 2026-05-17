using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Domain;

public sealed class Sale
{
  private readonly List<SaleItem> items = [];

  private Sale()
  {
  }

  private Sale(
    Guid id,
    BusinessId businessId,
    BranchId branchId,
    Guid userId,
    string paymentMethod,
    DateTimeOffset createdAt)
  {
    if (id == Guid.Empty)
    {
      throw new ArgumentException("Sale id is required.", nameof(id));
    }

    if (userId == Guid.Empty)
    {
      throw new ArgumentException("User id is required.", nameof(userId));
    }

    ArgumentException.ThrowIfNullOrWhiteSpace(paymentMethod);

    Id = id;
    BusinessId = businessId;
    BranchId = branchId;
    UserId = userId;
    PaymentMethod = paymentMethod.Trim();
    Status = SaleStatus.Received;
    CreatedAt = createdAt;
    UpdatedAt = createdAt;
  }

  public Guid Id { get; private set; }

  public BusinessId BusinessId { get; private set; }

  public BranchId BranchId { get; private set; }

  public Guid UserId { get; private set; }

  public SaleStatus Status { get; private set; }

  public string PaymentMethod { get; private set; } = string.Empty;

  public decimal Total { get; private set; }

  public DateTimeOffset CreatedAt { get; private set; }

  public DateTimeOffset UpdatedAt { get; private set; }

  public DateTimeOffset? CompletedAt { get; private set; }

  public DateTimeOffset? FailedAt { get; private set; }

  public DateTimeOffset? CancelledAt { get; private set; }

  public string? FailureReason { get; private set; }

  public string? CancellationReason { get; private set; }

  public IReadOnlyCollection<SaleItem> Items => items.AsReadOnly();

  public static Sale Create(
    Guid id,
    BusinessId businessId,
    BranchId branchId,
    Guid userId,
    IReadOnlyCollection<SaleLine> lines,
    string paymentMethod,
    DateTimeOffset createdAt)
  {
    ArgumentNullException.ThrowIfNull(lines);

    if (lines.Count == 0)
    {
      throw new InvalidOperationException("A sale requires at least one item.");
    }

    var sale = new Sale(id, businessId, branchId, userId, paymentMethod, createdAt);

    foreach (var line in lines)
    {
      var item = new SaleItem(
        Guid.NewGuid(),
        sale.Id,
        line.ProductId,
        line.Quantity,
        line.UnitPrice);

      sale.items.Add(item);
      sale.Total += item.LineTotal;
    }

    return sale;
  }

  public void MarkAsProcessing(DateTimeOffset processedAt)
  {
    if (Status == SaleStatus.Cancelled)
    {
      throw new InvalidOperationException("Cancelled sales cannot be processed.");
    }

    if (Status != SaleStatus.Received)
    {
      throw new InvalidOperationException("Only received sales can move to processing.");
    }

    Status = SaleStatus.Processing;
    UpdatedAt = processedAt;
  }

  public void Complete(DateTimeOffset completedAt)
  {
    if (Status == SaleStatus.Failed)
    {
      throw new InvalidOperationException("Failed sales cannot be completed.");
    }

    if (Status == SaleStatus.Cancelled)
    {
      throw new InvalidOperationException("Cancelled sales cannot be completed.");
    }

    if (Status != SaleStatus.Processing)
    {
      throw new InvalidOperationException("Only processing sales can be completed.");
    }

    Status = SaleStatus.Completed;
    CompletedAt = completedAt;
    UpdatedAt = completedAt;
  }

  public void Fail(string reason, DateTimeOffset failedAt)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(reason);

    if (Status == SaleStatus.Completed)
    {
      throw new InvalidOperationException("Completed sales cannot be failed.");
    }

    if (Status == SaleStatus.Cancelled)
    {
      throw new InvalidOperationException("Cancelled sales cannot be failed.");
    }

    if (Status == SaleStatus.Failed)
    {
      return;
    }

    Status = SaleStatus.Failed;
    FailureReason = reason.Trim();
    FailedAt = failedAt;
    UpdatedAt = failedAt;
  }

  public void Cancel(string reason, DateTimeOffset cancelledAt)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(reason);

    if (Status == SaleStatus.Completed)
    {
      throw new InvalidOperationException("Completed sales cannot be cancelled.");
    }

    if (Status == SaleStatus.Failed)
    {
      throw new InvalidOperationException("Failed sales cannot be cancelled.");
    }

    if (Status == SaleStatus.Cancelled)
    {
      return;
    }

    Status = SaleStatus.Cancelled;
    CancellationReason = reason.Trim();
    CancelledAt = cancelledAt;
    UpdatedAt = cancelledAt;
  }
}

public sealed record SaleLine(Guid ProductId, decimal Quantity, decimal UnitPrice);
