using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Billing.Application.Abstractions;
using SaasCommerce.Modules.Billing.Contracts.Responses;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Billing.Application.Invoices;

public sealed class GetInvoiceByIdHandler(
  IInvoiceRepository invoices,
  ICurrentUserService currentUser)
{
  public async Task<Result<InvoiceResponse>> Handle(
    GetInvoiceByIdQuery query,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(query);

    if (query.InvoiceId == Guid.Empty)
    {
      return Result.Failure<InvoiceResponse>(InvoiceErrors.InvalidInvoice);
    }

    if (!currentUser.IsAuthenticated || currentUser.BusinessId is not { } businessId)
    {
      return Result.Failure<InvoiceResponse>(InvoiceErrors.UserContextRequired);
    }

    var invoice = await invoices.GetAsync(
      new BusinessId(businessId),
      query.InvoiceId,
      cancellationToken);

    return invoice is null
      ? Result.Failure<InvoiceResponse>(InvoiceErrors.InvoiceNotFound)
      : Result.Success(InvoiceResponseMapper.ToResponse(invoice));
  }
}
