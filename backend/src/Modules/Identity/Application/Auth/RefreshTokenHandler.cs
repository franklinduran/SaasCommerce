using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Identity.Application.Abstractions;
using SaasCommerce.Modules.Identity.Contracts.Responses;
using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Identity.Application.Auth;

public sealed class RefreshTokenHandler(
  IIdentityUserRepository users,
  IJwtTokenService jwtTokenService,
  IRefreshTokenGenerator refreshTokenGenerator,
  IClock clock,
  IUnitOfWork unitOfWork)
{
  public Task<Result<LoginResponse>> Handle(
    RefreshTokenCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    return HandleCoreAsync(command, cancellationToken);
  }

  private async Task<Result<LoginResponse>> HandleCoreAsync(
    RefreshTokenCommand command,
    CancellationToken cancellationToken)
  {
    var tokenValue = command.RefreshToken.Trim();
    var user = await users.GetByRefreshTokenAsync(tokenValue, cancellationToken);
    var currentToken = user?.RefreshTokens.SingleOrDefault(token => token.Token == tokenValue);

    if (user is null || currentToken is null || !currentToken.IsActive(clock.UtcNow))
    {
      return Result.Failure<LoginResponse>(IdentityErrors.InvalidRefreshToken);
    }

    currentToken.Revoke(clock.UtcNow);

    var accessToken = jwtTokenService.CreateAccessToken(user);
    var refreshToken = refreshTokenGenerator.Create();

    user.AddRefreshToken(
      refreshToken,
      clock.UtcNow.AddDays(30),
      clock.UtcNow);

    await unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success(new LoginResponse(
      accessToken.Token,
      refreshToken,
      accessToken.ExpiresAt,
      IdentityResponseMapper.ToAuthUserResponse(user)));
  }
}
