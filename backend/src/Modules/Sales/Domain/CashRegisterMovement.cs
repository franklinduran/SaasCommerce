namespace SaasCommerce.Modules.Sales.Domain;

public sealed class CashRegisterMovement
{
  private CashRegisterMovement()
  {
  }

  private CashRegisterMovement(
    Guid id,
    Guid cashRegisterId,
    Guid userId,
    CashMovementType movementType,
    decimal amount,
    string reason,
    DateTimeOffset createdAt)
  {
    if (id == Guid.Empty)
    {
      throw new ArgumentException("Movement id is required.", nameof(id));
    }

    if (cashRegisterId == Guid.Empty)
    {
      throw new ArgumentException("Cash register id is required.", nameof(cashRegisterId));
    }

    if (userId == Guid.Empty)
    {
      throw new ArgumentException("User id is required.", nameof(userId));
    }

    if (amount <= 0)
    {
      throw new ArgumentException("Amount must be greater than zero.", nameof(amount));
    }

    ArgumentException.ThrowIfNullOrWhiteSpace(reason);

    Id = id;
    CashRegisterId = cashRegisterId;
    UserId = userId;
    MovementType = movementType;
    Amount = amount;
    Reason = reason.Trim();
    CreatedAt = createdAt;
  }

  public Guid Id { get; private set; }

  public Guid CashRegisterId { get; private set; }

  public Guid UserId { get; private set; }

  public CashMovementType MovementType { get; private set; }

  public decimal Amount { get; private set; }

  public string Reason { get; private set; } = string.Empty;

  public DateTimeOffset CreatedAt { get; private set; }

  public static CashRegisterMovement Create(
    Guid id,
    Guid cashRegisterId,
    Guid userId,
    CashMovementType movementType,
    decimal amount,
    string reason,
    DateTimeOffset createdAt)
    => new(id, cashRegisterId, userId, movementType, amount, reason, createdAt);
}
