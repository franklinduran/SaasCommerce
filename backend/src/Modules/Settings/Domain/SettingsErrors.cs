using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Settings.Domain;

public static class SettingsErrors
{
  public static readonly DomainError UserContextRequired =
    new("settings.user_context_required", "User context is required.");

  public static readonly DomainError Forbidden =
    new("settings.forbidden", "Only Owner or Admin can manage settings.");

  public static readonly DomainError InvalidCurrency =
    new("settings.invalid_currency", "Currency must be a valid 3-letter ISO 4217 code.");

  public static readonly DomainError InvalidEmail =
    new("settings.invalid_email", "Email address is not valid.");

  public static readonly DomainError InvalidTimezone =
    new("settings.invalid_timezone", "Timezone is not valid.");

  public static readonly DomainError InvalidThreshold =
    new("settings.invalid_threshold", "Low stock threshold must be >= 0.");

  public static readonly DomainError InvalidSequenceStart =
    new("settings.invalid_sequence_start", "Invoice sequence start must be >= 1.");
}
