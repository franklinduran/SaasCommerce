using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Inventory.Application.Transfers;

public static class TransferErrors
{
  public static readonly DomainError UserContextRequired =
    new("TRANSFER_USER_CONTEXT_REQUIRED", "Se requiere contexto de usuario autenticado.");

  public static readonly DomainError NotFound =
    new("TRANSFER_NOT_FOUND", "La transferencia no fue encontrada.");

  public static readonly DomainError SameBranch =
    new("TRANSFER_SAME_BRANCH", "La sucursal origen y destino no pueden ser iguales.");

  public static readonly DomainError NoItems =
    new("TRANSFER_NO_ITEMS", "La transferencia debe tener al menos un producto.");

  public static readonly DomainError InvalidQuantity =
    new("TRANSFER_INVALID_QUANTITY", "La cantidad de cada producto debe ser mayor que cero.");

  public static readonly DomainError CannotCancel =
    new("TRANSFER_CANNOT_CANCEL", "Solo se pueden cancelar transferencias en estado Pendiente o Fallido.");

  public static readonly DomainError InsufficientStock =
    new("TRANSFER_INSUFFICIENT_STOCK", "Stock insuficiente en la sucursal origen para uno o mas productos.");

  public static readonly DomainError ProductNotFound =
    new("TRANSFER_PRODUCT_NOT_FOUND", "Uno o mas productos no tienen stock registrado en la sucursal origen.");
}
