using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Domain;

public sealed class ExpenseCategory
{
  private ExpenseCategory()
  {
  }

  private ExpenseCategory(
    Guid id,
    BusinessId businessId,
    string name,
    DateTimeOffset createdAt)
  {
    if (id == Guid.Empty)
    {
      throw new ArgumentException("Category id is required.", nameof(id));
    }

    ArgumentException.ThrowIfNullOrWhiteSpace(name);

    Id = id;
    BusinessId = businessId;
    Name = name.Trim();
    IsActive = true;
    CreatedAt = createdAt;
    UpdatedAt = createdAt;
  }

  public Guid Id { get; private set; }

  public BusinessId BusinessId { get; private set; }

  public string Name { get; private set; } = string.Empty;

  public bool IsActive { get; private set; }

  public DateTimeOffset CreatedAt { get; private set; }

  public DateTimeOffset UpdatedAt { get; private set; }

  public static ExpenseCategory Create(
    Guid id,
    BusinessId businessId,
    string name,
    DateTimeOffset createdAt)
    => new(id, businessId, name, createdAt);

  public void Activate(DateTimeOffset updatedAt)
  {
    IsActive = true;
    UpdatedAt = updatedAt;
  }

  public void Deactivate(DateTimeOffset updatedAt)
  {
    IsActive = false;
    UpdatedAt = updatedAt;
  }
}
