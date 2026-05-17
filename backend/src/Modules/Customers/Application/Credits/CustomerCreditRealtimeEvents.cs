namespace SaasCommerce.Modules.Customers.Application.Credits;

public static class CustomerCreditRealtimeEvents
{
  public const string CreditDebited = "customer.creditDebited";
  public const string PaymentRegistered = "customer.paymentRegistered";
  public const string CreditBlocked = "customer.creditBlocked";
  public const string CreditUnblocked = "customer.creditUnblocked";
}

public sealed record CustomerCreditDebitedNotificationV1(
  Guid EventId,
  Guid CorrelationId,
  Guid BusinessId,
  Guid CustomerId,
  Guid SaleId,
  decimal Amount,
  decimal NewBalance,
  DateTimeOffset CreatedAt,
  int Version = 1);

public sealed record CustomerPaymentRegisteredNotificationV1(
  Guid EventId,
  Guid CorrelationId,
  Guid BusinessId,
  Guid CustomerId,
  Guid PaymentId,
  decimal Amount,
  decimal NewBalance,
  DateTimeOffset CreatedAt,
  int Version = 1);

public sealed record CustomerCreditStatusChangedNotificationV1(
  Guid EventId,
  Guid BusinessId,
  Guid CustomerId,
  string Status,
  DateTimeOffset CreatedAt,
  int Version = 1);
