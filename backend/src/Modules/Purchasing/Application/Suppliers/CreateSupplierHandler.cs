using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Purchasing.Application.Abstractions;
using SaasCommerce.Modules.Purchasing.Contracts.Responses;
using SaasCommerce.Modules.Purchasing.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Purchasing.Application.Suppliers;

public sealed class CreateSupplierHandler(
  ISupplierRepository suppliers,
  ICurrentUserService currentUser,
  IClock clock,
  IUnitOfWork unitOfWork)
{
  public Task<Result<SupplierResponse>> Handle(
    CreateSupplierCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    return HandleCoreAsync(command, cancellationToken);
  }

  private async Task<Result<SupplierResponse>> HandleCoreAsync(
    CreateSupplierCommand command,
    CancellationToken cancellationToken)
  {
    if (currentUser.BusinessId is not Guid businessId)
    {
      return Result.Failure<SupplierResponse>(SupplierErrors.UserContextRequired);
    }

    Supplier supplier;

    try
    {
      supplier = new Supplier(
        Guid.NewGuid(),
        new BusinessId(businessId),
        command.Name,
        command.Rnc,
        command.Phone,
        command.Email,
        command.Address,
        clock.UtcNow);
    }
    catch (ArgumentOutOfRangeException)
    {
      return Result.Failure<SupplierResponse>(SupplierErrors.InvalidSupplier);
    }
    catch (ArgumentException)
    {
      return Result.Failure<SupplierResponse>(SupplierErrors.InvalidSupplier);
    }

    await suppliers.AddAsync(supplier, cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success(SupplierResponseMapper.ToResponse(supplier));
  }
}
