namespace SaasCommerce.Modules.Tenancy.Application.Branches;

public sealed record UpdateBranchCommand(
  Guid BranchId,
  string Name,
  string? Address,
  string? Phone);
