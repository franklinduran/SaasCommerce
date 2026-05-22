using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Billing.Application.Abstractions;
using SaasCommerce.Modules.Tenancy.Contracts.Responses;
using SaasCommerce.Modules.Tenancy.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Tenancy.Application.Branches;

public sealed class CreateBranchHandler(
  IBranchRepository repository,
  ICurrentUserService currentUser,
  IClock clock,
  IUnitOfWork unitOfWork,
  ISubscriptionLimitChecker limitChecker)
{
  public Task<Result<BranchResponse>> Handle(
    CreateBranchCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    return HandleCoreAsync(command, cancellationToken);
  }

  private async Task<Result<BranchResponse>> HandleCoreAsync(
    CreateBranchCommand command,
    CancellationToken cancellationToken)
  {
    if (currentUser.BusinessId is not Guid businessId)
    {
      return Result.Failure<BranchResponse>(BranchErrors.UserContextRequired);
    }

    var tenantId = new BusinessId(businessId);

    // Check subscription limits
    var limitCheck = await limitChecker.CanCreateBranchAsync(tenantId, cancellationToken);
    if (!limitCheck.IsAllowed)
    {
      return Result.Failure<BranchResponse>(new DomainError("subscription.limit_reached", limitCheck.Message));
    }

    var normalizedCode = command.Code.Trim().ToUpperInvariant();

    if (await repository.ExistsByCodeAsync(tenantId, normalizedCode, null, cancellationToken))
    {
      return Result.Failure<BranchResponse>(BranchErrors.CodeAlreadyExists);
    }

    if (await repository.ExistsByNameAsync(tenantId, command.Name, null, cancellationToken))
    {
      return Result.Failure<BranchResponse>(BranchErrors.NameAlreadyExists);
    }

    if (command.IsMain && await repository.HasMainBranchAsync(tenantId, null, cancellationToken))
    {
      return Result.Failure<BranchResponse>(BranchErrors.MainBranchAlreadyExists);
    }

    var branch = new Branch(
      BranchId.New(),
      tenantId,
      command.Name,
      normalizedCode,
      clock.UtcNow,
      command.IsMain);

    if (!string.IsNullOrWhiteSpace(command.Address) || !string.IsNullOrWhiteSpace(command.Phone))
    {
      branch.Update(command.Name, command.Address, command.Phone, clock.UtcNow);
    }

    await repository.AddAsync(branch, cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success(BranchResponseMapper.ToResponse(branch));
  }
}
