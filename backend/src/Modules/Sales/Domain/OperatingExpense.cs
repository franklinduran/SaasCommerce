using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Domain;

public sealed class OperatingExpense
{
  private OperatingExpense()
  {
  }

  private OperatingExpense(
    Guid id,
    BusinessId businessId,
    BranchId branchId,
    Guid userId,
    Guid categoryId,
    string description,
    decimal amount,
    ExpensePaymentMethod paymentMethod,
    OperatingExpenseStatus status,
    DateTimeOffset expenseDate,
    string? notes,
    Guid? cashSessionId,
    Guid? cashMovementId,
    DateTimeOffset createdAt)
  {
    if (id == Guid.Empty)
    {
      throw new ArgumentException("Expense id is required.", nameof(id));
    }

    if (userId == Guid.Empty)
    {
      throw new ArgumentException("User id is required.", nameof(userId));
    }

    if (categoryId == Guid.Empty)
    {
      throw new ArgumentException("Category id is required.", nameof(categoryId));
    }

    ArgumentException.ThrowIfNullOrWhiteSpace(description);

    if (amount <= 0)
    {
      throw new ArgumentException("Amount must be greater than zero.", nameof(amount));
    }

    Id = id;
    BusinessId = businessId;
    BranchId = branchId;
    UserId = userId;
    CategoryId = categoryId;
    Description = description.Trim();
    Amount = amount;
    PaymentMethod = paymentMethod;
    Status = status;
    ExpenseDate = expenseDate;
    Notes = notes?.Trim();
    CashSessionId = cashSessionId;
    CashMovementId = cashMovementId;
    CreatedAt = createdAt;
    UpdatedAt = createdAt;

    if (status == OperatingExpenseStatus.Paid)
    {
      PaidAt = createdAt;
      PaidByUserId = userId;
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
  public static OperatingExpense CreatePaid(
    Guid id,
    BusinessId businessId,
    BranchId branchId,
    Guid userId,
    Guid categoryId,
    string description,
    decimal amount,
    ExpensePaymentMethod paymentMethod,
    DateTimeOffset expenseDate,
    string? notes,
    Guid? cashSessionId,
    Guid? cashMovementId,
    DateTimeOffset createdAt)
    => new(id, businessId, branchId, userId, categoryId, description, amount,
      paymentMethod, OperatingExpenseStatus.Paid, expenseDate, notes,
      cashSessionId, cashMovementId, createdAt);

  /// <summary>Creates a pending expense (payment not yet registered).</summary>
  public static OperatingExpense CreatePending(
    Guid id,
    BusinessId businessId,
    BranchId branchId,
    Guid userId,
    Guid categoryId,
    string description,
    decimal amount,
    ExpensePaymentMethod paymentMethod,
    DateTimeOffset expenseDate,
    string? notes,
    DateTimeOffset createdAt)
    => new(id, businessId, branchId, userId, categoryId, description, amount,
      paymentMethod, OperatingExpenseStatus.Pending, expenseDate, notes,
      null, null, createdAt);

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
