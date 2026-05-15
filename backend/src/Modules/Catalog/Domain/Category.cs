using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Catalog.Domain;

public sealed class Category
{
  private Category()
  {
  }

  public Category(Guid id, BusinessId businessId, string name, DateTimeOffset createdAt)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(name);

    Id = id;
    BusinessId = businessId;
    Name = name.Trim();
    CreatedAt = createdAt;
    IsActive = true;
  }

  public Guid Id { get; private set; }

  public BusinessId BusinessId { get; private set; }

  public string Name { get; private set; } = string.Empty;

  public bool IsActive { get; private set; }

  public DateTimeOffset CreatedAt { get; private set; }
}
