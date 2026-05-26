using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Billing.Application.Abstractions;
using SaasCommerce.Modules.Billing.Domain;
using SaasCommerce.Modules.Identity.Application.Abstractions;
using SaasCommerce.Modules.Identity.Application.Auth;
using SaasCommerce.Modules.Identity.Contracts.Responses;
using SaasCommerce.Modules.Identity.Domain;
using SaasCommerce.Modules.Tenancy.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Identity.Application.Account;

public sealed class RegisterBusinessDependencies
{
  public required IAccountBusinessRepository Businesses { get; init; }
  public required IIdentityUserRepository Users { get; init; }
  public required IPasswordHasher PasswordHasher { get; init; }
  public required IJwtTokenService JwtTokenService { get; init; }
  public required IRefreshTokenGenerator RefreshTokenGenerator { get; init; }
  public required ISubscriptionPlanRepository SubscriptionPlans { get; init; }
  public required IBusinessSubscriptionRepository Subscriptions { get; init; }
  public required IClock Clock { get; init; }
  public required IUnitOfWork UnitOfWork { get; init; }
}

public sealed class RegisterBusinessHandler(RegisterBusinessDependencies dependencies)
{
  private readonly IAccountBusinessRepository businesses = dependencies.Businesses;
  private readonly IIdentityUserRepository users = dependencies.Users;
  private readonly IPasswordHasher passwordHasher = dependencies.PasswordHasher;
  private readonly IJwtTokenService jwtTokenService = dependencies.JwtTokenService;
  private readonly IRefreshTokenGenerator refreshTokenGenerator = dependencies.RefreshTokenGenerator;
  private readonly ISubscriptionPlanRepository subscriptionPlans = dependencies.SubscriptionPlans;
  private readonly IBusinessSubscriptionRepository subscriptions = dependencies.Subscriptions;
  private readonly IClock clock = dependencies.Clock;
  private readonly IUnitOfWork unitOfWork = dependencies.UnitOfWork;

  public Task<Result<RegisterBusinessResponse>> Handle(
    RegisterBusinessCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    return HandleCoreAsync(command, cancellationToken);
  }

  private async Task<Result<RegisterBusinessResponse>> HandleCoreAsync(
    RegisterBusinessCommand command,
    CancellationToken cancellationToken)
  {
    if (!TryValidateAndNormalize(
      command,
      out var normalizedEmail,
      out var identificationType,
      out var identificationNumber,
      out var phones,
      out var validationErrors))
    {
      return Result.Failure<RegisterBusinessResponse>(
        AccountErrors.InvalidRegistrationWith(validationErrors));
    }

    var existingUser = await users.GetByEmailAsync(normalizedEmail, cancellationToken);

    if (existingUser is not null)
    {
      return Result.Failure<RegisterBusinessResponse>(AccountErrors.DuplicateEmail);
    }

    if (await businesses.ExistsByIdentificationAsync(
          identificationType.ToString(),
          identificationNumber,
          cancellationToken))
    {
      return Result.Failure<RegisterBusinessResponse>(AccountErrors.DuplicateIdentification);
    }

    var businessId = BusinessId.New();
    var branchId = BranchId.New();
    var business = new Business(
      businessId,
      command.BusinessName,
      clock.UtcNow,
      identificationType,
      identificationNumber);
    business.AddBranch(branchId, command.BranchName, "PRINCIPAL", clock.UtcNow, isMain: true);

    foreach (var phone in phones)
    {
      business.AddPhone(Guid.NewGuid(), phone.Number, phone.Label, phone.IsPrimary, clock.UtcNow);
    }

    var adminRole = new Role(Guid.NewGuid(), businessId, "Admin");
    var user = new User(
      Guid.NewGuid(),
      businessId,
      branchId,
      command.OwnerFullName,
      normalizedEmail,
      passwordHasher.Hash(command.Password),
      clock.UtcNow);
    user.AddRole(adminRole);

    var basicPlan = await subscriptionPlans.GetByCodeAsync(SubscriptionPlanCodes.Basic, cancellationToken);
    if (basicPlan is null)
    {
      return Result.Failure<RegisterBusinessResponse>(SubscriptionErrors.PlanNotFound);
    }

    var subscription = BusinessSubscription.StartTrial(
      Guid.NewGuid(),
      businessId,
      basicPlan.Id,
      clock.UtcNow,
      clock.UtcNow.AddDays(14));

    await businesses.AddAsync(business, cancellationToken);
    await users.AddAsync(user, cancellationToken);
    await subscriptions.AddAsync(subscription, cancellationToken);

    var accessToken = jwtTokenService.CreateAccessToken(user);
    var refreshToken = refreshTokenGenerator.Create();
    user.AddRefreshToken(refreshToken, clock.UtcNow.AddDays(30), clock.UtcNow);

    await unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success(new RegisterBusinessResponse(
      businessId.Value,
      branchId.Value,
      user.Id,
      accessToken.Token,
      refreshToken,
      accessToken.ExpiresAt,
      IdentityResponseMapper.ToAuthUserResponse(user)));
  }

