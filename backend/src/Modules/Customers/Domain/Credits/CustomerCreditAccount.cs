using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Customers.Domain.Credits;

public sealed class CustomerCreditAccount
{
  private CustomerCreditAccount()
  {
  }

  public CustomerCreditAccount(
    Guid id,
    BusinessId businessId,
    Guid customerId,
    decimal creditLimit,
    DateTimeOffset createdAt)
  {
    if (id == Guid.Empty)
    {
      throw new ArgumentException("Credit account id is required.", nameof(id));
    }

    if (customerId == Guid.Empty)
    {
      throw new ArgumentException("Customer id is required.", nameof(customerId));
    }

    if (creditLimit < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(creditLimit), "Credit limit cannot be negative.");
    }

    Id = id;
    BusinessId = businessId;
    CustomerId = customerId;
    CreditLimit = creditLimit;
    CurrentBalance = 0;
    Status = CustomerCreditStatus.Active;
    CreatedAt = createdAt;
    UpdatedAt = createdAt;
  }

  public Guid Id { get; private set; }

  public BusinessId BusinessId { get; private set; }

  public Guid CustomerId { get; private set; }

  public decimal CreditLimit { get; private set; }

  public decimal CurrentBalance { get; private set; }

  public CustomerCreditStatus Status { get; private set; }

  public DateTimeOffset CreatedAt { get; private set; }

  public DateTimeOffset UpdatedAt { get; private set; }

  public bool HasUnlimitedCredit => CreditLimit == 0;

  public void EnsureCanDebit(decimal amount)
  {
    EnsurePositiveAmount(amount);

    if (Status is CustomerCreditStatus.Blocked or CustomerCreditStatus.Closed)
    {
      throw new InvalidOperationException("Credit account does not allow new debit movements.");
    }

    var nextBalance = CurrentBalance + amount;

    if (!HasUnlimitedCredit && nextBalance > CreditLimit)
    {
      throw new InvalidOperationException("Credit limit exceeded.");
    }
  }

  public CustomerCreditMovement ApplyDebit(
    Guid movementId,
    Guid saleId,
    decimal amount,
    string? note,
    Guid? createdBy,
    DateTimeOffset createdAt)
  {
    if (saleId == Guid.Empty)
    {
      throw new ArgumentException("Sale id is required.", nameof(saleId));
    }

    EnsureCanDebit(amount);

    var previousBalance = CurrentBalance;
    var newBalance = previousBalance + amount;
    CurrentBalance = newBalance;
    UpdatedAt = createdAt;

    return new CustomerCreditMovement(
      movementId,
      BusinessId,
      CustomerId,
      saleId,
      null,
      CustomerCreditMovementType.Debit,
      amount,
      previousBalance,
      newBalance,
      note,
      createdAt,
      createdBy);
  }

  public CustomerCreditMovement ApplyPayment(
    Guid movementId,
    Guid paymentId,
    decimal amount,
    string? note,
    Guid? createdBy,
    DateTimeOffset createdAt)
  {
    if (paymentId == Guid.Empty)
    {
      throw new ArgumentException("Payment id is required.", nameof(paymentId));
    }

    EnsurePositiveAmount(amount);

    if (amount > CurrentBalance)
    {
      throw new InvalidOperationException("Payment amount exceeds the current balance.");
    }

    var previousBalance = CurrentBalance;
    var newBalance = previousBalance - amount;
    CurrentBalance = newBalance;
    UpdatedAt = createdAt;

    return new CustomerCreditMovement(
      movementId,
      BusinessId,
      CustomerId,
      null,
      paymentId,
      CustomerCreditMovementType.Payment,
      amount,
      previousBalance,
      newBalance,
      note,
      createdAt,
      createdBy);
  }

  public void Block(DateTimeOffset updatedAt)
  {
    if (Status == CustomerCreditStatus.Closed)
    {
      throw new InvalidOperationException("Closed credit accounts cannot be blocked.");
    }

    Status = CustomerCreditStatus.Blocked;
    UpdatedAt = updatedAt;
  }

  public void Unblock(DateTimeOffset updatedAt)
  {
    if (Status == CustomerCreditStatus.Closed)
    {
      throw new InvalidOperationException("Closed credit accounts cannot be unblocked.");
    }

    Status = CustomerCreditStatus.Active;
    UpdatedAt = updatedAt;
  }

  private static void EnsurePositiveAmount(decimal amount)
  {
    if (amount <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be greater than zero.");
    }
  }
}
