namespace SaasCommerce.Modules.Catalog.Contracts.Responses;

public sealed record ImportProductsResponse(
  int ImportedCount,
  int SkippedCount,
  int TotalRows,
  IReadOnlyCollection<ImportRowError> Errors);

public sealed record ImportRowError(
  int RowNumber,
  string Name,
  IReadOnlyCollection<string> Messages);
