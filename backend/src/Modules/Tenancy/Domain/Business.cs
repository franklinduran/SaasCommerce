using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Tenancy.Domain;

public sealed class Business
{
  private readonly List<Branch> branches = [];

  private Business()
  {
  }

  public Business(BusinessId id, string name, DateTimeOffset createdAt)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(name);

    Id = id;
    Name = name.Trim();
    CreatedAt = createdAt;
    IsActive = true;
  }

  public BusinessId Id { get; private set; }

  public string Name { get; private set; } = string.Empty;

  public bool IsActive { get; private set; }

  public DateTimeOffset CreatedAt { get; private set; }

  public IReadOnlyCollection<Branch> Branches => branches.AsReadOnly();

  public Branch AddBranch(BranchId branchId, string name, DateTimeOffset createdAt, bool isMain = false)
  {
    var branch = new Branch(branchId, Id, name, createdAt, isMain);

    branches.Add(branch);

    return branch;
  }
}
