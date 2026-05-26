using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Domain;

public sealed class OperatingExpenseDraft
{
  public required Guid Id { get; init; }
  public required BusinessId BusinessId { get; init; }
  public required BranchId BranchId { get; init; }
  public required Guid UserId { get; init; }
  public required Guid CategoryId { get; init; }
  public required string Description { get; init; }
  public required decimal Amount { get; init; }
  public required ExpensePaymentMethod PaymentMethod { get; init; }
  public required DateTimeOffset ExpenseDate { get; init; }
  public string? Notes { get; init; }
  public Guid? CashSessionId { get; init; }
  public Guid? CashMovementId { get; init; }
  public required DateTimeOffset CreatedAt { get; init; }
}

public sealed class OperatingExpense
{
  private OperatingExpense()
  {
  }

  private OperatingExpense(OperatingExpenseDraft draft, OperatingExpenseStatus status)
  {
    if (draft.Id == Guid.Empty)
    {
      throw new ArgumentException("Expense id is required.", nameof(draft));
    }

    if (draft.UserId == Guid.Empty)
    {
      throw new ArgumentException("User id is required.", nameof(draft));
    }

    if (draft.CategoryId == Guid.Empty)
    {
      throw new ArgumentException("Category id is required.", nameof(draft));
    }

    ArgumentException.ThrowIfNullOrWhiteSpace(draft.Description);

    if (draft.Amount <= 0)
    {
      throw new ArgumentException("Amount must be greater than zero.", nameof(draft));
    }

    Id = draft.Id;
    BusinessId = draft.BusinessId;
    BranchId = draft.BranchId;
    UserId = draft.UserId;
    CategoryId = draft.CategoryId;
    Description = draft.Description.Trim();
    Amount = draft.Amount;
    PaymentMethod = draft.PaymentMethod;
    Status = status;
    ExpenseDate = draft.ExpenseDate;
    Notes = draft.Notes?.Trim();
    CashSessionId = draft.CashSessionId;
    CashMovementId = draft.CashMovementId;
    CreatedAt = draft.CreatedAt;
    UpdatedAt = draft.CreatedAt;

    if (status == OperatingExpenseStatus.Paid)
    {
      PaidAt = draft.CreatedAt;
      PaidByUserId = draft.UserId;
    }
  }

  public Guid Id { get; private set; }

  public BusinessId BusinessId { get; private set; }

  public BranchId BranchId { get; private set; }

  public Guid UserId { get; private set; }

  public Guid CategoryId { get; private set; }

  public string Description { get; private set; } = string.Empty;

  public decimal Amount { get; private set; }

  public ExpensePaymentMethod PaymentMethod { get; private set; }

  public OperatingExpenseStatus Status { get; private set; }

  public DateTimeOffset ExpenseDate { get; private set; }

  public string? Notes { get; private set; }

  /// <summary>Set when the expense is paid with cash — links to the open cash session.</summary>
  public Guid? CashSessionId { get; private set; }

  /// <summary>Set when the expense is paid with cash — links to the generated CashOut movement.</summary>
  public Guid? CashMovementId { get; private set; }

  public DateTimeOffset? PaidAt { get; private set; }

  public Guid? PaidByUserId { get; private set; }

  public DateTimeOffset? CancelledAt { get; private set; }

  public DateTimeOffset CreatedAt { get; private set; }

  public DateTimeOffset UpdatedAt { get; private set; }

  /// <summary>Creates an expense already in Paid status (e.g., immediate cash payment).</summary>
  public static OperatingExpense CreatePaid(OperatingExpenseDraft draft)
    => new(draft, OperatingExpenseStatus.Paid);

  /// <summary>Creates a pending expense (payment not yet registered).</summary>
  public static OperatingExpense CreatePending(OperatingExpenseDraft draft)
    => new(draft, OperatingExpenseStatus.Pending);

  /// <summary>Marks a pending expense as paid. Returns false if already paid (idempotent).</summary>
  public bool Pay(
    Guid paidByUserId,
    DateTimeOffset paidAt,
    Guid? cashSessionId = null,
    Guid? cashMovementId = null)
  {
    if (Status == OperatingExpenseStatus.Paid)
    {
      return false; // already paid — idempotent
    }

    if (Status == OperatingExpenseStatus.Cancelled)
    {
      throw new InvalidOperationException("Cannot pay a cancelled expense.");
    }

    Status = OperatingExpenseStatus.Paid;
    PaidAt = paidAt;
    PaidByUserId = paidByUserId;
    CashSessionId = cashSessionId;
    CashMovementId = cashMovementId;
    UpdatedAt = paidAt;
    return true;
  }

  /// <summary>Cancels the expense. Cannot cancel if already paid.</summary>
  public void Cancel(DateTimeOffset cancelledAt)
  {
    if (Status == OperatingExpenseStatus.Paid)
    {
      throw new InvalidOperationException("Cannot cancel a paid expense.");
    }

    if (Status == OperatingExpenseStatus.Cancelled)
    {
      return; // already cancelled — idempotent
    }

    Status = OperatingExpenseStatus.Cancelled;
    CancelledAt = cancelledAt;
    UpdatedAt = cancelledAt;
  }
}
