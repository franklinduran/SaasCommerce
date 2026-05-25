namespace SaasCommerce.Modules.Identity.Contracts.Responses;

public sealed record OnboardingStatusResponse(
  bool BusinessInfoCompleted,
  bool ProductsCompleted,
  bool InventoryCompleted,
  bool CashSessionCompleted,
  bool IsComplete,
  int CompletedCount,
  int TotalSteps);
