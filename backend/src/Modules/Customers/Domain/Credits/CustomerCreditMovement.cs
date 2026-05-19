using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Customers.Domain.Credits;

public sealed record CreditMovementBalance(decimal PreviousBalance, decimal NewBalance);

public sealed record CreditMovementSource(Guid? SaleId = null, Guid? PaymentId = null);

public sealed record CreditMovementIdentifiers(Guid Id, BusinessId BusinessId, Guid CustomerId, CreditMovementSource Source);

public sealed class CustomerCreditMovement
{
  private CustomerCreditMovement()
  {
  }

  public CustomerCreditMovement(
    CreditMovementIdentifiers ids,
    CustomerCreditMovementType type,
    decimal amount,
    CreditMovementBalance balance,
    string? note,
    DateTimeOffset createdAt,
    Guid? createdBy)
  {
    ArgumentNullException.ThrowIfNull(ids);
    ArgumentNullException.ThrowIfNull(balance);

    if (ids.Id == Guid.Empty)
    {
      throw new ArgumentException("Credit movement id is required.", nameof(ids));
    }

    if (ids.CustomerId == Guid.Empty)
    {
      throw new ArgumentException("Customer id is required.", nameof(ids));
    }

    if (amount <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be greater than zero.");
    }

    if (balance.PreviousBalance < 0 || balance.NewBalance < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(balance), "Balances cannot be negative.");
    }

    if (!HasConsistentBalance(type, amount, balance.PreviousBalance, balance.NewBalance))
    {
      throw new ArgumentException("Movement balances are inconsistent.", nameof(balance));
    }

    Id = ids.Id;
    BusinessId = ids.BusinessId;
    CustomerId = ids.CustomerId;
    SaleId = ids.Source?.SaleId;
    PaymentId = ids.Source?.PaymentId;
    Type = type;
    Amount = amount;
    PreviousBalance = balance.PreviousBalance;
    NewBalance = balance.NewBalance;
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
