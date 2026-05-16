using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Identity.Application.Account;

public static class AccountErrors
{
  public static DomainError InvalidRegistrationWith(IReadOnlyCollection<DomainError> validationErrors)
  {
    ArgumentNullException.ThrowIfNull(validationErrors);

    return new DomainError(
      InvalidRegistration.Code,
      InvalidRegistration.Message,
      validationErrors);
  }

  public static readonly DomainError InvalidRegistration = new(
    "validation_error",
    "Registration data is invalid.");

  public static readonly DomainError DuplicateEmail = new(
    "account.duplicate_email",
    "An active user with the same email already exists.");

  public static readonly DomainError DuplicateIdentification = new(
    "account.duplicate_identification",
    "A business with the same identification already exists.");
}
