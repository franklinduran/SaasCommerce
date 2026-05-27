using FluentValidation;
using Microsoft.Extensions.Logging;
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

public sealed record RegisterCashRegisterMovementCommand(
  Guid CashRegisterId,
  string MovementType,
  decimal Amount,
  string Reason);

public sealed class RegisterCashRegisterMovementValidator : AbstractValidator<RegisterCashRegisterMovementCommand>
{
  public RegisterCashRegisterMovementValidator()
  {
    RuleFor(x => x.CashRegisterId).NotEmpty();
    RuleFor(x => x.MovementType).NotEmpty().WithMessage("El tipo de movimiento es requerido.");
    RuleFor(x => x.Amount).GreaterThan(0).WithMessage("El monto debe ser mayor a cero.");
    RuleFor(x => x.Reason).NotEmpty().WithMessage("El motivo es requerido.");
  }
}

public sealed class RegisterCashRegisterMovementHandler(
  ICashRegisterRepository registers,
  ICurrentUserService currentUser,
  IOutboxWriter outbox,
  IUnitOfWork unitOfWork,
  IClock clock,
  ILogger<RegisterCashRegisterMovementHandler> logger)
{
  private static readonly Action<ILogger, Guid, Guid, Guid, string, decimal, Exception?> LogMovementRegistered =
    LoggerMessage.Define<Guid, Guid, Guid, string, decimal>(
      LogLevel.Information,
      new EventId(3102, nameof(LogMovementRegistered)),
      "Cash register movement registered. MovementId={MovementId} CashRegisterId={CashRegisterId} BusinessId={BusinessId} MovementType={MovementType} Amount={Amount}");

  public async Task<Result<CashRegisterMovementResponse>> Handle(
    RegisterCashRegisterMovementCommand command,
    CancellationToken cancellationToken = default)
  {
    if (currentUser.BusinessId is not Guid rawBusinessId || currentUser.UserId is not Guid userId)
    {
      return Result.Failure<CashRegisterMovementResponse>(CashRegisterErrors.UserContextRequired);
    }

    if (command.Amount <= 0)
    {
      return Result.Failure<CashRegisterMovementResponse>(CashRegisterErrors.InvalidMovementAmount);
    }

    if (!Enum.TryParse<CashMovementType>(command.MovementType, ignoreCase: true, out var movementType))
    {
      return Result.Failure<CashRegisterMovementResponse>(CashRegisterErrors.InvalidMovementType);
    }

    var businessId = new BusinessId(rawBusinessId);
    var register = await registers.GetAsync(businessId, command.CashRegisterId, cancellationToken);
    if (register is null)
    {
      return Result.Failure<CashRegisterMovementResponse>(CashRegisterErrors.RegisterNotFound);
    }

    if (register.Status != CashRegisterStatus.Open)
    {
      return Result.Failure<CashRegisterMovementResponse>(CashRegisterErrors.RegisterNotOpen);
    }

    var now = clock.UtcNow;
    var movement = register.AddMovement(
      Guid.NewGuid(),
      userId,
      movementType,
      command.Amount,
      command.Reason,
      now);

    await outbox.AddAsync(
      new CashRegisterMovementRegisteredEventV1(
        Guid.NewGuid(),
        Guid.NewGuid(),
        movement.Id,
        register.Id,
        rawBusinessId,
        register.BranchId.Value,
        userId,
        movement.MovementType.ToString(),
        movement.Amount,
        movement.Reason,
        now),
      cancellationToken);

    await unitOfWork.SaveChangesAsync(cancellationToken);

    LogMovementRegistered(logger, movement.Id, register.Id, rawBusinessId, movement.MovementType.ToString(), movement.Amount, null);

    return Result.Success(new CashRegisterMovementResponse(
      movement.Id,
      movement.MovementType.ToString(),
      movement.Amount,
      movement.Reason,
      movement.CreatedAt));
  }
}

public sealed record CashRegisterMovementResponse(
  Guid Id,
  string MovementType,
  decimal Amount,
  string Reason,
  DateTimeOffset CreatedAt);
