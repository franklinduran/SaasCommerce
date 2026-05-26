using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Inventory.Domain;

public sealed class InventoryTransferDraft
{
  public required Guid Id { get; init; }
  public required BusinessId BusinessId { get; init; }
  public required BranchId SourceBranchId { get; init; }
  public required BranchId TargetBranchId { get; init; }
  public required Guid CreatedByUserId { get; init; }
  public required IReadOnlyCollection<InventoryTransferItem> Items { get; init; }
  public required DateTimeOffset CreatedAt { get; init; }
  public string? Note { get; init; }
}

public sealed class InventoryTransfer
{
  private readonly List<InventoryTransferItem> _items = [];

  private InventoryTransfer()
  {
  }

  public InventoryTransfer(InventoryTransferDraft draft)
  {
    if (draft.SourceBranchId == draft.TargetBranchId)
    {
      throw new InvalidOperationException("Source and target branches must be different.");
    }

    if (draft.Items is null || draft.Items.Count == 0)
    {
      throw new InvalidOperationException("Transfer must contain at least one item.");
    }

    if (draft.Items.Any(item => item.Quantity <= 0))
    {
      throw new InvalidOperationException("All transfer items must have a quantity greater than zero.");
    }

    Id = draft.Id;
    BusinessId = draft.BusinessId;
    SourceBranchId = draft.SourceBranchId;
    TargetBranchId = draft.TargetBranchId;
    CreatedByUserId = draft.CreatedByUserId;
    Status = InventoryTransferStatus.Pending;
    Note = NormalizeNote(draft.Note);
    CreatedAt = draft.CreatedAt;
    UpdatedAt = draft.CreatedAt;

    _items.AddRange(draft.Items);
  }

  public Guid Id { get; private set; }

  public BusinessId BusinessId { get; private set; }

  public BranchId SourceBranchId { get; private set; }

  public BranchId TargetBranchId { get; private set; }

  public Guid CreatedByUserId { get; private set; }

  public InventoryTransferStatus Status { get; private set; }

  public string? Note { get; private set; }

  public string? FailureReason { get; private set; }

  public DateTimeOffset CreatedAt { get; private set; }

  public DateTimeOffset UpdatedAt { get; private set; }

  public IReadOnlyCollection<InventoryTransferItem> Items => _items.AsReadOnly();

  public void Complete(DateTimeOffset updatedAt)
  {
    if (Status != InventoryTransferStatus.Pending)
    {
      throw new InvalidOperationException($"Cannot complete a transfer in status {Status}.");
    }

    Status = InventoryTransferStatus.Completed;
    UpdatedAt = updatedAt;
  }

  public void Fail(string reason, DateTimeOffset updatedAt)
  {
    if (Status != InventoryTransferStatus.Pending)
    {
      throw new InvalidOperationException($"Cannot fail a transfer in status {Status}.");
    }

    Status = InventoryTransferStatus.Failed;
    FailureReason = reason;
    UpdatedAt = updatedAt;
  }

  public void Cancel(DateTimeOffset updatedAt)
  {
    if (Status is InventoryTransferStatus.Completed or InventoryTransferStatus.Cancelled)
    {
      throw new InvalidOperationException($"Cannot cancel a transfer in status {Status}.");
    }

    Status = InventoryTransferStatus.Cancelled;
    UpdatedAt = updatedAt;
  }

  private static string? NormalizeNote(string? value)
    => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
