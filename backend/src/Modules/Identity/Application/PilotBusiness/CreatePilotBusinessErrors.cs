using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Identity.Application.PilotBusiness;

public static class CreatePilotBusinessErrors
{
  public static DomainError DuplicateEmail { get; } =
    new("PILOT_BUSINESS_DUPLICATE_EMAIL", "A user with this email already exists.");

  public static DomainError DuplicateIdentification { get; } =
    new("PILOT_BUSINESS_DUPLICATE_IDENTIFICATION", "A business with this identification already exists.");

  public static DomainError InvalidEmail { get; } =
    new("PILOT_BUSINESS_INVALID_EMAIL", "Admin email format is invalid.");

  public static DomainError PasswordTooShort { get; } =
    new("PILOT_BUSINESS_PASSWORD_TOO_SHORT", "Admin password must be at least 8 characters.");

  public static DomainError PlanNotFound { get; } =
    new("PILOT_BUSINESS_PLAN_NOT_FOUND", "No active subscription plan found to assign to the new business.");

  public static DomainError InvalidIdentificationType { get; } =
    new("PILOT_BUSINESS_INVALID_IDENTIFICATION_TYPE", "Identification type must be Cedula, Rnc, or Passport.");

  public static DomainError InvalidIdentificationNumber { get; } =
    new("PILOT_BUSINESS_INVALID_IDENTIFICATION_NUMBER", "Identification number format is invalid for the given type.");

  public static DomainError ValidationFailed(IReadOnlyCollection<DomainError> errors) =>
    new("VALIDATION_ERROR", $"Validation failed: {string.Join("; ", errors.Select(e => e.Message))}");
}
