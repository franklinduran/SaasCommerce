using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Domain;

public sealed class SaleReturn
{
  private readonly List<SaleReturnItem> items = [];

  private SaleReturn()
  {
  }

  private SaleReturn(
    Guid id,
    BusinessId businessId,
    BranchId branchId,
    Guid saleId,
    Guid userId,
    string reason,
    DateTimeOffset requestedAt)
  {
    if (id == Guid.Empty)
    {
      throw new ArgumentException("Return id is required.", nameof(id));
    }

    if (saleId == Guid.Empty)
    {
      throw new ArgumentException("Sale id is required.", nameof(saleId));
    }

    if (userId == Guid.Empty)
    {
      throw new ArgumentException("User id is required.", nameof(userId));
    }

    ArgumentException.ThrowIfNullOrWhiteSpace(reason);

    Id = id;
    BusinessId = businessId;
    BranchId = branchId;
    SaleId = saleId;
    UserId = userId;
    Reason = NormalizeReason(reason);
    Status = SaleReturnStatus.Requested;
    RequestedAt = requestedAt;
    UpdatedAt = requestedAt;
  }

  public Guid Id { get; private set; }

  public BusinessId BusinessId { get; private set; }

  public BranchId BranchId { get; private set; }

  public Guid SaleId { get; private set; }

  public Guid UserId { get; private set; }

  public SaleReturnStatus Status { get; private set; }

  public string Reason { get; private set; } = string.Empty;

  public decimal Total { get; private set; }

  public DateTimeOffset RequestedAt { get; private set; }

  public DateTimeOffset? ApprovedAt { get; private set; }

  public DateTimeOffset? FailedAt { get; private set; }

  public string? FailureReason { get; private set; }

  public DateTimeOffset UpdatedAt { get; private set; }

  public IReadOnlyCollection<SaleReturnItem> Items => items.AsReadOnly();

  public static SaleReturn Request(
    Guid id,
    Sale sale,
    Guid userId,
    string reason,
    IReadOnlyCollection<SaleReturnLine> lines,
    DateTimeOffset requestedAt)
  {
    ArgumentNullException.ThrowIfNull(sale);
    ArgumentNullException.ThrowIfNull(lines);

    if (sale.Status != SaleStatus.Completed)
    {
      throw new InvalidOperationException("Only completed sales can be returned.");
    }

    if (lines.Count == 0)
    {
      throw new InvalidOperationException("A return requires at least one item.");
    }

    var saleReturn = new SaleReturn(
      id,
      sale.BusinessId,
      sale.BranchId,
      sale.Id,
      userId,
      reason,
      requestedAt);

    foreach (var line in lines)
    {
      var saleItem = sale.Items.SingleOrDefault(item => item.Id == line.SaleItemId);
      if (saleItem is null)
      {
        throw new InvalidOperationException("Returned item does not belong to the sale.");
      }

      if (line.Quantity > saleItem.Quantity)
      {
        throw new InvalidOperationException("Returned quantity exceeds sold quantity.");
      }

      var item = new SaleReturnItem(
        Guid.NewGuid(),
        saleReturn.Id,
        saleItem.Id,
        saleItem.ProductId,
        line.Quantity,
        saleItem.UnitPrice,
        saleItem.UnitCost);

      saleReturn.items.Add(item);
      saleReturn.Total += item.LineTotal;
    }

    return saleReturn;
  }

  public void Approve(DateTimeOffset approvedAt)
  {
    if (Status == SaleReturnStatus.Approved)
    {
      return;
    }

    if (Status != SaleReturnStatus.Requested)
    {
      throw new InvalidOperationException("Only requested returns can be approved.");
    }

    Status = SaleReturnStatus.Approved;
    ApprovedAt = approvedAt;
    UpdatedAt = approvedAt;
  }

  public void Fail(string reason, DateTimeOffset failedAt)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(reason);

    if (Status == SaleReturnStatus.Failed)
    {
      return;
    }

    if (Status == SaleReturnStatus.Approved)
    {
      throw new InvalidOperationException("Approved returns cannot be failed.");
    }

    Status = SaleReturnStatus.Failed;
    FailureReason = NormalizeReason(reason);
    FailedAt = failedAt;
    UpdatedAt = failedAt;
  }

  private static string NormalizeReason(string reason)
  {
    var trimmed = reason.Trim();
    return trimmed.Length <= 500 ? trimmed : trimmed[..500];
  }
}

public sealed record SaleReturnLine(Guid SaleItemId, decimal Quantity);
