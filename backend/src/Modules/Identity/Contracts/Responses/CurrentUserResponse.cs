namespace SaasCommerce.Modules.Identity.Contracts.Responses;

public sealed record CurrentUserResponse(
  Guid UserId,
  string FullName,
  string Email,
  string? Phone,
  IReadOnlyCollection<string> Roles,
  CurrentUserBusinessResponse Business,
  CurrentUserBranchResponse? Branch);

public sealed record CurrentUserBusinessResponse(
  Guid BusinessId,
  string Name,
  string? IdentificationType,
  string? IdentificationNumber);

public sealed record CurrentUserBranchResponse(Guid BranchId, string Name);
