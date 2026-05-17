using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Realtime;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Customers.Application.Abstractions;
using SaasCommerce.Modules.Customers.Contracts.Responses;
using SaasCommerce.Modules.Customers.Domain.Credits;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Customers.Application.Credits;

public sealed class UnblockCustomerCreditUseCase(
  ICustomerRepository customers,
  ICustomerCreditRepository credits,
  ICurrentUserService currentUser,
  IRealtimeNotifier realtime,
  IClock clock,
  IUnitOfWork unitOfWork) : IUnblockCustomerCreditUseCase
{
  public Task<Result<CustomerCreditSummaryResponse>> ExecuteAsync(
    UnblockCustomerCreditCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    return ChangeStatusAsync(command.CustomerId, false, cancellationToken);
  }

  private async Task<Result<CustomerCreditSummaryResponse>> ChangeStatusAsync(
    Guid customerId,
    bool block,
    CancellationToken cancellationToken)
  {
    if (currentUser.BusinessId is not Guid businessId)
    {
      return Result.Failure<CustomerCreditSummaryResponse>(CustomerCreditErrors.UserContextRequired);
    }

    var tenant = new BusinessId(businessId);
    var customer = await customers.GetAsync(tenant, customerId, cancellationToken);

    if (customer is null)
    {
      return Result.Failure<CustomerCreditSummaryResponse>(CustomerCreditErrors.CustomerNotFound);
    }

    var account = await credits.GetAccountAsync(tenant, customerId, cancellationToken);

    if (account is null)
    {
      account = new CustomerCreditAccount(Guid.NewGuid(), tenant, customerId, 0, clock.UtcNow);
      await credits.AddAccountAsync(account, cancellationToken);
    }

    try
    {
      if (block)
      {
        account.Block(clock.UtcNow);
      }
      else
      {
        account.Unblock(clock.UtcNow);
      }
    }
    catch (InvalidOperationException)
    {
      return Result.Failure<CustomerCreditSummaryResponse>(CustomerCreditErrors.InvalidCreditOperation);
    }

    await unitOfWork.SaveChangesAsync(cancellationToken);
    await realtime.NotifyBusinessAsync(
      businessId,
      CustomerCreditRealtimeEvents.CreditUnblocked,
      new CustomerCreditStatusChangedNotificationV1(
        Guid.NewGuid(),
        businessId,
        customerId,
        account.Status.ToString(),
        clock.UtcNow),
      cancellationToken);

    return Result.Success(CustomerCreditResponseMapper.ToSummary(customer, account));
  }
}
