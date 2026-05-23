using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Domain;

public sealed class CashSession
{
  private readonly List<CashMovement> movements = [];

  private CashSession()
  {
  }

  private CashSession(
    Guid id,
    BusinessId businessId,
    BranchId branchId,
    Guid userId,
    decimal openingBalance,
    string? notes,
    DateTimeOffset openedAt)
  {
    if (id == Guid.Empty)
    {
      throw new ArgumentException("Cash session id is required.", nameof(id));
    }

    if (userId == Guid.Empty)
    {
      throw new ArgumentException("User id is required.", nameof(userId));
    }

    if (openingBalance < 0)
    {
      throw new ArgumentException("Opening balance cannot be negative.", nameof(openingBalance));
    }

    Id = id;
    BusinessId = businessId;
    BranchId = branchId;
    UserId = userId;
    OpeningBalance = openingBalance;
    Notes = notes?.Trim();
    Status = CashSessionStatus.Open;
    OpenedAt = openedAt;
    UpdatedAt = openedAt;
  }

  public Guid Id { get; private set; }

  public BusinessId BusinessId { get; private set; }

  public BranchId BranchId { get; private set; }

  public Guid UserId { get; private set; }

  public CashSessionStatus Status { get; private set; }

  public decimal OpeningBalance { get; private set; }

  public decimal? ClosingBalance { get; private set; }

  public string? Notes { get; private set; }

  public DateTimeOffset OpenedAt { get; private set; }

  public DateTimeOffset? ClosedAt { get; private set; }

  public DateTimeOffset UpdatedAt { get; private set; }

  public IReadOnlyCollection<CashMovement> Movements => movements.AsReadOnly();

  /// <summary>
  /// Computed balance: opening + all CashIn - all CashOut movements.
  /// </summary>
  public decimal SystemBalance
    => OpeningBalance
       + movements.Where(m => m.Type == CashMovementType.CashIn).Sum(m => m.Amount)
       - movements.Where(m => m.Type == CashMovementType.CashOut).Sum(m => m.Amount);

  public static CashSession Create(
    Guid id,
    BusinessId businessId,
    BranchId branchId,
    Guid userId,
    decimal openingBalance,
    string? notes,
    DateTimeOffset openedAt)
    => new(id, businessId, branchId, userId, openingBalance, notes, openedAt);

  public CashMovement AddMovement(
    Guid movementId,
    Guid userId,
    CashMovementType type,
    decimal amount,
    string description,
    DateTimeOffset createdAt)
  {
    if (Status != CashSessionStatus.Open)
    {
      throw new InvalidOperationException("Movements can only be added to an open session.");
    }

    var movement = CashMovement.Create(movementId, Id, userId, type, amount, description, createdAt);
    movements.Add(movement);
    UpdatedAt = createdAt;
    return movement;
  }

  public CashSessionClosingResult Close(decimal closingBalance, DateTimeOffset closedAt)
  {
    if (closingBalance < 0)
    {
      throw new ArgumentException("Closing balance cannot be negative.", nameof(closingBalance));
    }

    if (Status != CashSessionStatus.Open)
    {
      throw new InvalidOperationException("Only open sessions can be closed.");
    }

    var system = SystemBalance;
    var difference = closingBalance - system;

    Status = CashSessionStatus.Closed;
    ClosingBalance = closingBalance;
    ClosedAt = closedAt;
    UpdatedAt = closedAt;

    return new CashSessionClosingResult(
      OpeningBalance,
      system,
      closingBalance,
      difference);
  }
}

/// <summary>
/// Result returned after closing a cash session.
/// </summary>
public sealed record CashSessionClosingResult(
  decimal OpeningBalance,
  decimal SystemBalance,
  decimal ClosingBalance,
  decimal Difference)
{
  /// <summary>
  /// Balanced = zero difference; Surplus = cashier has more than expected; Shortage = cashier has less.
  /// </summary>
  public string Outcome => Difference switch
  {
    0 => "Balanced",
    > 0 => "Surplus",
    _ => "Shortage"
  };
}
