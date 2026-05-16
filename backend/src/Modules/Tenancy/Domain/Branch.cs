using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Tenancy.Domain;

public sealed class Branch
{
  private Branch()
  {
  }

  public Branch(
    BranchId id,
    BusinessId businessId,
    string name,
    DateTimeOffset createdAt,
    bool isMain)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(name);

    Id = id;
    BusinessId = businessId;
    Name = name.Trim();
    CreatedAt = createdAt;
    UpdatedAt = createdAt;
    IsMain = isMain;
    IsActive = true;
  }

  public BranchId Id { get; private set; }

  public BusinessId BusinessId { get; private set; }

  public string Name { get; private set; } = string.Empty;

  public string? Address { get; private set; }

  public string? Phone { get; private set; }

  public bool IsMain { get; private set; }

  public bool IsActive { get; private set; }

  public DateTimeOffset CreatedAt { get; private set; }

  public DateTimeOffset UpdatedAt { get; private set; }

  public void Update(string name, string? address, string? phone, DateTimeOffset updatedAt)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(name);

    Name = name.Trim();
    Address = NormalizeOptional(address);
    Phone = NormalizeOptional(phone);
    UpdatedAt = updatedAt;
  }

  private static string? NormalizeOptional(string? value)
    => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
