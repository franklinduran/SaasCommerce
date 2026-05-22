namespace SaasCommerce.Api.Endpoints;

public sealed record InventoryTransferListEndpointRequest(
  Guid? SourceBranchId,
  Guid? TargetBranchId,
  string? Status,
  int? Page,
  int? PageSize);
