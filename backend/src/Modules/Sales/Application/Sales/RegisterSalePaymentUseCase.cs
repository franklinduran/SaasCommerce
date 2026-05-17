using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Payments.Contracts.Events.V1;
using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Sales.Application.Sales;

public sealed class RegisterSalePaymentUseCase(
  IOutboxWriter outbox,
  IClock clock,
  IUnitOfWork unitOfWork) : IRegisterSalePaymentUseCase
{
  public async Task<Result> ExecuteAsync(
    PaymentRegistrationRequestedEventV1 paymentRegistrationRequested,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(paymentRegistrationRequested);

    if (paymentRegistrationRequested.Total <= 0 ||
        string.IsNullOrWhiteSpace(paymentRegistrationRequested.PaymentMethod))
    {
      await outbox.AddAsync(
        new PaymentFailedEventV1(
          Guid.NewGuid(),
          paymentRegistrationRequested.CorrelationId,
          paymentRegistrationRequested.SaleId,
          paymentRegistrationRequested.BusinessId,
          paymentRegistrationRequested.BranchId,
          paymentRegistrationRequested.UserId,
          "Payment data is invalid.",
          clock.UtcNow),
        cancellationToken);
      await unitOfWork.SaveChangesAsync(cancellationToken);
      return Result.Success();
    }

    await outbox.AddAsync(
      new PaymentRegisteredEventV1(
        Guid.NewGuid(),
        paymentRegistrationRequested.CorrelationId,
        paymentRegistrationRequested.SaleId,
        paymentRegistrationRequested.BusinessId,
        paymentRegistrationRequested.BranchId,
        paymentRegistrationRequested.UserId,
        Guid.NewGuid(),
        paymentRegistrationRequested.Total,
        paymentRegistrationRequested.PaymentMethod,
        clock.UtcNow),
      cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success();
  }
}
