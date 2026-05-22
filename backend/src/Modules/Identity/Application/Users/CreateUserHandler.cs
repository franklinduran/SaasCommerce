using System.Net.Mail;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Audit;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Billing.Application.Abstractions;
using SaasCommerce.Modules.Identity.Application.Abstractions;
using SaasCommerce.Modules.Identity.Application.Permissions;
using SaasCommerce.Modules.Identity.Contracts;
using SaasCommerce.Modules.Identity.Contracts.Events.V1;
using SaasCommerce.Modules.Identity.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Identity.Application.Users;

public sealed class CreateUserHandler(
  IUserManagementRepository repository,
  ICurrentUserService currentUser,
  IPasswordHasher passwordHasher,
  IClock clock,
  IUnitOfWork unitOfWork,
  IAuditLogWriter auditLogWriter,
  IEventBus eventBus,
  ISubscriptionLimitChecker limitChecker)
{
  public async Task<Result<Guid>> Handle(
    CreateUserCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    // Validate authentication and business context
    if (!currentUser.IsAuthenticated || currentUser.BusinessId is not { } businessId)
    {
      return Result.Failure<Guid>(IdentityPermissionsErrors.UserContextRequired);
    }

    // Validate email format
    try
    {
      _ = new MailAddress(command.Email);
    }
    catch
    {
      return Result.Failure<Guid>(IdentityPermissionsErrors.InvalidEmail);
    }

    // Validate password length
    if (string.IsNullOrWhiteSpace(command.Password) || command.Password.Length < 8)
    {
      return Result.Failure<Guid>(IdentityPermissionsErrors.PasswordTooShort);
    }

    // Cannot create Owner role
    if (command.Role.Equals(SystemRoles.Owner, StringComparison.OrdinalIgnoreCase))
    {
      return Result.Failure<Guid>(IdentityPermissionsErrors.CannotCreateOwnerRole);
    }

    // Validate role exists
    if (!SystemRoles.All.Contains(command.Role))
    {
      return Result.Failure<Guid>(IdentityPermissionsErrors.InvalidRole);
    }

    var businessIdVo = new BusinessId(businessId);

    // Check subscription limits
    var limitCheck = await limitChecker.CanCreateUserAsync(businessIdVo, cancellationToken);
    if (!limitCheck.IsAllowed)
    {
      return Result.Failure<Guid>(new DomainError("subscription.limit_reached", limitCheck.Message));
    }

    // Check email uniqueness within business
    var emailExists = await repository.EmailExistsInBusinessAsync(
      command.Email.Trim().ToLowerInvariant(),
      businessIdVo,
      cancellationToken);

    if (emailExists)
    {
      return Result.Failure<Guid>(IdentityPermissionsErrors.DuplicateEmail);
    }

    // Create user
    var userId = Guid.NewGuid();
    var hashedPassword = passwordHasher.Hash(command.Password);
    var now = clock.UtcNow;

    var user = new User(
      userId,
      businessIdVo,
      command.DefaultBranchId is not null ? new BranchId(command.DefaultBranchId.Value) : null,
      command.FullName,
      command.Email,
      hashedPassword,
      now);

    var role = new Role(Guid.NewGuid(), businessIdVo, command.Role);

    await repository.CreateUserAsync(user, role, cancellationToken);

    // Create audit log entry
    var auditEntry = new AuditEntry(
      businessIdVo,
      currentUser.UserId,
      "user.created",
      "User",
      userId,
      $"Created user: {command.FullName} ({command.Email}) with role: {command.Role}");

    await auditLogWriter.WriteAsync(auditEntry, cancellationToken);

    // Publish integration event
    var integrationEvent = new UserCreatedIntegrationEventV1(
      Guid.NewGuid(),
      Guid.NewGuid(),
      businessId,
      userId,
      command.FullName,
      command.Email,
      command.Role,
      now);

    await eventBus.PublishAsync(integrationEvent, cancellationToken);

    await unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success(userId);
  }
}
