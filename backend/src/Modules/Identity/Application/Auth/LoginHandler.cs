using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Identity.Application.Abstractions;
using SaasCommerce.Modules.Identity.Contracts.Responses;
using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Identity.Application.Auth;

public sealed class LoginHandler(
  IIdentityUserRepository users,
  IPasswordHasher passwordHasher,
  IJwtTokenService jwtTokenService,
  IRefreshTokenGenerator refreshTokenGenerator,
  IClock clock,
  IUnitOfWork unitOfWork)
{
  public async Task<Result<LoginResponse>> Handle(
    LoginCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    var email = command.Email.Trim().ToLowerInvariant();
    var user = await users.GetByEmailAsync(email, cancellationToken);

    if (user is null ||
        !user.IsActive ||
        !passwordHasher.Verify(command.Password, user.PasswordHash))
    {
      return Result.Failure<LoginResponse>(IdentityErrors.InvalidCredentials);
    }

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
