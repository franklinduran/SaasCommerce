using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Identity.Contracts.Responses;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Identity.Application.Settings;

public sealed class UpdateCurrentBranchHandler(
  ICurrentUserService currentUser,
  IIdentitySettingsRepository settings,
  IClock clock,
  IUnitOfWork unitOfWork)
{
  public Task<Result<CurrentBranchResponse>> Handle(
    UpdateCurrentBranchCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    return HandleCoreAsync(command, cancellationToken);
  }

  private async Task<Result<CurrentBranchResponse>> HandleCoreAsync(
    UpdateCurrentBranchCommand command,
    CancellationToken cancellationToken)
  {
    if (string.IsNullOrWhiteSpace(command.Name))
    {
      return Result.Failure<CurrentBranchResponse>(IdentitySettingsErrors.InvalidBranch);
    }

    if (!currentUser.IsAuthenticated ||
        currentUser.BusinessId is not { } businessId ||
        currentUser.BranchId is not { } branchId)
    {
      return Result.Failure<CurrentBranchResponse>(IdentitySettingsErrors.InvalidCurrentUser);
    }

    var branch = await settings.GetBranchAsync(
      new BusinessId(businessId),
      new BranchId(branchId),
      cancellationToken);

    if (branch is null)
    {
      return Result.Failure<CurrentBranchResponse>(IdentitySettingsErrors.BranchNotFound);
    }

    branch.Update(command.Name, command.Address, command.Phone, clock.UtcNow);
    await unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success(SettingsResponseMapper.ToCurrentBranchResponse(branch));
  }
}
