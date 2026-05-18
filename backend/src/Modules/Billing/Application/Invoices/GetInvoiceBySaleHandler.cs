using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Billing.Application.Abstractions;
using SaasCommerce.Modules.Billing.Contracts.Responses;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Billing.Application.Invoices;

public sealed class GetInvoiceBySaleHandler(
  IInvoiceRepository invoices,
  ICurrentUserService currentUser)
{
  public async Task<Result<InvoiceResponse>> Handle(
    GetInvoiceBySaleQuery query,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(query);

    if (query.SaleId == Guid.Empty)
    {
      return Result.Failure<InvoiceResponse>(InvoiceErrors.InvalidInvoice);
    }

    if (!currentUser.IsAuthenticated || currentUser.BusinessId is not { } businessId)
    {
      return Result.Failure<InvoiceResponse>(InvoiceErrors.UserContextRequired);
    }

    var invoice = await invoices.GetBySaleAsync(
      new BusinessId(businessId),
      query.SaleId,
      cancellationToken);

    return invoice is null
      ? Result.Failure<InvoiceResponse>(InvoiceErrors.InvoiceNotFound)
      : Result.Success(InvoiceResponseMapper.ToResponse(invoice));
  }
}
