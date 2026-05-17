using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Customers.Application.Abstractions;
using SaasCommerce.Modules.Customers.Contracts.Responses;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Customers.Application.Customers;

public sealed class DeleteCustomerUseCase(
  ICustomerRepository customers,
  ICurrentUserService currentUser,
  IClock clock,
  IUnitOfWork unitOfWork) : IDeleteCustomerUseCase
{
  public Task<Result<CustomerResponse>> ExecuteAsync(
    DeleteCustomerCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    return ExecuteCoreAsync(command, cancellationToken);
  }

  private async Task<Result<CustomerResponse>> ExecuteCoreAsync(
    DeleteCustomerCommand command,
    CancellationToken cancellationToken)
  {
    if (currentUser.BusinessId is not Guid businessId)
    {
      return Result.Failure<CustomerResponse>(CustomerErrors.UserContextRequired);
    }

    var customer = await customers.GetAsync(
      new BusinessId(businessId),
      command.CustomerId,
      cancellationToken);

    if (customer is null)
    {
      return Result.Failure<CustomerResponse>(CustomerErrors.CustomerNotFound);
    }

    customer.Deactivate(clock.UtcNow);
    await unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success(CustomerResponseMapper.ToResponse(customer));
  }
}
