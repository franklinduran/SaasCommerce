using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Customers.Domain.Credits;

public sealed class CustomerCreditMovement
{
  private CustomerCreditMovement()
  {
  }

  public CustomerCreditMovement( // NOSONAR S107 — credit movement requires full audit trail of balance transition
    Guid id,
    BusinessId businessId,
    Guid customerId,
    Guid? saleId,
    Guid? paymentId,
    CustomerCreditMovementType type,
    decimal amount,
    decimal previousBalance,
    decimal newBalance,
    string? note,
    DateTimeOffset createdAt,
    Guid? createdBy)
  {
    if (id == Guid.Empty)
    {
      throw new ArgumentException("Credit movement id is required.", nameof(id));
    }

    if (customerId == Guid.Empty)
    {
      throw new ArgumentException("Customer id is required.", nameof(customerId));
    }

    if (amount <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be greater than zero.");
    }

    if (previousBalance < 0 || newBalance < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(newBalance), "Balances cannot be negative.");
    }

    if (!HasConsistentBalance(type, amount, previousBalance, newBalance))
    {
      throw new ArgumentException("Movement balances are inconsistent.", nameof(newBalance));
    }

    Id = id;
    BusinessId = businessId;
    CustomerId = customerId;
    SaleId = saleId;
    PaymentId = paymentId;
    Type = type;
    Amount = amount;
    PreviousBalance = previousBalance;
    NewBalance = newBalance;
    Note = NormalizeNote(note);
    CreatedAt = createdAt;
    CreatedBy = createdBy;
  }

  public Guid Id { get; private set; }

  public BusinessId BusinessId { get; private set; }

  public Guid CustomerId { get; private set; }

  public Guid? SaleId { get; private set; }

  public Guid? PaymentId { get; private set; }

  public CustomerCreditMovementType Type { get; private set; }

  public decimal Amount { get; private set; }

  public decimal PreviousBalance { get; private set; }

  public decimal NewBalance { get; private set; }

  public string? Note { get; private set; }

  public DateTimeOffset CreatedAt { get; private set; }

  public Guid? CreatedBy { get; private set; }

  private static bool HasConsistentBalance(
    CustomerCreditMovementType type,
    decimal amount,
    decimal previousBalance,
    decimal newBalance)
    => type switch
    {
      CustomerCreditMovementType.Debit => newBalance == previousBalance + amount,
      CustomerCreditMovementType.Payment => newBalance == previousBalance - amount,
      CustomerCreditMovementType.Cancellation => newBalance == previousBalance - amount,
      CustomerCreditMovementType.Adjustment => true,
      _ => false
    };

  private static string? NormalizeNote(string? note)
  {
    if (string.IsNullOrWhiteSpace(note))
    {
      return null;
    }

    var trimmed = note.Trim();
    return trimmed.Length <= CustomerCreditRules.NoteMaxLength
      ? trimmed
      : trimmed[..CustomerCreditRules.NoteMaxLength];
  }
}
