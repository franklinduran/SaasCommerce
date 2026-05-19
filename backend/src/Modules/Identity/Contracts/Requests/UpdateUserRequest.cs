namespace SaasCommerce.Modules.Identity.Contracts.Requests;

public sealed record UpdateUserRequest(
  string FullName,
  string? Phone,
  Guid? DefaultBranchId);
