using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Identity.Application.Abstractions;
using SaasCommerce.Modules.Identity.Application.Auth;
using SaasCommerce.Modules.Identity.Contracts.Responses;
using SaasCommerce.Modules.Identity.Domain;
using SaasCommerce.Modules.Tenancy.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Identity.Application.Account;

public sealed class RegisterBusinessHandler(
  IAccountBusinessRepository businesses,
  IIdentityUserRepository users,
  IPasswordHasher passwordHasher,
  IJwtTokenService jwtTokenService,
  IRefreshTokenGenerator refreshTokenGenerator,
  IClock clock,
  IUnitOfWork unitOfWork)
{
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
      out var phones))
    {
      return Result.Failure<RegisterBusinessResponse>(AccountErrors.InvalidRegistration);
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
    business.AddBranch(branchId, command.BranchName, clock.UtcNow, isMain: true);

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

    await businesses.AddAsync(business, cancellationToken);
    await users.AddAsync(user, cancellationToken);

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
    out IReadOnlyCollection<RegisterBusinessPhoneCommand> phones)
  {
    normalizedEmail = string.Empty;
    identificationType = default;
    identificationNumber = string.Empty;
    phones = [];

    if (string.IsNullOrWhiteSpace(command.BusinessName) ||
        string.IsNullOrWhiteSpace(command.BranchName) ||
        string.IsNullOrWhiteSpace(command.OwnerFullName) ||
        string.IsNullOrWhiteSpace(command.Email) ||
        !command.Email.Contains('@', StringComparison.Ordinal) ||
        string.IsNullOrWhiteSpace(command.Password) ||
        command.Password.Length < 8 ||
        string.IsNullOrWhiteSpace(command.IdentificationType) ||
        string.IsNullOrWhiteSpace(command.IdentificationNumber) ||
        !Enum.TryParse(command.IdentificationType, true, out identificationType))
    {
      return false;
    }

    identificationNumber = NormalizeIdentification(identificationType, command.IdentificationNumber);

    if (!IsValidIdentification(identificationType, identificationNumber))
    {
      return false;
    }

    if (!TryNormalizePhones(command.Phones, out var normalizedPhones))
    {
      return false;
    }

    normalizedEmail = command.Email.Trim().ToLowerInvariant();
    phones = normalizedPhones;

    return true;
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

  private static bool TryNormalizePhones(
    IReadOnlyCollection<RegisterBusinessPhoneCommand>? phones,
    out List<RegisterBusinessPhoneCommand> normalizedPhones)
  {
    normalizedPhones = [];

    if (phones is null || phones.Count == 0 || phones.Count(phone => phone.IsPrimary) != 1)
    {
      return false;
    }

    var normalized = new List<RegisterBusinessPhoneCommand>();
    var seen = new HashSet<string>(StringComparer.Ordinal);

    foreach (var phone in phones)
    {
      var number = NormalizePhone(phone.Number);

      if (string.IsNullOrWhiteSpace(number) || !seen.Add(number))
      {
        return false;
      }

      normalized.Add(new RegisterBusinessPhoneCommand(number, phone.Label, phone.IsPrimary));
    }

    normalizedPhones = normalized;

    return true;
  }

  private static string NormalizePhone(string? number)
    => string.IsNullOrWhiteSpace(number)
      ? string.Empty
      : KeepDigits(number);

  private static string KeepDigits(string value)
    => string.Concat(value.Where(char.IsDigit));
}
