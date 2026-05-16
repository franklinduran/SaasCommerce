using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Identity.Application.Abstractions;
using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Identity.Application.Settings;

public sealed class ChangeMyPasswordHandler(
  ICurrentUserService currentUser,
  IIdentitySettingsRepository settings,
  IPasswordHasher passwordHasher,
  IClock clock,
  IUnitOfWork unitOfWork)
{
  public Task<Result<string>> Handle(
    ChangeMyPasswordCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    return HandleCoreAsync(command, cancellationToken);
  }

  private async Task<Result<string>> HandleCoreAsync(
    ChangeMyPasswordCommand command,
    CancellationToken cancellationToken)
  {
    if (string.IsNullOrWhiteSpace(command.CurrentPassword) ||
        string.IsNullOrWhiteSpace(command.NewPassword) ||
        command.NewPassword.Length < 8)
    {
      return Result.Failure<string>(IdentitySettingsErrors.InvalidPassword);
    }

    var userId = currentUser.UserId;

    if (!currentUser.IsAuthenticated || userId is null)
    {
      return Result.Failure<string>(IdentitySettingsErrors.InvalidCurrentUser);
    }

    var user = await settings.GetUserAsync(userId.Value, cancellationToken);

    if (user is null)
    {
      return Result.Failure<string>(IdentitySettingsErrors.UserNotFound);
    }

    if (!passwordHasher.Verify(command.CurrentPassword, user.PasswordHash))
    {
      return Result.Failure<string>(IdentitySettingsErrors.InvalidCurrentPassword);
    }

    user.ChangePasswordHash(passwordHasher.Hash(command.NewPassword), clock.UtcNow);
    await unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success("Password updated.");
  }
}
