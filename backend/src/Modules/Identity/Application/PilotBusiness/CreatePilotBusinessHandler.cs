using System.Net.Mail;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Audit;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Billing.Application.Abstractions;
using SaasCommerce.Modules.Billing.Domain;
using SaasCommerce.Modules.Identity.Application.Account;
using SaasCommerce.Modules.Identity.Application.Abstractions;
using SaasCommerce.Modules.Identity.Contracts;
using SaasCommerce.Modules.Identity.Contracts.Responses;
using SaasCommerce.Modules.Identity.Domain;
using SaasCommerce.Modules.Tenancy.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Identity.Application.PilotBusiness;

public sealed class CreatePilotBusinessHandler(
  IAccountBusinessRepository businesses,
  IIdentityUserRepository users,
  IPasswordHasher passwordHasher,
  ISubscriptionPlanRepository subscriptionPlans,
  IBusinessSubscriptionRepository subscriptions,
  IClock clock,
  IUnitOfWork unitOfWork,
  IAuditLogWriter auditLogWriter,
  ICurrentUserService currentUser)
{
  public Task<Result<CreatePilotBusinessResponse>> Handle(
    CreatePilotBusinessCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    return HandleCoreAsync(command, cancellationToken);
  }

  private async Task<Result<CreatePilotBusinessResponse>> HandleCoreAsync(
    CreatePilotBusinessCommand command,
    CancellationToken cancellationToken)
  {
    // Validate required fields
    var errors = new List<DomainError>();

    if (string.IsNullOrWhiteSpace(command.BusinessName))
    {
      errors.Add(new DomainError("businessName", "Business name is required."));
    }

    if (string.IsNullOrWhiteSpace(command.BranchName))
    {
      errors.Add(new DomainError("branchName", "Branch name is required."));
    }

    if (string.IsNullOrWhiteSpace(command.AdminFullName))
    {
      errors.Add(new DomainError("adminFullName", "Admin full name is required."));
    }

    if (string.IsNullOrWhiteSpace(command.Phone))
    {
      errors.Add(new DomainError("phone", "Phone number is required."));
    }

    if (string.IsNullOrWhiteSpace(command.AdminEmail))
    {
      errors.Add(new DomainError("adminEmail", "Admin email is required."));
    }

    if (string.IsNullOrWhiteSpace(command.AdminPassword) || command.AdminPassword.Length < 8)
    {
      errors.Add(new DomainError("adminPassword", "Admin password must be at least 8 characters."));
    }

    if (errors.Count > 0)
    {
      return Result.Failure<CreatePilotBusinessResponse>(
        CreatePilotBusinessErrors.ValidationFailed(errors));
    }

    // Validate email format
    try
    {
      _ = new MailAddress(command.AdminEmail);
    }
    catch
    {
      return Result.Failure<CreatePilotBusinessResponse>(CreatePilotBusinessErrors.InvalidEmail);
    }

    // Validate identification type
    if (!Enum.TryParse<BusinessIdentificationType>(command.IdentificationType, true, out var identificationType))
    {
      return Result.Failure<CreatePilotBusinessResponse>(CreatePilotBusinessErrors.InvalidIdentificationType);
    }

    var normalizedId = NormalizeIdentification(identificationType, command.IdentificationNumber);
    if (!IsValidIdentification(identificationType, normalizedId))
    {
      return Result.Failure<CreatePilotBusinessResponse>(CreatePilotBusinessErrors.InvalidIdentificationNumber);
    }

    // Check uniqueness
    var normalizedEmail = command.AdminEmail.Trim().ToLowerInvariant();
    var existingUser = await users.GetByEmailAsync(normalizedEmail, cancellationToken);

    if (existingUser is not null)
    {
      return Result.Failure<CreatePilotBusinessResponse>(CreatePilotBusinessErrors.DuplicateEmail);
    }

    if (await businesses.ExistsByIdentificationAsync(
      identificationType.ToString(),
      normalizedId,
      cancellationToken))
    {
      return Result.Failure<CreatePilotBusinessResponse>(CreatePilotBusinessErrors.DuplicateIdentification);
    }

    // Load subscription plan
    var basicPlan = await subscriptionPlans.GetByCodeAsync(SubscriptionPlanCodes.Basic, cancellationToken);
    if (basicPlan is null)
    {
      return Result.Failure<CreatePilotBusinessResponse>(CreatePilotBusinessErrors.PlanNotFound);
    }

    // Create entities
    var now = clock.UtcNow;
    var businessId = BusinessId.New();
    var branchId = BranchId.New();

    var business = new Business(businessId, command.BusinessName, now, identificationType, normalizedId);
    business.AddBranch(branchId, command.BranchName, "PRINCIPAL", now, isMain: true);

    var normalizedPhone = string.Concat(command.Phone.Where(char.IsDigit));
    if (!string.IsNullOrEmpty(normalizedPhone))
    {
      business.AddPhone(Guid.NewGuid(), normalizedPhone, "Principal", true, now);
    }

    var adminRole = new Role(Guid.NewGuid(), businessId, SystemRoles.Admin);
    var adminUser = new User(
      Guid.NewGuid(),
      businessId,
      branchId,
      command.AdminFullName,
      normalizedEmail,
      passwordHasher.Hash(command.AdminPassword),
      now);
    adminUser.AddRole(adminRole);

    var subscription = BusinessSubscription.StartTrial(
      Guid.NewGuid(),
      businessId,
      basicPlan.Id,
      now,
      now.AddDays(14));

    await businesses.AddAsync(business, cancellationToken);
    await users.AddAsync(adminUser, cancellationToken);
    await subscriptions.AddAsync(subscription, cancellationToken);

    // Audit
    var auditEntry = new AuditEntry(
      new BusinessId(currentUser.BusinessId!.Value),
      currentUser.UserId,
      "pilot_business.created",
      "Business",
      businessId.Value,
      $"Pilot business created: '{command.BusinessName}' (admin: {normalizedEmail})");
    await auditLogWriter.WriteAsync(auditEntry, cancellationToken);

    await unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success(new CreatePilotBusinessResponse(
      businessId.Value,
      branchId.Value,
      adminUser.Id,
      business.Name,
      command.BranchName,
      normalizedEmail,
      subscription.TrialEndsAt));
  }

  private static string NormalizeIdentification(
    BusinessIdentificationType type,
    string value)
  {
    var trimmed = (value ?? string.Empty).Trim();

    return type is BusinessIdentificationType.Cedula or BusinessIdentificationType.Rnc
      ? string.Concat(trimmed.Where(char.IsDigit))
      : trimmed.Replace(" ", string.Empty, StringComparison.Ordinal).ToUpperInvariant();
  }

  private static bool IsValidIdentification(BusinessIdentificationType type, string value)
    => type switch
    {
      BusinessIdentificationType.Cedula => value.Length == 11,
      BusinessIdentificationType.Rnc => value.Length == 9,
      BusinessIdentificationType.Passport => value.Length >= 5 && value.All(char.IsLetterOrDigit),
      _ => false
    };
}
