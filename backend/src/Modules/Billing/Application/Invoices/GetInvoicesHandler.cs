using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Billing.Application.Abstractions;
using SaasCommerce.Modules.Billing.Contracts.Responses;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Billing.Application.Invoices;

public sealed class GetInvoicesHandler(
  IInvoiceRepository invoices,
  ICurrentUserService currentUser)
{
  private const int MaxPageSize = 50;

  public async Task<Result<InvoiceListResponse>> Handle(
    GetInvoicesQuery query,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(query);

    if (!currentUser.IsAuthenticated || currentUser.BusinessId is not { } businessId)
    {
      return Result.Failure<InvoiceListResponse>(InvoiceErrors.UserContextRequired);
    }

    var page = Math.Max(1, query.Page);
    var pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);
    var criteria = new InvoiceSearchCriteria(
      query.Status,
      query.Query,
      query.DateFrom,
      query.DateTo,
      page,
      pageSize);
    var tenantId = new BusinessId(businessId);
    var totalItems = await invoices.CountAsync(tenantId, criteria, cancellationToken);
    var items = await invoices.ListAsync(tenantId, criteria, cancellationToken);
    var totalPages = totalItems == 0
      ? 0
      : (int)Math.Ceiling(totalItems / (double)pageSize);

    return Result.Success(new InvoiceListResponse(
      items.Select(InvoiceResponseMapper.ToResponse).ToArray(),
      page,
      pageSize,
      totalItems,
      totalPages,
      page > 1,
      totalPages > page));
  }
}
