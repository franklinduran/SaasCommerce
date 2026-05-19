namespace SaasCommerce.Modules.Identity.Contracts.Requests;

public sealed record CreateUserRequest(
  string FullName,
  string Email,
  string Password,
  string Role,
  Guid? DefaultBranchId);
