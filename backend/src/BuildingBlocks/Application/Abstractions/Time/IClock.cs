namespace SaasCommerce.BuildingBlocks.Application.Abstractions.Time;

public interface IClock
{
  DateTimeOffset UtcNow { get; }
}
