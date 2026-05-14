namespace SaasCommerce.SharedKernel;

public abstract class AggregateRoot : Entity
{
  private readonly List<IDomainEvent> domainEvents = [];

  public IReadOnlyCollection<IDomainEvent> DomainEvents => domainEvents.AsReadOnly();

  protected void Raise(IDomainEvent domainEvent)
  {
    ArgumentNullException.ThrowIfNull(domainEvent);

    domainEvents.Add(domainEvent);
  }

  public void ClearDomainEvents() => domainEvents.Clear();
}
