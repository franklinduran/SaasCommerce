using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Customers.Domain.Credits;

public sealed class CustomerPayment
{
  private CustomerPayment()
  {
  }

  public CustomerPayment(
    Guid id,
    BusinessId businessId,
    Guid customerId,
    decimal amount,
    string? note,
    DateTimeOffset createdAt,
    Guid? createdBy)
  {
    if (id == Guid.Empty)
    {
      throw new ArgumentException("Payment id is required.", nameof(id));
    }

    if (customerId == Guid.Empty)
    {
      throw new ArgumentException("Customer id is required.", nameof(customerId));
    }

    if (amount <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(amount), "Payment amount must be greater than zero.");
    }

    Id = id;
    BusinessId = businessId;
    CustomerId = customerId;
    Amount = amount;
    Note = NormalizeNote(note);
    CreatedAt = createdAt;
    CreatedBy = createdBy;
  }

  public Guid Id { get; private set; }

  public BusinessId BusinessId { get; private set; }

  public Guid CustomerId { get; private set; }

  public decimal Amount { get; private set; }

  public string? Note { get; private set; }

  public DateTimeOffset CreatedAt { get; private set; }

  public Guid? CreatedBy { get; private set; }

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
