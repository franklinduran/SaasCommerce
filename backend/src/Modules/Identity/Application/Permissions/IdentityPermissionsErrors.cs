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
}
