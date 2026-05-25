using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Identity.Application.Onboarding;

/// <summary>
/// Computes the onboarding status for a business by querying multiple domain sources.
/// </summary>
public interface IOnboardingStatusReader
{
  Task<OnboardingStatusSummary> ReadAsync(
    BusinessId businessId,
    CancellationToken cancellationToken = default);
}

/// <summary>
/// Summary of which onboarding steps have been completed for a business.
/// </summary>
public sealed record OnboardingStatusSummary(
  bool BusinessInfoCompleted,
  bool ProductsCompleted,
  bool InventoryCompleted,
  bool CashSessionCompleted)
{
  public bool IsComplete =>
    BusinessInfoCompleted &&
    ProductsCompleted &&
    InventoryCompleted &&
    CashSessionCompleted;

  public int CompletedCount =>
    (BusinessInfoCompleted ? 1 : 0) +
    (ProductsCompleted ? 1 : 0) +
    (InventoryCompleted ? 1 : 0) +
    (CashSessionCompleted ? 1 : 0);

  public static int TotalSteps => 4;
}
