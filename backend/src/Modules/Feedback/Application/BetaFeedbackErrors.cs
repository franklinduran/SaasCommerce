using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Feedback.Application;

public static class BetaFeedbackErrors
{
  public static readonly DomainError UserContextRequired =
    new("beta_feedback.user_context_required", "Se requiere contexto de usuario autenticado.");

  public static readonly DomainError InvalidFeedback =
    new("beta_feedback.invalid", "El feedback beta no es valido.");

  public static readonly DomainError FeedbackNotFound =
    new("beta_feedback.not_found", "No se encontro el feedback beta.");
}
