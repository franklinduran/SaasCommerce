using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Customers.Application.Abstractions;
using SaasCommerce.Modules.Customers.Contracts.Responses;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Customers.Application.Customers;

public sealed class UpdateCustomerUseCase(
  ICustomerRepository customers,
  ICurrentUserService currentUser,
  IClock clock,
  IUnitOfWork unitOfWork) : IUpdateCustomerUseCase
{
  public Task<Result<CustomerResponse>> ExecuteAsync(
    UpdateCustomerCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    return ExecuteCoreAsync(command, cancellationToken);
  }

  private async Task<Result<CustomerResponse>> ExecuteCoreAsync(
    UpdateCustomerCommand command,
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

    try
    {
      customer.Update(
        command.FirstName,
        command.LastName,
        command.Phone,
        command.Email,
        command.IsActive,
        clock.UtcNow);
    }
    catch (ArgumentOutOfRangeException)
    {
      return Result.Failure<CustomerResponse>(CustomerErrors.InvalidCustomer);
    }
    catch (ArgumentException)
    {
      return Result.Failure<CustomerResponse>(CustomerErrors.InvalidCustomer);
    }

    await unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success(CustomerResponseMapper.ToResponse(customer));
  }
}
