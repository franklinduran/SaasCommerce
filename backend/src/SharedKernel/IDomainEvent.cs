namespace SaasCommerce.SharedKernel;

public interface IDomainEvent
{
  Guid EventId { get; }

  DateTimeOffset OccurredAt { get; }
}
