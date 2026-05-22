using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Tenancy.Contracts.Responses;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Tenancy.Application.Branches;

public sealed record DeactivateBranchCommand(Guid BranchId);

public sealed class DeactivateBranchHandler(
  IBranchRepository repository,
  ICurrentUserService currentUser,
  IClock clock,
  IUnitOfWork unitOfWork)
{
  public Task<Result<BranchResponse>> Handle(
    DeactivateBranchCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    return HandleCoreAsync(command, cancellationToken);
  }

  private async Task<Result<BranchResponse>> HandleCoreAsync(
    DeactivateBranchCommand command,
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

    if (!branch.IsActive)
    {
      return Result.Failure<BranchResponse>(BranchErrors.AlreadyInactive);
    }

    var activeCount = await repository.CountActiveAsync(tenantId, cancellationToken);
    var isOnlyActive = activeCount <= 1;

    try
    {
      branch.Deactivate(isOnlyActive, clock.UtcNow);
    }
    catch (InvalidOperationException)
    {
      return Result.Failure<BranchResponse>(BranchErrors.CannotDeactivateLastActive);
    }

    await unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success(BranchResponseMapper.ToResponse(branch));
  }
}
