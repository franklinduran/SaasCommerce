using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Customers.Application.Abstractions;
using SaasCommerce.Modules.Customers.Contracts.Responses;
using SaasCommerce.Modules.Customers.Domain.Credits;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Customers.Application.Credits;

public sealed class GetCustomerCreditSummaryUseCase(
  ICustomerRepository customers,
  ICustomerCreditRepository credits,
  ICurrentUserService currentUser) : IGetCustomerCreditSummaryUseCase
{
  public async Task<Result<CustomerCreditSummaryResponse>> ExecuteAsync(
    GetCustomerCreditSummaryQuery query,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(query);

    var tenant = ResolveTenant();

    if (tenant.IsFailure)
    {
      return Result.Failure<CustomerCreditSummaryResponse>(tenant.Error);
    }

    var customer = await customers.GetAsync(tenant.Value, query.CustomerId, cancellationToken);

    if (customer is null)
    {
      return Result.Failure<CustomerCreditSummaryResponse>(CustomerCreditErrors.CustomerNotFound);
    }

    var account = await credits.GetAccountAsync(tenant.Value, query.CustomerId, cancellationToken) ??
      new CustomerCreditAccount(Guid.NewGuid(), tenant.Value, query.CustomerId, 0, customer.CreatedAt);

    return Result.Success(CustomerCreditResponseMapper.ToSummary(customer, account));
  }

  private Result<BusinessId> ResolveTenant()
    => currentUser.BusinessId is Guid businessId
      ? Result.Success(new BusinessId(businessId))
      : Result.Failure<BusinessId>(CustomerCreditErrors.UserContextRequired);
}
