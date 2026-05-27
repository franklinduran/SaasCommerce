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

public sealed record CloseCashRegisterCommand(
  Guid CashRegisterId,
  decimal CountedAmount,
  string? CloseNotes);

public sealed class CloseCashRegisterValidator : AbstractValidator<CloseCashRegisterCommand>
{
  public CloseCashRegisterValidator()
  {
    RuleFor(x => x.CashRegisterId).NotEmpty();
    RuleFor(x => x.CountedAmount).GreaterThanOrEqualTo(0).WithMessage("El monto contado no puede ser negativo.");
  }
}

public sealed class CloseCashRegisterHandler(
  ICashRegisterRepository registers,
  ICashRegisterCalculator calculator,
  ICurrentUserService currentUser,
  IOutboxWriter outbox,
  IUnitOfWork unitOfWork,
  IClock clock,
  ILogger<CloseCashRegisterHandler> logger)
{
  private static readonly Action<ILogger, Guid, Guid, string, Exception?> LogClosed =
    LoggerMessage.Define<Guid, Guid, string>(
      LogLevel.Information,
      new EventId(3101, nameof(LogClosed)),
      "Cash register closed. CashRegisterId={CashRegisterId} BusinessId={BusinessId} DifferenceType={DifferenceType}");

  public async Task<Result<CloseCashRegisterResponse>> Handle(
    CloseCashRegisterCommand command,
    CancellationToken cancellationToken = default)
  {
    if (currentUser.BusinessId is not Guid rawBusinessId)
    {
      return Result.Failure<CloseCashRegisterResponse>(CashRegisterErrors.UserContextRequired);
    }

    if (command.CountedAmount < 0)
    {
      return Result.Failure<CloseCashRegisterResponse>(CashRegisterErrors.InvalidCountedAmount);
    }

    var businessId = new BusinessId(rawBusinessId);
    var register = await registers.GetAsync(businessId, command.CashRegisterId, cancellationToken);
    if (register is null)
    {
      return Result.Failure<CloseCashRegisterResponse>(CashRegisterErrors.RegisterNotFound);
    }

    if (register.Status != CashRegisterStatus.Open)
    {
      return Result.Failure<CloseCashRegisterResponse>(CashRegisterErrors.RegisterNotOpen);
    }

    var now = clock.UtcNow;
    var totals = await calculator.CalculateAsync(
      businessId,
      register.BranchId,
      register.OpenedAt,
      now,
      cancellationToken);

    var closing = register.Close(command.CountedAmount, totals, now, command.CloseNotes);

    await outbox.AddAsync(
      new CashRegisterClosedEventV1(
        Guid.NewGuid(),
        Guid.NewGuid(),
        register.Id,
        rawBusinessId,
        register.BranchId.Value,
        register.UserId,
        register.OpeningAmount,
        closing.CashSales,
        closing.CardSales,
        closing.TransferSales,
        closing.CreditSales,
        closing.CashReturns,
        closing.ManualCashIn,
        closing.ManualCashOut,
        closing.ExpectedCashAmount,
        closing.CountedAmount,
        closing.Difference,
        closing.DifferenceType.ToString(),
        now),
      cancellationToken);

    if (closing.DifferenceType != CashDifferenceType.Balanced)
    {
      await outbox.AddAsync(
        new CashRegisterDifferenceDetectedEventV1(
          Guid.NewGuid(),
          Guid.NewGuid(),
          register.Id,
          rawBusinessId,
          register.BranchId.Value,
          register.UserId,
          closing.Difference,
          closing.DifferenceType.ToString(),
          now),
        cancellationToken);
    }

    await unitOfWork.SaveChangesAsync(cancellationToken);

    LogClosed(logger, register.Id, rawBusinessId, closing.DifferenceType.ToString(), null);

    return Result.Success(new CloseCashRegisterResponse(
      register.Id,
      closing.OpeningAmount,
      closing.CashSales,
      closing.CardSales,
      closing.TransferSales,
      closing.CreditSales,
      closing.CashReturns,
      closing.ManualCashIn,
      closing.ManualCashOut,
      closing.ExpectedCashAmount,
      closing.CountedAmount,
      closing.Difference,
      closing.DifferenceType.ToString(),
      register.ClosedAt!.Value));
  }
}

public sealed record CloseCashRegisterResponse(
  Guid CashRegisterId,
  decimal OpeningAmount,
  decimal CashSales,
  decimal CardSales,
  decimal TransferSales,
  decimal CreditSales,
  decimal CashReturns,
  decimal ManualCashIn,
  decimal ManualCashOut,
  decimal ExpectedCashAmount,
  decimal CountedAmount,
  decimal Difference,
  string DifferenceType,
  DateTimeOffset ClosedAt);
