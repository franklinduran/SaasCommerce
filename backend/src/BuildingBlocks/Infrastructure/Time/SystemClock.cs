using SaasCommerce.BuildingBlocks.Application.Abstractions.Time;

namespace SaasCommerce.BuildingBlocks.Infrastructure.Time;

public sealed class SystemClock : IClock
{
  public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
