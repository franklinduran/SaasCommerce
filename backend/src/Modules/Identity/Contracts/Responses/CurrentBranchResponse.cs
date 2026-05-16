namespace SaasCommerce.Modules.Identity.Contracts.Responses;

public sealed record CurrentBranchResponse(
  Guid BranchId,
  Guid BusinessId,
  string Name,
  string? Address,
  string? Phone);
