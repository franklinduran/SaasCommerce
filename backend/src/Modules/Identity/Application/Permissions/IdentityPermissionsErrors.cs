using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Identity.Application.Permissions;

public static class IdentityPermissionsErrors
{
  public static readonly DomainError UserContextRequired =
    new("identity.user_context_required", "Authenticated user context is required.");

  public static readonly DomainError Forbidden =
    new("identity.forbidden", "You do not have permission to perform this action.");

  public static readonly DomainError UserNotFound =
    new("identity.user_not_found", "User not found.");

  public static readonly DomainError CannotDisableSelf =
    new("identity.cannot_disable_self", "You cannot disable your own account.");

  public static readonly DomainError UserBelongsToDifferentBusiness =
    new("identity.user_different_business", "The specified user does not belong to your business.");

  public static readonly DomainError CannotRemoveLastOwner =
    new("identity.cannot_remove_last_owner", "Cannot change role of the last Owner or Admin in the business.");

  public static readonly DomainError DuplicateEmail =
    new("identity.duplicate_email", "A user with this email already exists in your business.");

  public static readonly DomainError InvalidEmail =
    new("identity.invalid_email", "The provided email address is invalid.");

  public static readonly DomainError PasswordTooShort =
    new("identity.password_too_short", "Password must be at least 8 characters long.");

  public static readonly DomainError CannotCreateOwnerRole =
    new("identity.cannot_create_owner_role", "You cannot create users with the Owner role.");

  public static readonly DomainError CannotResetOwnPassword =
    new("identity.cannot_reset_own_password", "You cannot reset your own password.");

  public static readonly DomainError UserAlreadyActive =
    new("identity.user_already_active", "User is already active.");

  public static readonly DomainError UserAlreadyInactive =
    new("identity.user_already_inactive", "User is already inactive.");

  public static readonly DomainError InvalidRole =
    new("identity.invalid_role", "The specified role is invalid or does not exist.");
}
