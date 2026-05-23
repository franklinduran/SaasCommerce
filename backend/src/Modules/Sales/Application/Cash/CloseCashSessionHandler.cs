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

public sealed record CloseCashSessionCommand(
  Guid CashSessionId,
  decimal ClosingBalance);

public sealed class CloseCashSessionHandler(
  ICashSessionRepository cashSessions,
  ICurrentUserService currentUser,
  IOutboxWriter outbox,
  IUnitOfWork unitOfWork,
  IClock clock)
{
  public async Task<Result<CashClosingResultResponse>> Handle(
    CloseCashSessionCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    if (currentUser.BusinessId is not Guid businessId ||
        currentUser.UserId is not Guid userId ||
        currentUser.BranchId is not Guid branchIdValue)
    {
      return Result.Failure<CashClosingResultResponse>(CashErrors.UserContextRequired);
    }

    if (command.ClosingBalance < 0)
    {
      return Result.Failure<CashClosingResultResponse>(CashErrors.InvalidClosingBalance);
    }

    var bId = new BusinessId(businessId);

    var session = await cashSessions.GetAsync(bId, command.CashSessionId, cancellationToken);
    if (session is null)
    {
      return Result.Failure<CashClosingResultResponse>(CashErrors.SessionNotFound);
    }

    if (session.Status != CashSessionStatus.Open)
    {
      return Result.Failure<CashClosingResultResponse>(CashErrors.SessionNotOpen);
    }

    CashSessionClosingResult closingResult;

    try
    {
      closingResult = session.Close(command.ClosingBalance, clock.UtcNow);
    }
    catch (ArgumentException)
    {
      return Result.Failure<CashClosingResultResponse>(CashErrors.InvalidClosingBalance);
    }
    catch (InvalidOperationException)
    {
      return Result.Failure<CashClosingResultResponse>(CashErrors.SessionNotOpen);
    }

    var ev = new CashSessionClosedEventV1(
      Guid.NewGuid(),
      Guid.NewGuid(),
      session.Id,
      businessId,
      branchIdValue,
      userId,
      closingResult.OpeningBalance,
      closingResult.SystemBalance,
      closingResult.ClosingBalance,
      closingResult.Difference,
      closingResult.Outcome,
      session.ClosedAt!.Value);

    await outbox.AddAsync(ev, cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success(new CashClosingResultResponse(
      session.Id,
      closingResult.OpeningBalance,
      closingResult.SystemBalance,
      closingResult.ClosingBalance,
      closingResult.Difference,
      closingResult.Outcome));
  }
}
