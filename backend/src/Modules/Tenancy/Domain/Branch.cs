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
    IsMain = isMain;
    IsActive = true;
  }

  public BranchId Id { get; private set; }

  public BusinessId BusinessId { get; private set; }

  public string Name { get; private set; } = string.Empty;

  public bool IsMain { get; private set; }

  public bool IsActive { get; private set; }

  public DateTimeOffset CreatedAt { get; private set; }
}
