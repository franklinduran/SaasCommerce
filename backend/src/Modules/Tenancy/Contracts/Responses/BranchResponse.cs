namespace SaasCommerce.Modules.Tenancy.Contracts.Responses;

public sealed record BranchResponse(
  Guid Id,
  Guid BusinessId,
  string Name,
  string Code,
  string? Address,
  string? Phone,
  bool IsMain,
  bool IsActive,
  DateTimeOffset CreatedAt,
  DateTimeOffset UpdatedAt);

public sealed record BranchListResponse(
  IReadOnlyCollection<BranchResponse> Items,
  int Total);
