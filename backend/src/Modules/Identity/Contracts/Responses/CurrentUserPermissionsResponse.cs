namespace SaasCommerce.Modules.Identity.Contracts.Responses;

public sealed record CurrentUserPermissionsResponse(
  Guid UserId,
  Guid BusinessId,
  string Role,
  IReadOnlyCollection<string> Permissions);
