using SaasCommerce.Modules.Customers.Contracts.Responses;
using SaasCommerce.Modules.Customers.Domain;
using SaasCommerce.Modules.Customers.Domain.Credits;

namespace SaasCommerce.Modules.Customers.Application.Credits;

internal static class CustomerCreditResponseMapper
{
  public static CustomerCreditSummaryResponse ToSummary(
    Customer customer,
    CustomerCreditAccount account)
  {
    ArgumentNullException.ThrowIfNull(customer);
    ArgumentNullException.ThrowIfNull(account);

    return new CustomerCreditSummaryResponse(
      customer.BusinessId.Value,
      customer.Id,
      customer.FullName,
      account.CreditLimit,
      account.CurrentBalance,
      account.Status.ToString(),
      account.CreatedAt,
      account.UpdatedAt);
  }

  public static CustomerCreditMovementResponse ToMovement(CustomerCreditMovement movement)
  {
    ArgumentNullException.ThrowIfNull(movement);

    return new CustomerCreditMovementResponse(
      movement.Id,
      movement.BusinessId.Value,
      movement.CustomerId,
      movement.SaleId,
      movement.PaymentId,
      movement.Type.ToString(),
      movement.Amount,
      movement.PreviousBalance,
      movement.NewBalance,
      movement.Note,
      movement.CreatedAt,
      movement.CreatedBy);
  }
}
