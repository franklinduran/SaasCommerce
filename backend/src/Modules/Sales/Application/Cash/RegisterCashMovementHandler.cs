using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Contracts.Events.V1;
using SaasCommerce.Modules.Sales.Contracts.Responses;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Application.Cash;

public sealed record RegisterCashMovementCommand(
  Guid CashSessionId,
  string Type,
  decimal Amount,
  string Description);

public sealed class RegisterCashMovementHandler(
  ICashSessionRepository cashSessions,
  ICurrentUserService currentUser,
  IOutboxWriter outbox,
  IUnitOfWork unitOfWork,
  IClock clock)
{
  public async Task<Result<CashMovementResponse>> Handle(
    RegisterCashMovementCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    if (currentUser.BusinessId is not Guid businessId ||
        currentUser.UserId is not Guid userId ||
        currentUser.BranchId is not Guid branchIdValue)
    {
      return Result.Failure<CashMovementResponse>(CashErrors.UserContextRequired);
    }

    if (!Enum.TryParse<CashMovementType>(command.Type, ignoreCase: true, out var movementType))
    {
      return Result.Failure<CashMovementResponse>(CashErrors.InvalidMovementType);
    }

    if (command.Amount <= 0)
    {
      return Result.Failure<CashMovementResponse>(CashErrors.InvalidAmount);
    }

    var bId = new BusinessId(businessId);

    var session = await cashSessions.GetAsync(bId, command.CashSessionId, cancellationToken);
    if (session is null)
    {
      return Result.Failure<CashMovementResponse>(CashErrors.SessionNotFound);
    }

    if (session.Status != CashSessionStatus.Open)
    {
      return Result.Failure<CashMovementResponse>(CashErrors.SessionNotOpen);
    }

    CashMovement movement;

    try
    {
      movement = session.AddMovement(
        Guid.NewGuid(),
        userId,
        movementType,
        command.Amount,
        command.Description,
        clock.UtcNow);
    }
    catch (ArgumentException)
    {
      return Result.Failure<CashMovementResponse>(CashErrors.InvalidAmount);
    }
    catch (InvalidOperationException)
    {
      return Result.Failure<CashMovementResponse>(CashErrors.SessionNotOpen);
    }

    var ev = new CashMovementRegisteredEventV1(
      Guid.NewGuid(),
      Guid.NewGuid(),
      movement.Id,
      session.Id,
      businessId,
      branchIdValue,
      userId,
      movement.Type.ToString(),
      movement.Amount,
      movement.Description,
      movement.CreatedAt);

    await outbox.AddAsync(ev, cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success(CashSessionResponseMapper.ToMovementResponse(movement));
  }
}
