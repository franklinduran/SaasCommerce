namespace SaasCommerce.Modules.Identity.Application.Users;

public sealed record UpdateUserCommand(
  Guid UserId,
  string FullName,
  string? Phone,
  Guid? DefaultBranchId);
