using SaasCommerce.Modules.Identity.Application.Abstractions;
using SaasCommerce.SharedKernel;

namespace SaasCommerce.Modules.Identity.Application.PilotBusiness;

public sealed record GetPilotMetricsQuery;

public sealed class GetPilotMetricsHandler(IPilotMetricsRepository metricsRepository)
{
  public async Task<Result<PilotMetricsSummary>> Handle(
    GetPilotMetricsQuery query,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(query);

    var summary = await metricsRepository.GetSummaryAsync(cancellationToken);
    return Result.Success(summary);
  }
}
