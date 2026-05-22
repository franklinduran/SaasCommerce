using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Tenancy.Contracts.Responses;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Tenancy.Application.Branches;

public sealed record GetBranchByIdQuery(Guid BranchId);

public sealed class GetBranchByIdHandler(
  IBranchRepository repository,
  ICurrentUserService currentUser)
{
  public Task<Result<BranchResponse>> Handle(
    GetBranchByIdQuery query,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(query);

    return HandleCoreAsync(query, cancellationToken);
  }

  private async Task<Result<BranchResponse>> HandleCoreAsync(
    GetBranchByIdQuery query,
    CancellationToken cancellationToken)
  {
    if (currentUser.BusinessId is not Guid businessId)
    {
      return Result.Failure<BranchResponse>(BranchErrors.UserContextRequired);
    }

    var tenantId = new BusinessId(businessId);
    var branchId = new BranchId(query.BranchId);

    var branch = await repository.GetByIdAsync(tenantId, branchId, cancellationToken);

    if (branch is null)
    {
      return Result.Failure<BranchResponse>(BranchErrors.NotFound);
    }

    return Result.Success(BranchResponseMapper.ToResponse(branch));
  }
}
