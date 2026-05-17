using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Customers.Application.Abstractions;
using SaasCommerce.Modules.Customers.Contracts.Responses;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Customers.Application.Credits;

public sealed class GetCustomerCreditMovementsUseCase(
  ICustomerRepository customers,
  ICustomerCreditRepository credits,
  ICurrentUserService currentUser) : IGetCustomerCreditMovementsUseCase
{
  private static readonly int[] AllowedPageSizes = [10, 25, 50, 100];

  public async Task<Result<CustomerCreditMovementListResponse>> ExecuteAsync(
    GetCustomerCreditMovementsQuery query,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(query);

    if (query.Page < 1 || !AllowedPageSizes.Contains(query.PageSize))
    {
      return Result.Failure<CustomerCreditMovementListResponse>(CustomerCreditErrors.InvalidCreditOperation);
    }

    var tenant = ResolveTenant();

    if (tenant.IsFailure)
    {
      return Result.Failure<CustomerCreditMovementListResponse>(tenant.Error);
    }

    var customer = await customers.GetAsync(tenant.Value, query.CustomerId, cancellationToken);

    if (customer is null)
    {
      return Result.Failure<CustomerCreditMovementListResponse>(CustomerCreditErrors.CustomerNotFound);
    }

    var totalItems = await credits.CountMovementsAsync(tenant.Value, query.CustomerId, cancellationToken);
    var movements = await credits.ListMovementsAsync(
      tenant.Value,
      query.CustomerId,
      query.Page,
      query.PageSize,
      cancellationToken);
    var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)query.PageSize);

    return Result.Success(new CustomerCreditMovementListResponse(
      movements.Select(CustomerCreditResponseMapper.ToMovement).ToArray(),
      query.Page,
      query.PageSize,
      totalItems,
      totalPages,
      query.Page > 1,
      totalPages > 0 && query.Page < totalPages));
  }

  private Result<BusinessId> ResolveTenant()
    => currentUser.BusinessId is Guid businessId
      ? Result.Success(new BusinessId(businessId))
      : Result.Failure<BusinessId>(CustomerCreditErrors.UserContextRequired);
}
