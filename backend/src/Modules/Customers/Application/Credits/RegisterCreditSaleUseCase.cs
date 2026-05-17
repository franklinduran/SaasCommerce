using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Customers.Application.Abstractions;
using SaasCommerce.Modules.Customers.Contracts.Events.V1;
using SaasCommerce.Modules.Customers.Domain.Credits;
using SaasCommerce.Modules.Sales.Application.Abstractions;
using SaasCommerce.Modules.Sales.Contracts.Events.V1;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Customers.Application.Credits;

public sealed class RegisterCreditSaleUseCase(
  ISaleRepository sales,
  ICustomerCreditRepository credits,
  IOutboxWriter outbox,
  IClock clock,
  IUnitOfWork unitOfWork) : IRegisterCreditSaleUseCase
{
  private const string CreditPaymentMethod = "Credit";

  public async Task<Result> ExecuteAsync(
    SaleCompletedEventV1 saleCompleted,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(saleCompleted);

    var tenant = new BusinessId(saleCompleted.BusinessId);
    var sale = await sales.GetAsync(tenant, saleCompleted.SaleId, cancellationToken);

    if (sale is null)
    {
      return Result.Failure(CustomerCreditErrors.SaleNotFound);
    }

    if (!IsCreditSale(sale.PaymentMethod))
    {
      return Result.Success();
    }

    if (sale.CustomerId is not Guid customerId)
    {
      return Result.Failure(CustomerCreditErrors.InvalidCreditOperation);
    }

    if (await credits.HasDebitForSaleAsync(tenant, sale.Id, cancellationToken))
    {
      return Result.Success();
    }

    var account = await credits.GetAccountAsync(tenant, customerId, cancellationToken);

    if (account is null)
    {
      account = new CustomerCreditAccount(Guid.NewGuid(), tenant, customerId, 0, clock.UtcNow);
      await credits.AddAccountAsync(account, cancellationToken);
    }

    CustomerCreditMovement movement;

    try
    {
      movement = account.ApplyDebit(
        Guid.NewGuid(),
        sale.Id,
        sale.Total,
        $"Venta fiada {sale.Id:D}",
        saleCompleted.UserId,
        clock.UtcNow);
    }
    catch (InvalidOperationException) when (account.Status is CustomerCreditStatus.Blocked or CustomerCreditStatus.Closed)
    {
      return Result.Failure(CustomerCreditErrors.CreditAccountBlocked);
    }
    catch (InvalidOperationException)
    {
      await PublishLimitExceededAsync(saleCompleted, customerId, account, cancellationToken);
      await unitOfWork.SaveChangesAsync(cancellationToken);
      return Result.Failure(CustomerCreditErrors.CreditLimitExceeded);
    }
    catch (ArgumentException)
    {
      return Result.Failure(CustomerCreditErrors.InvalidCreditOperation);
    }

    await credits.AddMovementAsync(movement, cancellationToken);
    await outbox.AddAsync(
      new CustomerCreditDebitedEventV1(
        Guid.NewGuid(),
        saleCompleted.CorrelationId,
        saleCompleted.BusinessId,
        customerId,
        sale.Id,
        sale.Total,
        account.CurrentBalance,
        clock.UtcNow),
      cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success();
  }

  private Task PublishLimitExceededAsync(
    SaleCompletedEventV1 saleCompleted,
    Guid customerId,
    CustomerCreditAccount account,
    CancellationToken cancellationToken)
    => outbox.AddAsync(
      new CustomerCreditLimitExceededEventV1(
        Guid.NewGuid(),
        saleCompleted.CorrelationId,
        saleCompleted.BusinessId,
        customerId,
        saleCompleted.SaleId,
        saleCompleted.Total,
        account.CreditLimit,
        account.CurrentBalance,
        clock.UtcNow),
      cancellationToken);

  public static bool IsCreditSale(string paymentMethod)
    => string.Equals(paymentMethod, CreditPaymentMethod, StringComparison.OrdinalIgnoreCase);
}
