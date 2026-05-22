using SaasCommerce.Modules.Tenancy.Contracts.Responses;
using SaasCommerce.Modules.Tenancy.Domain;

namespace SaasCommerce.Modules.Tenancy.Application.Branches;

public static class BranchResponseMapper
{
  public static BranchResponse ToResponse(Branch branch)
  {
    ArgumentNullException.ThrowIfNull(branch);

    return new BranchResponse(
      branch.Id.Value,
      branch.BusinessId.Value,
      branch.Name,
      branch.Code,
      branch.Address,
      branch.Phone,
      branch.IsMain,
      branch.IsActive,
      branch.CreatedAt,
      branch.UpdatedAt);
  }
}
