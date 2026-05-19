using System.Security.Cryptography;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Audit;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Identity.Application.Abstractions;
using SaasCommerce.Modules.Identity.Application.Permissions;
using SaasCommerce.Modules.Identity.Contracts.Events.V1;
using SaasCommerce.Modules.Identity.Contracts.Responses;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Identity.Application.Users;

public sealed class ResetUserPasswordHandler(
  IUserManagementRepository repository,
  ICurrentUserService currentUser,
  IPasswordHasher passwordHasher,
  IClock clock,
  IUnitOfWork unitOfWork,
  IAuditLogWriter auditLogWriter,
  IEventBus eventBus)
{
  public async Task<Result<ResetPasswordResponse>> Handle(
    ResetUserPasswordCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    if (!currentUser.IsAuthenticated || currentUser.BusinessId is not { } businessId)
    {
      return Result.Failure<ResetPasswordResponse>(IdentityPermissionsErrors.UserContextRequired);
    }

    if (currentUser.UserId == command.TargetUserId)
    {
      return Result.Failure<ResetPasswordResponse>(IdentityPermissionsErrors.CannotResetOwnPassword);
    }

    var businessIdVo = new BusinessId(businessId);
    var user = await repository.GetByIdInBusinessAsync(
      command.TargetUserId,
      businessIdVo,
      cancellationToken);

    if (user is null)
    {
      return Result.Failure<ResetPasswordResponse>(IdentityPermissionsErrors.UserNotFound);
    }

    // Generate cryptographically secure temporary password (12 chars)
    var tempPassword = GenerateTemporaryPassword();
    var hashedPassword = passwordHasher.Hash(tempPassword);

    var now = clock.UtcNow;
    user.ChangePasswordHash(hashedPassword, now);
    user.ForcePasswordChange(now);

    await unitOfWork.SaveChangesAsync(cancellationToken);

    // Create audit log entry
    var auditEntry = new AuditEntry(
      businessIdVo,
      currentUser.UserId,
      "user.password_reset",
      "User",
      command.TargetUserId,
      "User password reset - temporary password generated");

    await auditLogWriter.WriteAsync(auditEntry, cancellationToken);

    // Publish integration event
    var integrationEvent = new UserPasswordResetIntegrationEventV1(
      Guid.NewGuid(),
      Guid.NewGuid(),
      businessId,
      command.TargetUserId,
      now);

    await eventBus.PublishAsync(integrationEvent, cancellationToken);

    return Result.Success(new ResetPasswordResponse(tempPassword));
  }

  private static string GenerateTemporaryPassword()
  {
    // Generate 12-character password with uppercase, lowercase, digits, and special chars
    const string uppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    const string lowercaseChars = "abcdefghijklmnopqrstuvwxyz";
    const string digitChars = "0123456789";
    const string specialChars = "!@#$%^&*";

    var allChars = $"{uppercaseChars}{lowercaseChars}{digitChars}{specialChars}";
    var password = new char[12];

    using var rng = RandomNumberGenerator.Create();
    var randomBytes = new byte[12];
    rng.GetBytes(randomBytes);

    // Ensure at least one char from each category
    password[0] = uppercaseChars[randomBytes[0] % uppercaseChars.Length];
    password[1] = lowercaseChars[randomBytes[1] % lowercaseChars.Length];
    password[2] = digitChars[randomBytes[2] % digitChars.Length];
    password[3] = specialChars[randomBytes[3] % specialChars.Length];

    // Fill remaining positions randomly
    for (var i = 4; i < 12; i++)
    {
      password[i] = allChars[randomBytes[i] % allChars.Length];
    }

    // Shuffle the password
    for (var i = password.Length - 1; i > 0; i--)
    {
      rng.GetBytes(randomBytes);
      var j = randomBytes[0] % (i + 1);

      (password[i], password[j]) = (password[j], password[i]);
    }

    return new string(password);
  }
}
