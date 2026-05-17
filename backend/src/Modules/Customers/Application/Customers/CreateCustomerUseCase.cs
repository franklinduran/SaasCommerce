using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Customers.Application.Abstractions;
using SaasCommerce.Modules.Customers.Contracts.Responses;
using SaasCommerce.Modules.Customers.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Customers.Application.Customers;

public sealed class CreateCustomerUseCase(
  ICustomerRepository customers,
  ICurrentUserService currentUser,
  IClock clock,
  IUnitOfWork unitOfWork) : ICreateCustomerUseCase
{
  public Task<Result<CustomerResponse>> ExecuteAsync(
    CreateCustomerCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    return ExecuteCoreAsync(command, cancellationToken);
  }

  private async Task<Result<CustomerResponse>> ExecuteCoreAsync(
    CreateCustomerCommand command,
    CancellationToken cancellationToken)
  {
    if (currentUser.BusinessId is not Guid businessId)
    {
      return Result.Failure<CustomerResponse>(CustomerErrors.UserContextRequired);
    }

    Customer customer;

    try
    {
      customer = new Customer(
        Guid.NewGuid(),
        new BusinessId(businessId),
        command.FullName,
        command.Phone,
        command.Email,
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

    await customers.AddAsync(customer, cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success(CustomerResponseMapper.ToResponse(customer));
  }
}
