using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Identity.Domain;

public sealed class Role
{
  private Role()
  {
  }

  public Role(Guid id, BusinessId businessId, string name)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(name);

    Id = id;
    BusinessId = businessId;
    Name = name.Trim();
  }

  public Guid Id { get; private set; }

  public BusinessId BusinessId { get; private set; }

  public string Name { get; private set; } = string.Empty;
}
