using SaasCommerce.BuildingBlocks.Application.Abstractions.Auth;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;
using SaasCommerce.Modules.Reporting.Application.Abstractions;
using SaasCommerce.Modules.Reporting.Contracts.Responses;
using SaasCommerce.SharedKernel;
using SaasCommerce.SharedKernel.Tenancy;

namespace SaasCommerce.Modules.Reporting.Application.Dashboard;

public sealed class GetDashboardSummaryHandler(
  IReportsReadRepository reports,
  ICurrentUserService currentUser,
  IClock clock)
{
  public async Task<Result<DashboardSummaryResponse>> Handle(
    GetDashboardSummaryQuery query,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(query);

    if (!currentUser.IsAuthenticated || currentUser.BusinessId is not { } businessId)
    {
      return Result.Failure<DashboardSummaryResponse>(ReportingErrors.UserContextRequired);
    }

    var tenantId = new BusinessId(businessId);
    var today = clock.UtcNow.Date;
    var todayStart = new DateTimeOffset(today, TimeSpan.Zero);
    var thirtyDaysAgo = clock.UtcNow.AddDays(-30);

    var summary = await reports.GetDashboardSummaryAsync(tenantId, todayStart, thirtyDaysAgo, cancellationToken);

    return Result.Success(summary);
  }
}
