using SaasCommerce.Modules.Sales.Contracts.Events.V1;
using SaasCommerce.Modules.Sales.Contracts.Responses;
using SaasCommerce.Modules.Sales.Domain;

namespace SaasCommerce.Modules.Sales.Application.Sales;

internal static class SaleReturnMapper
{
  public static IReadOnlyCollection<SaleReturnItemV1> ToEventItems(SaleReturn saleReturn)
  {
    ArgumentNullException.ThrowIfNull(saleReturn);

    return saleReturn.Items
      .Select(item => new SaleReturnItemV1(
        item.SaleItemId,
        item.ProductId,
        item.Quantity,
        item.UnitPrice,
        item.LineTotal))
      .ToArray();
  }

  public static SaleReturnResponse ToResponse(SaleReturn saleReturn, CreditNote? creditNote = null)
  {
    ArgumentNullException.ThrowIfNull(saleReturn);

    return new SaleReturnResponse(
      saleReturn.Id,
      saleReturn.SaleId,
      saleReturn.BusinessId.Value,
      saleReturn.BranchId.Value,
      saleReturn.UserId,
      saleReturn.Status.ToString(),
      saleReturn.Reason,
      saleReturn.Total,
      saleReturn.Items.Select(item => new SaleReturnItemResponse(
        item.Id,
        item.SaleItemId,
        item.ProductId,
        "Producto no disponible",
        null,
        item.Quantity,
        item.UnitPrice,
        item.LineTotal)).ToArray(),
      creditNote is null ? null : ToResponse(creditNote),
      saleReturn.RequestedAt,
      saleReturn.ApprovedAt,
      saleReturn.FailedAt,
      saleReturn.FailureReason);
  }

  private static CreditNoteResponse ToResponse(CreditNote creditNote)
    => new(
      creditNote.Id,
      creditNote.SaleId,
      creditNote.SaleReturnId,
      creditNote.CustomerId,
      creditNote.Code,
      creditNote.Total,
      creditNote.Items.Select(item => new CreditNoteItemResponse(
        item.Id,
        item.ProductId,
        "Producto no disponible",
        null,
        item.Quantity,
        item.UnitPrice,
        item.LineTotal)).ToArray(),
      creditNote.CreatedAt);
}
