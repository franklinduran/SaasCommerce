using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Notifications.Application;

internal static class NotificationErrors
{
  internal static readonly DomainError UserContextRequired =
    new("notifications.user_context_required", "Se requiere contexto de usuario autenticado.");

  internal static readonly DomainError NotFound =
    new("notifications.not_found", "No se encontró la notificación.");

  internal static readonly DomainError Forbidden =
    new("forbidden", "No tiene permiso para acceder a esta notificación.");
}
