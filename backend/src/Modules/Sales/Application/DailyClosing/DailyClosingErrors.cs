using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Sales.Application.DailyClosings;

public static class DailyClosingErrors
{
  public static readonly DomainError UserContextRequired =
    new("daily_closing.user_context_required", "Se requiere contexto de usuario autenticado.");

  public static readonly DomainError BranchRequired =
    new("daily_closing.branch_required", "Se requiere especificar una sucursal.");

  public static readonly DomainError NotFound =
    new("daily_closing.not_found", "El cierre operativo no fue encontrado.");

  public static readonly DomainError AlreadyClosed =
    new("daily_closing.already_closed", "Ya existe un cierre cerrado para esta fecha y sucursal.");

  public static readonly DomainError AlreadyClosedStatus =
    new("daily_closing.already_closed_status", "Este cierre ya fue completado y no puede modificarse.");

  public static readonly DomainError OpenCashSessionsExist =
    new("daily_closing.open_cash_sessions", "Existen sesiones de caja abiertas. Cierra la caja antes de cerrar el día.");
}
