using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Tenancy.Contracts.Responses;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Tenancy.Application.Branches;

public sealed record ActivateBranchCommand(Guid BranchId);

public sealed class ActivateBranchHandler(
  IBranchRepository repository,
  ICurrentUserService currentUser,
  IClock clock,
  IUnitOfWork unitOfWork)
{
  public Task<Result<BranchResponse>> Handle(
    ActivateBranchCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    return HandleCoreAsync(command, cancellationToken);
  }

  private async Task<Result<BranchResponse>> HandleCoreAsync(
    ActivateBranchCommand command,
    CancellationToken cancellationToken)
  {
    if (currentUser.BusinessId is not Guid businessId)
    {
      return Result.Failure<BranchResponse>(BranchErrors.UserContextRequired);
    }

    var tenantId = new BusinessId(businessId);
    var branchId = new BranchId(command.BranchId);

    var branch = await repository.GetByIdAsync(tenantId, branchId, cancellationToken);

    if (branch is null)
    {
      return Result.Failure<BranchResponse>(BranchErrors.NotFound);
    }

    if (branch.IsActive)
    {
      return Result.Failure<BranchResponse>(BranchErrors.AlreadyActive);
    }

    branch.Activate(clock.UtcNow);

    await unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success(BranchResponseMapper.ToResponse(branch));
  }
}
