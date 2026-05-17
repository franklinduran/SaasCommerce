using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Customers.Application.Abstractions;
using SaasCommerce.Modules.Customers.Contracts.Responses;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Customers.Application.Customers;

public sealed class GetCustomerByIdUseCase(
  ICustomerRepository customers,
  ICurrentUserService currentUser) : IGetCustomerByIdUseCase
{
  public Task<Result<CustomerResponse>> ExecuteAsync(
    GetCustomerByIdQuery query,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(query);

    return ExecuteCoreAsync(query, cancellationToken);
  }

  private async Task<Result<CustomerResponse>> ExecuteCoreAsync(
    GetCustomerByIdQuery query,
    CancellationToken cancellationToken)
  {
    if (currentUser.BusinessId is not Guid businessId)
    {
      return Result.Failure<CustomerResponse>(CustomerErrors.UserContextRequired);
    }

    var customer = await customers.GetAsync(
      new BusinessId(businessId),
      query.CustomerId,
      cancellationToken);

    return customer is null
      ? Result.Failure<CustomerResponse>(CustomerErrors.CustomerNotFound)
      : Result.Success(CustomerResponseMapper.ToResponse(customer));
  }
}
