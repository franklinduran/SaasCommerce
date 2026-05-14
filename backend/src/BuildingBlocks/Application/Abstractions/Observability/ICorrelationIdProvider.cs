namespace SaasCommerce.BuildingBlocks.Application.Abstractions.Observability;

public interface ICorrelationIdProvider
{
  string CorrelationId { get; }
}