  private static bool TryValidateAndNormalize(
    RegisterBusinessCommand command,
    out string normalizedEmail,
    out BusinessIdentificationType identificationType,
    out string identificationNumber,
    out IReadOnlyCollection<RegisterBusinessPhoneCommand> phones,
    out IReadOnlyCollection<DomainError> validationErrors)
  {
    normalizedEmail = string.Empty;
    identificationType = default;
    identificationNumber = string.Empty;
    phones = [];
    var errors = new List<DomainError>();

    AddRequiredError(errors, command.BusinessName, "businessName", "Business name is required.");
    AddRequiredError(errors, command.BranchName, "branchName", "Branch name is required.");
    AddRequiredError(errors, command.OwnerFullName, "ownerFullName", "Owner full name is required.");
    AddRequiredError(errors, command.Email, "email", "Email is required.");
    AddRequiredError(errors, command.Password, "password", "Password is required.");
    AddRequiredError(errors, command.IdentificationType, "identificationType", "Identification type is required.");
    AddRequiredError(errors, command.IdentificationNumber, "identificationNumber", "Identification number is required.");

    if (!string.IsNullOrWhiteSpace(command.Email) &&
        !command.Email.Contains('@', StringComparison.Ordinal))
    {
      errors.Add(new DomainError("email", "Email format is invalid."));
    }

    if (!string.IsNullOrWhiteSpace(command.Password) && command.Password.Length < 8)
    {
      errors.Add(new DomainError("password", "Password must contain at least 8 characters."));
    }

    if (!string.IsNullOrWhiteSpace(command.IdentificationType) &&
        !Enum.TryParse(command.IdentificationType, true, out identificationType))
    {
      errors.Add(new DomainError("identificationType", "Identification type must be Cedula, Rnc or Passport."));
    }

    if (errors.Count > 0)
    {
      validationErrors = errors;

      return false;
    }

    identificationNumber = NormalizeIdentification(identificationType, command.IdentificationNumber!);

    if (!IsValidIdentification(identificationType, identificationNumber))
    {
      errors.Add(new DomainError(
        "identificationNumber",
        GetIdentificationErrorMessage(identificationType)));
      validationErrors = errors;

      return false;
    }

    if (!TryNormalizePhones(command.Phones, out var normalizedPhones, out var phoneErrors))
    {
      validationErrors = phoneErrors;

      return false;
    }

    normalizedEmail = command.Email.Trim().ToLowerInvariant();
    phones = normalizedPhones;
    validationErrors = [];

    return true;
  }

  private static void AddRequiredError(
    List<DomainError> errors,
    string? value,
    string field,
    string message)
  {
    if (string.IsNullOrWhiteSpace(value))
    {
      errors.Add(new DomainError(field, message));
    }
  }

  private static string NormalizeIdentification(
    BusinessIdentificationType identificationType,
    string identificationNumber)
  {
    var trimmed = identificationNumber.Trim();

    return identificationType is BusinessIdentificationType.Cedula or BusinessIdentificationType.Rnc
      ? KeepDigits(trimmed)
      : trimmed.Replace(" ", string.Empty, StringComparison.Ordinal).ToUpperInvariant();
  }

  private static bool IsValidIdentification(
    BusinessIdentificationType identificationType,
    string identificationNumber)
    => identificationType switch
    {
      BusinessIdentificationType.Cedula => identificationNumber.Length == 11,
      BusinessIdentificationType.Rnc => identificationNumber.Length == 9,
      BusinessIdentificationType.Passport => identificationNumber.Length >= 5 &&
        identificationNumber.All(char.IsLetterOrDigit),
      _ => false
    };

  private static string GetIdentificationErrorMessage(BusinessIdentificationType identificationType)
    => identificationType switch
    {
      BusinessIdentificationType.Cedula => "Cedula must contain 11 digits.",
      BusinessIdentificationType.Rnc => "RNC must contain 9 digits.",
      BusinessIdentificationType.Passport => "Passport must contain at least 5 letters or digits.",
      _ => "Identification number is invalid."
    };

  private static bool TryNormalizePhones(
    IReadOnlyCollection<RegisterBusinessPhoneCommand>? phones,
    out List<RegisterBusinessPhoneCommand> normalizedPhones,
    out IReadOnlyCollection<DomainError> validationErrors)
  {
    normalizedPhones = [];
    var errors = new List<DomainError>();

    if (phones is null || phones.Count == 0)
    {
      validationErrors = [new DomainError("phones", "At least one phone is required.")];

      return false;
    }

    if (phones.Count(phone => phone.IsPrimary) != 1)
    {
      validationErrors =
      [
        new DomainError("phones", "Exactly one primary phone is required.")
      ];

      return false;
    }

    var normalized = new List<RegisterBusinessPhoneCommand>();
    var seen = new HashSet<string>(StringComparer.Ordinal);

    foreach (var phone in phones)
    {
      var number = NormalizePhone(phone.Number);

      if (string.IsNullOrWhiteSpace(number))
      {
        errors.Add(new DomainError("phones", "Phone number is required."));

        continue;
      }

      if (!seen.Add(number))
      {
        errors.Add(new DomainError("phones", "Phone numbers must not be duplicated."));

        continue;
      }

      normalized.Add(new RegisterBusinessPhoneCommand(number, phone.Label, phone.IsPrimary));
    }

    if (errors.Count > 0)
    {
      validationErrors = errors;

      return false;
    }

    normalizedPhones = normalized;
    validationErrors = [];

    return true;
  }

  private static string NormalizePhone(string? number)
    => string.IsNullOrWhiteSpace(number)
      ? string.Empty
      : KeepDigits(number);

  private static string KeepDigits(string value)
    => string.Concat(value.Where(char.IsDigit));
}
