using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Domain;

public sealed class DailyClosing
{
  private readonly List<DailyClosingAlert> alerts = [];

  private DailyClosing()
  {
  }

  private DailyClosing(
    Guid id,
    BusinessId businessId,
    BranchId branchId,
    Guid createdByUserId,
    DateOnly closingDate,
    // Sales
    decimal totalSales,
    decimal cashSales,
    decimal transferSales,
    decimal cardSales,
    decimal creditSales,
    int salesCount,
    // Cash
    decimal cashExpected,
    // Expenses
    decimal totalExpenses,
    // Cost / profitability
    decimal totalCost,
    decimal grossProfit,
    decimal estimatedNetProfit,
    decimal grossMarginPercent,
    decimal netMarginPercent,
    // Credits (business-wide for date)
    decimal newCreditsAmount,
    int newCreditsCount,
    decimal creditPaymentsReceived,
    // Notes
    string? notes,
    DateTimeOffset createdAt)
  {
    if (id == Guid.Empty)
    {
      throw new ArgumentException("DailyClosing id is required.", nameof(id));
    }

    if (createdByUserId == Guid.Empty)
    {
      throw new ArgumentException("User id is required.", nameof(createdByUserId));
    }

    Id = id;
    BusinessId = businessId;
    BranchId = branchId;
    CreatedByUserId = createdByUserId;
    ClosingDate = closingDate;
    Status = DailyClosingStatus.Draft;
    TotalSales = totalSales;
    CashSales = cashSales;
    TransferSales = transferSales;
    CardSales = cardSales;
    CreditSales = creditSales;
    SalesCount = salesCount;
    CashExpected = cashExpected;
    CashCounted = null;
    CashDifference = null;
    TotalExpenses = totalExpenses;
    TotalCost = totalCost;
    GrossProfit = grossProfit;
    EstimatedNetProfit = estimatedNetProfit;
    GrossMarginPercent = grossMarginPercent;
    NetMarginPercent = netMarginPercent;
    NewCreditsAmount = newCreditsAmount;
    NewCreditsCount = newCreditsCount;
    CreditPaymentsReceived = creditPaymentsReceived;
    Notes = notes?.Trim();
    CreatedAt = createdAt;
    UpdatedAt = createdAt;
  }

  public Guid Id { get; private set; }

  public BusinessId BusinessId { get; private set; }

  public BranchId BranchId { get; private set; }

  public Guid CreatedByUserId { get; private set; }

  public DateOnly ClosingDate { get; private set; }

  public DailyClosingStatus Status { get; private set; }

  // ── Sales by payment method ──────────────────────────────────────────────────
  public decimal TotalSales { get; private set; }

  public decimal CashSales { get; private set; }

  public decimal TransferSales { get; private set; }

  public decimal CardSales { get; private set; }

  public decimal CreditSales { get; private set; }

  public int SalesCount { get; private set; }

  // ── Cash register ───────────────────────────────────────────────────────────
  public decimal CashExpected { get; private set; }

  public decimal? CashCounted { get; private set; }

  public decimal? CashDifference { get; private set; }

  // ── Expenses ────────────────────────────────────────────────────────────────
  public decimal TotalExpenses { get; private set; }

  // ── Profitability ───────────────────────────────────────────────────────────
  public decimal TotalCost { get; private set; }

  public decimal GrossProfit { get; private set; }

  public decimal EstimatedNetProfit { get; private set; }

  public decimal GrossMarginPercent { get; private set; }

  public decimal NetMarginPercent { get; private set; }

  // ── Credits (business-wide) ──────────────────────────────────────────────────
  public decimal NewCreditsAmount { get; private set; }

  public int NewCreditsCount { get; private set; }

  public decimal CreditPaymentsReceived { get; private set; }

  // ── Metadata ─────────────────────────────────────────────────────────────────
  public string? Notes { get; private set; }

  public DateTimeOffset CreatedAt { get; private set; }

  public DateTimeOffset? ClosedAt { get; private set; }

  public Guid? ClosedByUserId { get; private set; }

  public DateTimeOffset UpdatedAt { get; private set; }

  public IReadOnlyCollection<DailyClosingAlert> Alerts => alerts.AsReadOnly();

  // ── Factory ──────────────────────────────────────────────────────────────────

  public static DailyClosing Create(
    Guid id,
    BusinessId businessId,
    BranchId branchId,
    Guid createdByUserId,
    DateOnly closingDate,
    decimal totalSales,
    decimal cashSales,
    decimal transferSales,
    decimal cardSales,
    decimal creditSales,
    int salesCount,
    decimal cashExpected,
    decimal totalExpenses,
    decimal totalCost,
    decimal grossProfit,
    decimal estimatedNetProfit,
    decimal grossMarginPercent,
    decimal netMarginPercent,
    decimal newCreditsAmount,
    int newCreditsCount,
    decimal creditPaymentsReceived,
    string? notes,
    DateTimeOffset createdAt)
    => new(
      id, businessId, branchId, createdByUserId, closingDate,
      totalSales, cashSales, transferSales, cardSales, creditSales, salesCount,
      cashExpected, totalExpenses, totalCost, grossProfit, estimatedNetProfit,
      grossMarginPercent, netMarginPercent, newCreditsAmount, newCreditsCount,
      creditPaymentsReceived, notes, createdAt);

  // ── Behaviour ────────────────────────────────────────────────────────────────

  public void AddAlert(DailyClosingAlert alert)
  {
    ArgumentNullException.ThrowIfNull(alert);

    if (Status != DailyClosingStatus.Draft)
    {
      throw new InvalidOperationException("Alerts can only be added to a Draft closing.");
    }

    alerts.Add(alert);
  }

  /// <summary>
  /// Transitions the closing from Draft to Closed, recording the physical cash count.
  /// </summary>
  public void Close(
    Guid closedByUserId,
    decimal cashCounted,
    string? notes,
    DateTimeOffset closedAt)
  {
    if (closedByUserId == Guid.Empty)
    {
      throw new ArgumentException("ClosedBy user id is required.", nameof(closedByUserId));
    }

    if (cashCounted < 0)
    {
      throw new ArgumentException("Cash counted cannot be negative.", nameof(cashCounted));
    }

    if (Status != DailyClosingStatus.Draft)
    {
      throw new InvalidOperationException("Only a Draft closing can be closed.");
    }

    Status = DailyClosingStatus.Closed;
    CashCounted = cashCounted;
    CashDifference = cashCounted - CashExpected;
    ClosedByUserId = closedByUserId;
    ClosedAt = closedAt;
    Notes = notes?.Trim() ?? Notes;
    UpdatedAt = closedAt;
  }
}
