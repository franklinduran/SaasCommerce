using SaasCommerce.Modules.Sales.Contracts.Responses;
using SaasCommerce.Modules.Sales.Domain;

namespace SaasCommerce.Modules.Sales.Application.Sales;

internal static class SaleResponseMapper
{
  public static SaleResponse ToResponse(Sale sale)
  {
    ArgumentNullException.ThrowIfNull(sale);

    return new SaleResponse(
      sale.Id,
      sale.BusinessId.Value,
      sale.BranchId.Value,
      sale.UserId,
      sale.CustomerId,
      sale.Status.ToString(),
      sale.PaymentMethod,
      sale.Total,
      sale.Items
        .Select(item => new SaleItemResponse(
          item.ProductId,
          item.Quantity,
          item.UnitPrice,
          item.LineTotal))
        .ToArray(),
      sale.CreatedAt,
      sale.UpdatedAt,
      sale.CompletedAt,
      sale.FailedAt,
      sale.CancelledAt,
      sale.FailureReason,
      sale.CancellationReason);
  }
}
