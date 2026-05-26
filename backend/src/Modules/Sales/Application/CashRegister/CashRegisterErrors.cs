using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Sales.Application.CashRegisters;

public static class CashRegisterErrors
{
  public static readonly DomainError UserContextRequired =
    new("CashRegister.UserContextRequired", "Se requiere contexto de usuario autenticado.");

  public static readonly DomainError RegisterAlreadyOpen =
    new("CashRegister.AlreadyOpen", "Ya existe una caja abierta para este usuario en esta sucursal.");

  public static readonly DomainError RegisterNotFound =
    new("CashRegister.NotFound", "No se encontró la caja especificada.");

  public static readonly DomainError RegisterNotOpen =
    new("CashRegister.NotOpen", "La caja no está abierta.");

  public static readonly DomainError NoOpenRegister =
    new("CashRegister.NoOpenRegister", "No existe una caja abierta para este usuario.");

  public static readonly DomainError InvalidOpeningAmount =
    new("CashRegister.InvalidOpeningAmount", "El monto inicial no puede ser negativo.");

  public static readonly DomainError InvalidMovementAmount =
    new("CashRegister.InvalidMovementAmount", "El monto del movimiento debe ser mayor a cero.");

  public static readonly DomainError InvalidMovementType =
    new("CashRegister.InvalidMovementType", "Tipo de movimiento inválido. Use 'CashIn' o 'CashOut'.");

  public static readonly DomainError InvalidCountedAmount =
    new("CashRegister.InvalidCountedAmount", "El monto contado no puede ser negativo.");
}
