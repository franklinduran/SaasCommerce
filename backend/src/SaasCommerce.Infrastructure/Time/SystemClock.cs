using SaasCommerce.Application.Abstractions.Time;

namespace SaasCommerce.Infrastructure.Time;

public sealed class SystemClock : IClock
{
  public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
