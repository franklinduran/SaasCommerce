using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Domain;

public sealed class CashRegister
{
  private readonly List<CashRegisterMovement> movements = [];

  private CashRegister()
  {
  }

  private CashRegister(
    Guid id,
    BusinessId businessId,
    BranchId branchId,
    Guid userId,
    decimal openingAmount,
    string? notes,
    DateTimeOffset openedAt)
  {
    if (id == Guid.Empty)
    {
      throw new ArgumentException("Cash register id is required.", nameof(id));
    }

    if (userId == Guid.Empty)
    {
      throw new ArgumentException("User id is required.", nameof(userId));
    }

    if (openingAmount < 0)
    {
      throw new ArgumentException("Opening amount cannot be negative.", nameof(openingAmount));
    }

    Id = id;
    BusinessId = businessId;
    BranchId = branchId;
    UserId = userId;
    OpeningAmount = openingAmount;
    Notes = notes?.Trim();
    Status = CashRegisterStatus.Open;
    OpenedAt = openedAt;
    UpdatedAt = openedAt;
  }

  public Guid Id { get; private set; }

  public BusinessId BusinessId { get; private set; }

  public BranchId BranchId { get; private set; }

  public Guid UserId { get; private set; }

  public CashRegisterStatus Status { get; private set; }

  public decimal OpeningAmount { get; private set; }

  public string? Notes { get; private set; }

  public DateTimeOffset OpenedAt { get; private set; }

  public DateTimeOffset? ClosedAt { get; private set; }

  public DateTimeOffset UpdatedAt { get; private set; }

  // Populated at close time
  public decimal? CountedAmount { get; private set; }

  public decimal? ExpectedCashAmount { get; private set; }

  public decimal? Difference { get; private set; }

  public CashDifferenceType? DifferenceType { get; private set; }

  public string? CloseNotes { get; private set; }

  // Sales totals snapshot (stored at close)
  public decimal CashSalesTotal { get; private set; }

  public decimal CardSalesTotal { get; private set; }

  public decimal TransferSalesTotal { get; private set; }

  public decimal CreditSalesTotal { get; private set; }

  public decimal CashReturnsTotal { get; private set; }

  public IReadOnlyCollection<CashRegisterMovement> Movements => movements.AsReadOnly();

  public decimal ManualCashIn
    => movements.Where(m => m.MovementType == CashMovementType.CashIn).Sum(m => m.Amount);

  public decimal ManualCashOut
    => movements.Where(m => m.MovementType == CashMovementType.CashOut).Sum(m => m.Amount);

  public static CashRegister Open(
    Guid id,
    BusinessId businessId,
    BranchId branchId,
    Guid userId,
    decimal openingAmount,
    string? notes,
    DateTimeOffset openedAt)
    => new(id, businessId, branchId, userId, openingAmount, notes, openedAt);

  public CashRegisterMovement AddMovement(
    Guid movementId,
    Guid userId,
    CashMovementType movementType,
    decimal amount,
    string reason,
    DateTimeOffset createdAt)
  {
    if (Status != CashRegisterStatus.Open)
    {
      throw new InvalidOperationException("Movements can only be added to an open register.");
    }

    var movement = CashRegisterMovement.Create(movementId, Id, userId, movementType, amount, reason, createdAt);
    movements.Add(movement);
    UpdatedAt = createdAt;
    return movement;
  }

  public CashRegisterClosingResult Close(
    decimal countedAmount,
    CashRegisterTotals totals,
    DateTimeOffset closedAt,
    string? closeNotes)
  {
    ArgumentNullException.ThrowIfNull(totals);

    if (countedAmount < 0)
    {
      throw new ArgumentException("Counted amount cannot be negative.", nameof(countedAmount));
    }

    if (Status != CashRegisterStatus.Open)
    {
      throw new InvalidOperationException("Only open registers can be closed.");
    }

    CashSalesTotal = totals.CashSales;
    CardSalesTotal = totals.CardSales;
    TransferSalesTotal = totals.TransferSales;
    CreditSalesTotal = totals.CreditSales;
    CashReturnsTotal = totals.CashReturns;

    var cashIn = ManualCashIn;
    var cashOut = ManualCashOut;
    var expected = OpeningAmount + CashSalesTotal - CashReturnsTotal + cashIn - cashOut;
    var difference = countedAmount - expected;

    var differenceType = difference switch
    {
      0 => CashDifferenceType.Balanced,
      > 0 => CashDifferenceType.Surplus,
      _ => CashDifferenceType.Shortage
    };

    CountedAmount = countedAmount;
    ExpectedCashAmount = expected;
    Difference = difference;
    DifferenceType = differenceType;
    CloseNotes = closeNotes?.Trim();
    Status = CashRegisterStatus.Closed;
    ClosedAt = closedAt;
    UpdatedAt = closedAt;

    return new CashRegisterClosingResult(
      OpeningAmount,
      CashSalesTotal,
      CardSalesTotal,
      TransferSalesTotal,
      CreditSalesTotal,
      CashReturnsTotal,
      cashIn,
      cashOut,
      expected,
      countedAmount,
      difference,
      differenceType);
  }
}

public sealed record CashRegisterTotals(
  decimal CashSales,
  decimal CardSales,
  decimal TransferSales,
  decimal CreditSales,
  decimal CashReturns);

public sealed record CashRegisterClosingResult(
  decimal OpeningAmount,
  decimal CashSales,
  decimal CardSales,
  decimal TransferSales,
  decimal CreditSales,
  decimal CashReturns,
  decimal ManualCashIn,
  decimal ManualCashOut,
  decimal ExpectedCashAmount,
  decimal CountedAmount,
  decimal Difference,
  CashDifferenceType DifferenceType);
