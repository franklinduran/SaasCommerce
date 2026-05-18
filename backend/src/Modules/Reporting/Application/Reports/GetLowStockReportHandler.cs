using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.Modules.Reporting.Application.Abstractions;
using SaasCommerce.Modules.Reporting.Contracts.Responses;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Reporting.Application.Reports;

public sealed class GetLowStockReportHandler(
  IReportsReadRepository reports,
  ICurrentUserService currentUser)
{
  private const int MaxPageSize = 100;

  public async Task<Result<LowStockReportResponse>> Handle(
    GetLowStockReportQuery query,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(query);

    if (!currentUser.IsAuthenticated || currentUser.BusinessId is not { } businessId)
    {
      return Result.Failure<LowStockReportResponse>(ReportingErrors.UserContextRequired);
    }

    var tenantId = new BusinessId(businessId);
    var page = Math.Max(1, query.Page);
    var pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);

    var criteria = new LowStockCriteria(
      query.BranchId,
      query.CategoryId,
      query.Search,
      page,
      pageSize);

    var result = await reports.GetLowStockReportAsync(tenantId, criteria, cancellationToken);

    return Result.Success(result);
  }
}
