using SaasCommerce.Modules.Sales.Domain;

namespace SaasCommerce.Modules.Sales.Application.CashRegisters;

public static class CashRegisterResponseMapper
{
  public static CashRegisterDetailResponse ToDetail(CashRegister register)
  {
    ArgumentNullException.ThrowIfNull(register);

    return new CashRegisterDetailResponse(
      register.Id,
      register.BranchId.Value,
      register.UserId,
      register.Status.ToString(),
      register.OpeningAmount,
      register.OpenedAt,
      register.ClosedAt,
      register.CountedAmount,
      register.ExpectedCashAmount,
      register.Difference,
      register.DifferenceType?.ToString(),
      register.CashSalesTotal,
      register.CardSalesTotal,
      register.TransferSalesTotal,
      register.CreditSalesTotal,
      register.CashReturnsTotal,
      register.ManualCashIn,
      register.ManualCashOut,
      register.Notes,
      register.CloseNotes,
      register.Movements
        .OrderByDescending(m => m.CreatedAt)
        .Select(m => new CashRegisterMovementResponse(
          m.Id,
          m.MovementType.ToString(),
          m.Amount,
          m.Reason,
          m.CreatedAt))
        .ToArray());
  }
}

public sealed record CashRegisterDetailResponse(
  Guid Id,
  Guid BranchId,
  Guid UserId,
  string Status,
  decimal OpeningAmount,
  DateTimeOffset OpenedAt,
  DateTimeOffset? ClosedAt,
  decimal? CountedAmount,
  decimal? ExpectedCashAmount,
  decimal? Difference,
  string? DifferenceType,
  decimal CashSalesTotal,
  decimal CardSalesTotal,
  decimal TransferSalesTotal,
  decimal CreditSalesTotal,
  decimal CashReturnsTotal,
  decimal ManualCashIn,
  decimal ManualCashOut,
  string? Notes,
  string? CloseNotes,
  IReadOnlyCollection<CashRegisterMovementResponse> Movements);
