namespace SaasCommerce.Modules.Tenancy.Application.Branches;

public sealed record CreateBranchCommand(
  string Name,
  string Code,
  string? Address,
  string? Phone,
  bool IsMain);
