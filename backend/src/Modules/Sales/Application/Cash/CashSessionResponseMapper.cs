using SaasCommerce.Modules.Sales.Contracts.Responses;
using SaasCommerce.Modules.Sales.Domain;

namespace SaasCommerce.Modules.Sales.Application.Cash;

public static class CashSessionResponseMapper
{
  public static CashSessionResponse ToResponse(CashSession session)
  {
    ArgumentNullException.ThrowIfNull(session);

    return new CashSessionResponse(
      session.Id,
      session.BusinessId.Value,
      session.BranchId.Value,
      session.UserId,
      session.Status.ToString(),
      session.OpeningBalance,
      session.ClosingBalance,
      session.SystemBalance,
      session.Notes,
      session.OpenedAt,
      session.ClosedAt,
      session.UpdatedAt,
      session.Movements.Select(ToMovementResponse).ToArray());
  }

  public static CashSessionListResponse ToListResponse(CashSession session)
  {
    ArgumentNullException.ThrowIfNull(session);

    return new CashSessionListResponse(
      session.Id,
      session.BranchId.Value,
      session.UserId,
      session.Status.ToString(),
      session.OpeningBalance,
      session.ClosingBalance,
      session.SystemBalance,
      session.OpenedAt,
      session.ClosedAt);
  }

  public static CashMovementResponse ToMovementResponse(CashMovement movement)
  {
    ArgumentNullException.ThrowIfNull(movement);

    return new CashMovementResponse(
      movement.Id,
      movement.CashSessionId,
      movement.UserId,
      movement.Type.ToString(),
      movement.Amount,
      movement.Description,
      movement.CreatedAt);
  }
}
