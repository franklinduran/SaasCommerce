namespace SaasCommerce.Modules.Identity.Application.Onboarding;

/// <summary>
/// Steps in the business onboarding wizard.
/// Each step represents a setup milestone that can be completed or pending.
/// </summary>
public enum OnboardingStep
{
  BusinessInfo = 1,
  Products = 2,
  Inventory = 3,
  CashSession = 4
}
