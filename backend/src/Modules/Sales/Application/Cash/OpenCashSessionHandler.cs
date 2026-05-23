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

public sealed record OpenCashSessionCommand(
  decimal OpeningBalance,
  string? Notes);

public sealed class OpenCashSessionHandler(
  ICashSessionRepository cashSessions,
  ICurrentUserService currentUser,
  IOutboxWriter outbox,
  IUnitOfWork unitOfWork,
  IClock clock)
{
  public async Task<Result<CashSessionResponse>> Handle(
    OpenCashSessionCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    if (currentUser.BusinessId is not Guid businessId ||
        currentUser.UserId is not Guid userId ||
        currentUser.BranchId is not Guid branchIdValue)
    {
      return Result.Failure<CashSessionResponse>(CashErrors.UserContextRequired);
    }

    if (command.OpeningBalance < 0)
    {
      return Result.Failure<CashSessionResponse>(CashErrors.InvalidClosingBalance);
    }

    var bId = new BusinessId(businessId);
    var branchId = new BranchId(branchIdValue);

    var hasOpen = await cashSessions.HasOpenSessionAsync(bId, branchId, cancellationToken);
    if (hasOpen)
    {
      return Result.Failure<CashSessionResponse>(CashErrors.SessionAlreadyOpen);
    }

    var now = clock.UtcNow;
    var session = CashSession.Create(
      Guid.NewGuid(),
      bId,
      branchId,
      userId,
      command.OpeningBalance,
      command.Notes,
      now);

    await cashSessions.AddAsync(session, cancellationToken);

    var ev = new CashSessionOpenedEventV1(
      Guid.NewGuid(),
      Guid.NewGuid(),
      session.Id,
      businessId,
      branchIdValue,
      userId,
      session.OpeningBalance,
      now);

    await outbox.AddAsync(ev, cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success(CashSessionResponseMapper.ToResponse(session));
  }
}
