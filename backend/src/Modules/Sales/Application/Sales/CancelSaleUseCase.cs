using SaasCommerce.BuildingBlocks.Application.Abstractions.Audit;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Realtime;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Contracts.Events.V1;
using SaasCommerce.Modules.Sales.Contracts.Responses;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Application.Sales;

public sealed class CancelSaleUseCase(
  ISaleRepository sales,
  ICurrentUserService currentUser,
  IRealtimeNotifier realtime,
  IAuditLogWriter auditLog,
  IClock clock,
  IUnitOfWork unitOfWork) : ICancelSaleUseCase
{
  public Task<Result<SaleResponse>> ExecuteAsync(
    CancelSaleCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    return ExecuteCoreAsync(command, cancellationToken);
  }

  private async Task<Result<SaleResponse>> ExecuteCoreAsync(
    CancelSaleCommand command,
    CancellationToken cancellationToken)
  {
    if (currentUser.BusinessId is not Guid businessId ||
        currentUser.UserId is not Guid userId)
    {
      return Result.Failure<SaleResponse>(SalesErrors.UserContextRequired);
    }

    var sale = await sales.GetAsync(
      new BusinessId(businessId),
      command.SaleId,
      cancellationToken);

    if (sale is null)
    {
      return Result.Failure<SaleResponse>(SalesErrors.SaleNotFound);
    }

    var reason = string.IsNullOrWhiteSpace(command.Reason)
      ? "Sale cancelled."
      : command.Reason.Trim();

    try
    {
      sale.Cancel(reason, clock.UtcNow);
    }
    catch (InvalidOperationException)
    {
      return Result.Failure<SaleResponse>(SalesErrors.InvalidSaleState);
    }

    await unitOfWork.SaveChangesAsync(cancellationToken);

    await auditLog.WriteAsync(
      new BusinessId(businessId),
      userId,
      "sale.cancelled",
      "Sale",
      sale.Id,
      $"Sale cancelled. Reason: {reason}",
      cancellationToken: cancellationToken);

    await realtime.NotifyBusinessAsync(
      businessId,
      SaleRealtimeEvents.StatusChanged,
      new SaleStatusChangedNotificationV1(
        Guid.NewGuid(),
        Guid.NewGuid(),
        sale.Id,
        businessId,
        sale.BranchId.Value,
        userId,
        SaleStatus.Cancelled.ToString(),
        reason,
        clock.UtcNow),
      cancellationToken);

    return Result.Success(SaleResponseMapper.ToResponse(sale));
  }
}
