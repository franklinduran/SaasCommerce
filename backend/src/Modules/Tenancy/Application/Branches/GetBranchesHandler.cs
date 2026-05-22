using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Tenancy.Contracts.Responses;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Tenancy.Application.Branches;

public sealed class GetBranchesHandler(
  IBranchRepository repository,
  ICurrentUserService currentUser)
{
  public Task<Result<BranchListResponse>> Handle(
    GetBranchesQuery query,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(query);

    return HandleCoreAsync(query, cancellationToken);
  }

  private async Task<Result<BranchListResponse>> HandleCoreAsync(
    GetBranchesQuery query,
    CancellationToken cancellationToken)
  {
    if (currentUser.BusinessId is not Guid businessId)
    {
      return Result.Failure<BranchListResponse>(BranchErrors.UserContextRequired);
    }

    var tenantId = new BusinessId(businessId);
    var branches = await repository.ListAsync(tenantId, query.IsActive, cancellationToken);

    var items = branches.Select(BranchResponseMapper.ToResponse).ToArray();

    return Result.Success(new BranchListResponse(items, items.Length));
  }
}
