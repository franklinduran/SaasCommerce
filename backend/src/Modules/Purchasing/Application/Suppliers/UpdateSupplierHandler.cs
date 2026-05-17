using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Purchasing.Application.Abstractions;
using SaasCommerce.Modules.Purchasing.Contracts.Responses;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Purchasing.Application.Suppliers;

public sealed class UpdateSupplierHandler(
  ISupplierRepository suppliers,
  ICurrentUserService currentUser,
  IClock clock,
  IUnitOfWork unitOfWork)
{
  public Task<Result<SupplierResponse>> Handle(
    UpdateSupplierCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    return HandleCoreAsync(command, cancellationToken);
  }

  private async Task<Result<SupplierResponse>> HandleCoreAsync(
    UpdateSupplierCommand command,
    CancellationToken cancellationToken)
  {
    if (currentUser.BusinessId is not Guid businessId)
    {
      return Result.Failure<SupplierResponse>(SupplierErrors.UserContextRequired);
    }

    var supplier = await suppliers.GetAsync(
      new BusinessId(businessId),
      command.SupplierId,
      cancellationToken);

    if (supplier is null)
    {
      return Result.Failure<SupplierResponse>(SupplierErrors.SupplierNotFound);
    }

    try
    {
      supplier.Update(
        command.Name,
        command.Rnc,
        command.Phone,
        command.Email,
        command.Address,
        command.IsActive,
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

    await unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success(SupplierResponseMapper.ToResponse(supplier));
  }
}
