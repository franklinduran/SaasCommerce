using FluentValidation;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Contracts.Events.V1;
using SaasCommerce.Modules.Sales.Domain;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Sales.Application.CashRegisters;

public sealed record OpenCashRegisterCommand(
  Guid BranchId,
  decimal OpeningAmount,
  string? Notes);

public sealed class OpenCashRegisterValidator : AbstractValidator<OpenCashRegisterCommand>
{
  public OpenCashRegisterValidator()
  {
    RuleFor(x => x.BranchId).NotEmpty().WithMessage("La sucursal es requerida.");
    RuleFor(x => x.OpeningAmount).GreaterThanOrEqualTo(0).WithMessage("El monto inicial no puede ser negativo.");
  }
}

public sealed class OpenCashRegisterHandler(
  ICashRegisterRepository registers,
  ICurrentUserService currentUser,
  IOutboxWriter outbox,
  IUnitOfWork unitOfWork,
  IClock clock)
{
  public async Task<Result<OpenCashRegisterResponse>> Handle(
    OpenCashRegisterCommand command,
    CancellationToken cancellationToken = default)
  {
    if (currentUser.BusinessId is not Guid rawBusinessId || currentUser.UserId is not Guid userId)
    {
      return Result.Failure<OpenCashRegisterResponse>(CashRegisterErrors.UserContextRequired);
    }

    if (command.OpeningAmount < 0)
    {
      return Result.Failure<OpenCashRegisterResponse>(CashRegisterErrors.InvalidOpeningAmount);
    }

    var businessId = new BusinessId(rawBusinessId);
    var branchId = new BranchId(command.BranchId);

    var hasOpen = await registers.HasOpenRegisterAsync(businessId, userId, cancellationToken);
    if (hasOpen)
    {
      return Result.Failure<OpenCashRegisterResponse>(CashRegisterErrors.RegisterAlreadyOpen);
    }

    var now = clock.UtcNow;
    var register = CashRegister.Open(
      Guid.NewGuid(),
      businessId,
      branchId,
      userId,
      command.OpeningAmount,
      command.Notes,
      now);

    await registers.AddAsync(register, cancellationToken);

    await outbox.AddAsync(
      new CashRegisterOpenedEventV1(
        Guid.NewGuid(),
        Guid.NewGuid(),
        register.Id,
        rawBusinessId,
        command.BranchId,
        userId,
        command.OpeningAmount,
        now),
      cancellationToken);

    await unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success(new OpenCashRegisterResponse(register.Id, register.OpenedAt));
  }
}

public sealed record OpenCashRegisterResponse(Guid CashRegisterId, DateTimeOffset OpenedAt);
