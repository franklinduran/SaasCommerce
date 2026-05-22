using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Tenancy.Contracts.Responses;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Tenancy.Application.Branches;

public sealed class UpdateBranchHandler(
  IBranchRepository repository,
  ICurrentUserService currentUser,
  IClock clock,
  IUnitOfWork unitOfWork)
{
  public Task<Result<BranchResponse>> Handle(
    UpdateBranchCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    return HandleCoreAsync(command, cancellationToken);
  }

  private async Task<Result<BranchResponse>> HandleCoreAsync(
    UpdateBranchCommand command,
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

    if (await repository.ExistsByNameAsync(tenantId, command.Name, branchId, cancellationToken))
    {
      return Result.Failure<BranchResponse>(BranchErrors.NameAlreadyExists);
    }

    branch.Update(command.Name, command.Address, command.Phone, clock.UtcNow);

    await unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success(BranchResponseMapper.ToResponse(branch));
  }
}
