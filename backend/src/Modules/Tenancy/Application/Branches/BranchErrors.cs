using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Tenancy.Application.Branches;

public static class BranchErrors
{
  public static readonly DomainError UserContextRequired =
    new("BRANCH_USER_CONTEXT_REQUIRED", "Se requiere contexto de usuario autenticado.");

  public static readonly DomainError NotFound =
    new("BRANCH_NOT_FOUND", "La sucursal no fue encontrada.");

  public static readonly DomainError CodeAlreadyExists =
    new("BRANCH_CODE_ALREADY_EXISTS", "Ya existe una sucursal con ese codigo en este negocio.");

  public static readonly DomainError NameAlreadyExists =
    new("BRANCH_NAME_ALREADY_EXISTS", "Ya existe una sucursal con ese nombre en este negocio.");

  public static readonly DomainError MainBranchAlreadyExists =
    new("BRANCH_MAIN_ALREADY_EXISTS", "Este negocio ya tiene una sucursal principal.");

  public static readonly DomainError CannotDeactivateLastActive =
    new("BRANCH_CANNOT_DEACTIVATE_LAST_ACTIVE", "No se puede desactivar la unica sucursal activa.");

  public static readonly DomainError AlreadyActive =
    new("BRANCH_ALREADY_ACTIVE", "La sucursal ya esta activa.");

  public static readonly DomainError AlreadyInactive =
    new("BRANCH_ALREADY_INACTIVE", "La sucursal ya esta inactiva.");
}
