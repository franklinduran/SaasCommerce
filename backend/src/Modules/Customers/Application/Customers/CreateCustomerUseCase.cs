using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Customers.Application.Abstractions;
using SaasCommerce.Modules.Customers.Contracts.Responses;
using SaasCommerce.Modules.Customers.Domain;
using SaasCommerce.Modules.Customers.Domain.Credits;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Customers.Application.Customers;

public sealed class CreateCustomerUseCase(
  ICustomerRepository customers,
  ICurrentUserService currentUser,
  IClock clock,
  IUnitOfWork unitOfWork,
  ICustomerCreditRepository? credits = null) : ICreateCustomerUseCase
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

    if (!string.IsNullOrWhiteSpace(command.Cedula) &&
        await customers.ExistsByCedulaAsync(new BusinessId(businessId), command.Cedula.Trim(), cancellationToken: cancellationToken))
    {
      return Result.Failure<CustomerResponse>(CustomerErrors.DuplicateCedula);
    }

    Customer customer;

    try
    {
      customer = new Customer(
        Guid.NewGuid(),
        new BusinessId(businessId),
        command.FirstName,
        command.LastName,
        command.Phone,
        command.Email,
        clock.UtcNow,
        command.Cedula);
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

    CustomerCreditAccount? creditAccount = null;

    if (credits is not null)
    {
      creditAccount = new CustomerCreditAccount(
        Guid.NewGuid(),
        customer.BusinessId,
        customer.Id,
        0,
        clock.UtcNow);
      await credits.AddAccountAsync(creditAccount, cancellationToken);
    }

    await unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success(CustomerResponseMapper.ToResponse(customer, creditAccount));
  }
}
