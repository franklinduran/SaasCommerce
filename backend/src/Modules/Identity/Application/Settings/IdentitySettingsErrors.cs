using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Identity.Application.Settings;

public static class IdentitySettingsErrors
{
  private const string ValidationErrorCode = "validation_error";

  public static readonly DomainError InvalidCurrentUser = new(
    "identity.invalid_current_user",
    "The authenticated user context is incomplete.");

  public static readonly DomainError UserNotFound = new(
    "identity.user_not_found",
    "The current user was not found.");

  public static readonly DomainError BusinessNotFound = new(
    "tenancy.business_not_found",
    "The current business was not found.");

  public static readonly DomainError BranchNotFound = new(
    "tenancy.branch_not_found",
    "The current branch was not found.");

  public static readonly DomainError InvalidProfile = new(
    ValidationErrorCode,
    "The profile data is invalid.");

  public static readonly DomainError InvalidPassword = new(
    ValidationErrorCode,
    "The password request is invalid.");

  public static readonly DomainError InvalidCurrentPassword = new(
    "identity.invalid_current_password",
    "The current password is incorrect.");

  public static readonly DomainError InvalidBusiness = new(
    ValidationErrorCode,
    "The business data is invalid.");

  public static readonly DomainError DuplicateIdentification = new(
    "tenancy.duplicate_identification",
    "A business with the same identification already exists.");

  public static readonly DomainError InvalidBranch = new(
    ValidationErrorCode,
    "The branch data is invalid.");

  public static readonly DomainError Forbidden = new(
    "forbidden",
    "The current user is not allowed to perform this action.");
}
