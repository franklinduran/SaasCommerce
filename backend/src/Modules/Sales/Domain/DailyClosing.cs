using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Domain;

public sealed class DailyClosingDraft
{
  public required Guid Id { get; init; }
  public required BusinessId BusinessId { get; init; }
  public required BranchId BranchId { get; init; }
  public required Guid CreatedByUserId { get; init; }
  public required DateOnly ClosingDate { get; init; }
  public required decimal TotalSales { get; init; }
  public required decimal CashSales { get; init; }
  public required decimal TransferSales { get; init; }
  public required decimal CardSales { get; init; }
  public required decimal CreditSales { get; init; }
  public required int SalesCount { get; init; }
  public required decimal CashExpected { get; init; }
  public required decimal TotalExpenses { get; init; }
  public required decimal TotalCost { get; init; }
  public required decimal GrossProfit { get; init; }
  public required decimal EstimatedNetProfit { get; init; }
  public required decimal GrossMarginPercent { get; init; }
  public required decimal NetMarginPercent { get; init; }
  public required decimal NewCreditsAmount { get; init; }
  public required int NewCreditsCount { get; init; }
  public required decimal CreditPaymentsReceived { get; init; }
  public string? Notes { get; init; }
  public required DateTimeOffset CreatedAt { get; init; }
}

public sealed class DailyClosing
{
  private readonly List<DailyClosingAlert> alerts = [];

  private DailyClosing()
  {
  }

  private DailyClosing(DailyClosingDraft draft)
  {
    if (draft.Id == Guid.Empty)
    {
      throw new ArgumentException("DailyClosing id is required.", nameof(draft));
    }

    if (draft.CreatedByUserId == Guid.Empty)
    {
      throw new ArgumentException("User id is required.", nameof(draft));
    }

    Id = draft.Id;
    BusinessId = draft.BusinessId;
    BranchId = draft.BranchId;
    CreatedByUserId = draft.CreatedByUserId;
    ClosingDate = draft.ClosingDate;
    Status = DailyClosingStatus.Draft;
    TotalSales = draft.TotalSales;
    CashSales = draft.CashSales;
    TransferSales = draft.TransferSales;
    CardSales = draft.CardSales;
    CreditSales = draft.CreditSales;
    SalesCount = draft.SalesCount;
    CashExpected = draft.CashExpected;
    CashCounted = null;
    CashDifference = null;
    TotalExpenses = draft.TotalExpenses;
    TotalCost = draft.TotalCost;
    GrossProfit = draft.GrossProfit;
    EstimatedNetProfit = draft.EstimatedNetProfit;
    GrossMarginPercent = draft.GrossMarginPercent;
    NetMarginPercent = draft.NetMarginPercent;
    NewCreditsAmount = draft.NewCreditsAmount;
    NewCreditsCount = draft.NewCreditsCount;
    CreditPaymentsReceived = draft.CreditPaymentsReceived;
    Notes = draft.Notes?.Trim();
    CreatedAt = draft.CreatedAt;
    UpdatedAt = draft.CreatedAt;
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

  public static DailyClosing Create(DailyClosingDraft draft)
    => new(draft);

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
