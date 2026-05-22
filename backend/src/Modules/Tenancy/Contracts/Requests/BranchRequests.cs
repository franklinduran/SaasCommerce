namespace SaasCommerce.Modules.Tenancy.Contracts.Requests;

public sealed record CreateBranchRequest(
  string Name,
  string Code,
  string? Address,
  string? Phone,
  bool IsMain);

public sealed record UpdateBranchRequest(
  string Name,
  string? Address,
  string? Phone);
