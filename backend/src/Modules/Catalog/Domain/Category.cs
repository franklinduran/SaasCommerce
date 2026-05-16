using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Catalog.Domain;

public sealed class Category
{
  private Category()
  {
  }

  public Category(
    Guid id,
    BusinessId businessId,
    string name,
    string? description,
    DateTimeOffset createdAt)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(name);

    Id = id;
    BusinessId = businessId;
    Name = name.Trim();
    Description = NormalizeDescription(description);
    CreatedAt = createdAt;
    IsActive = true;
  }

  public Guid Id { get; private set; }

  public BusinessId BusinessId { get; private set; }

  public string Name { get; private set; } = string.Empty;

  public string? Description { get; private set; }

  public bool IsActive { get; private set; }

  public DateTimeOffset CreatedAt { get; private set; }

  public DateTimeOffset? UpdatedAt { get; private set; }

  public void Update(string name, string? description, bool isActive, DateTimeOffset updatedAt)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(name);

    Name = name.Trim();
    Description = NormalizeDescription(description);
    IsActive = isActive;
    UpdatedAt = updatedAt;
  }

  private static string? NormalizeDescription(string? description)
    => string.IsNullOrWhiteSpace(description) ? null : description.Trim();
}
