namespace SaasCommerce.Modules.Sales.Domain;

public sealed class CashMovement
{
  private CashMovement()
  {
  }

  private CashMovement(
    Guid id,
    Guid cashSessionId,
    Guid userId,
    CashMovementType type,
    decimal amount,
    string description,
    DateTimeOffset createdAt)
  {
    if (id == Guid.Empty)
    {
      throw new ArgumentException("Cash movement id is required.", nameof(id));
    }

    if (cashSessionId == Guid.Empty)
    {
      throw new ArgumentException("Cash session id is required.", nameof(cashSessionId));
    }

    if (userId == Guid.Empty)
    {
      throw new ArgumentException("User id is required.", nameof(userId));
    }

    if (amount <= 0)
    {
      throw new ArgumentException("Amount must be greater than zero.", nameof(amount));
    }

    ArgumentException.ThrowIfNullOrWhiteSpace(description);

    Id = id;
    CashSessionId = cashSessionId;
    UserId = userId;
    Type = type;
    Amount = amount;
    Description = description.Trim();
    CreatedAt = createdAt;
  }

  public Guid Id { get; private set; }

  public Guid CashSessionId { get; private set; }

  public Guid UserId { get; private set; }

  public CashMovementType Type { get; private set; }

  public decimal Amount { get; private set; }

  public string Description { get; private set; } = string.Empty;

  public DateTimeOffset CreatedAt { get; private set; }

  public static CashMovement Create(
    Guid id,
    Guid cashSessionId,
    Guid userId,
    CashMovementType type,
    decimal amount,
    string description,
    DateTimeOffset createdAt)
    => new(id, cashSessionId, userId, type, amount, description, createdAt);
}
