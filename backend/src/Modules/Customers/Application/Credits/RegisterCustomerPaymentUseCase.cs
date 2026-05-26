using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Messaging;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Observability;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Persistence;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Billing.Application.Abstractions;
using SaasCommerce.Modules.Billing.Domain;
using SaasCommerce.Modules.Customers.Application.Abstractions;
using SaasCommerce.Modules.Customers.Contracts.Events.V1;
using SaasCommerce.Modules.Customers.Contracts.Responses;
using SaasCommerce.Modules.Customers.Domain.Credits;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Customers.Application.Credits;

public sealed class RegisterCustomerPaymentDependencies
{
  public required ICustomerRepository Customers { get; init; }
  public required ICustomerCreditRepository Credits { get; init; }
  public required ICurrentUserService CurrentUser { get; init; }
  public required IOutboxWriter Outbox { get; init; }
  public required ICorrelationIdProvider CorrelationIdProvider { get; init; }
  public required IClock Clock { get; init; }
  public required IUnitOfWork UnitOfWork { get; init; }
  public required ISubscriptionAccessPolicy SubscriptionAccess { get; init; }
}

public sealed class RegisterCustomerPaymentUseCase(RegisterCustomerPaymentDependencies dependencies) : IRegisterCustomerPaymentUseCase
{
  private readonly ICustomerRepository customers = dependencies.Customers;
  private readonly ICustomerCreditRepository credits = dependencies.Credits;
  private readonly ICurrentUserService currentUser = dependencies.CurrentUser;
  private readonly IOutboxWriter outbox = dependencies.Outbox;
  private readonly ICorrelationIdProvider correlationIdProvider = dependencies.CorrelationIdProvider;
  private readonly IClock clock = dependencies.Clock;
  private readonly IUnitOfWork unitOfWork = dependencies.UnitOfWork;
  private readonly ISubscriptionAccessPolicy subscriptionAccess = dependencies.SubscriptionAccess;

  public async Task<Result<RegisterCustomerPaymentResponse>> ExecuteAsync(
    RegisterCustomerPaymentCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(command);

    var context = ResolveContext();

    if (context.IsFailure)
    {
      return Result.Failure<RegisterCustomerPaymentResponse>(context.Error);
    }

    if (command.Amount <= 0)
    {
      return Result.Failure<RegisterCustomerPaymentResponse>(CustomerCreditErrors.InvalidCreditOperation);
    }

    var tenant = new BusinessId(context.Value.BusinessId);
    var access = await subscriptionAccess.EnsureCanUseFeatureAsync(
      tenant,
      SubscriptionFeatures.Payments,
      cancellationToken);
    if (access.IsFailure)
    {
      return Result.Failure<RegisterCustomerPaymentResponse>(access.Error);
    }

    var customer = await customers.GetAsync(tenant, command.CustomerId, cancellationToken);

    if (customer is null)
    {
      return Result.Failure<RegisterCustomerPaymentResponse>(CustomerCreditErrors.CustomerNotFound);
    }

    var account = await credits.GetAccountAsync(tenant, command.CustomerId, cancellationToken);

    if (account is null)
    {
      account = new CustomerCreditAccount(Guid.NewGuid(), tenant, command.CustomerId, 0, clock.UtcNow);
      await credits.AddAccountAsync(account, cancellationToken);
    }

    var paymentId = Guid.NewGuid();
    CustomerPayment payment;
    CustomerCreditMovement movement;

    try
    {
      payment = new CustomerPayment(
        paymentId,
        tenant,
        command.CustomerId,
        command.Amount,
        command.Note,
        clock.UtcNow,
        context.Value.UserId);
      movement = account.ApplyPayment(
        Guid.NewGuid(),
        paymentId,
        command.Amount,
        command.Note,
        context.Value.UserId,
        clock.UtcNow);
    }
    catch (InvalidOperationException)
    {
      return Result.Failure<RegisterCustomerPaymentResponse>(CustomerCreditErrors.PaymentExceedsBalance);
    }
    catch (ArgumentException)
    {
      return Result.Failure<RegisterCustomerPaymentResponse>(CustomerCreditErrors.InvalidCreditOperation);
    }

    await credits.AddPaymentAsync(payment, cancellationToken);
    await credits.AddMovementAsync(movement, cancellationToken);
    await outbox.AddAsync(
      new CustomerPaymentRegisteredEventV1(
        Guid.NewGuid(),
        ResolveCorrelationId(),
        context.Value.BusinessId,
        command.CustomerId,
        paymentId,
        command.Amount,
        account.CurrentBalance,
        clock.UtcNow),
      cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success(new RegisterCustomerPaymentResponse(
      command.CustomerId,
      paymentId,
      command.Amount,
      account.CurrentBalance));
  }

  private Result<CreditUserContext> ResolveContext()
    => currentUser.BusinessId is Guid businessId && currentUser.UserId is Guid userId
      ? Result.Success(new CreditUserContext(businessId, userId))
      : Result.Failure<CreditUserContext>(CustomerCreditErrors.UserContextRequired);

  private Guid ResolveCorrelationId()
    => Guid.TryParse(correlationIdProvider.CorrelationId, out var correlationId)
      ? correlationId
      : Guid.NewGuid();

  private sealed record CreditUserContext(Guid BusinessId, Guid UserId);
}
