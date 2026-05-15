using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Identity.Application.Abstractions;
using SaasCommerce.Modules.Identity.Contracts.Responses;
using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Identity.Application.Auth;

public sealed class GetCurrentUserHandler(
  ICurrentUserService currentUser,
  IIdentityUserRepository users)
{
  public async Task<Result<AuthUserResponse>> Handle(
    CancellationToken cancellationToken = default)
  {
    if (!currentUser.IsAuthenticated || currentUser.UserId is not { } userId)
    {
      return Result.Failure<AuthUserResponse>(IdentityErrors.NotAuthenticated);
    }

    var user = await users.GetByIdAsync(userId, cancellationToken);

    return user is null
      ? Result.Failure<AuthUserResponse>(IdentityErrors.UserNotFound)
      : Result.Success(IdentityResponseMapper.ToAuthUserResponse(user));
  }
}
