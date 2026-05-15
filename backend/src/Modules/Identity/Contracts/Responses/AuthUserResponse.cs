namespace SaasCommerce.Modules.Identity.Contracts.Responses;

public sealed record AuthUserResponse(
  Guid Id,
  Guid BusinessId,
  Guid? BranchId,
  string FullName,
  string Email,
  IReadOnlyCollection<string> Roles);
