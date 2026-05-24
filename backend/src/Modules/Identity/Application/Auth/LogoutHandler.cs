using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Identity.Application.Abstractions;
using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Identity.Application.Auth;

public sealed class LogoutHandler(
  IIdentityUserRepository users,
  IClock clock,
  IUnitOfWork unitOfWork)
{
  public Task<Result> Handle(LogoutCommand command, CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    return HandleCoreAsync(command, cancellationToken);
  }

  private async Task<Result> HandleCoreAsync(LogoutCommand command, CancellationToken cancellationToken)
  {
    // Trim to tolerate minor whitespace issues from the client
    var tokenValue = command.RefreshToken.Trim();

    // Intentionally succeed even if the token is not found to prevent enumeration attacks
    var user = await users.GetByRefreshTokenAsync(tokenValue, cancellationToken);

    if (user is null)
    {
      return Result.Success();
    }

    var token = user.RefreshTokens.SingleOrDefault(rt => rt.Token == tokenValue);

    if (token is null || !token.IsActive(clock.UtcNow))
    {
      return Result.Success();
    }

    token.Revoke(clock.UtcNow);

    await unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success();
  }
}
