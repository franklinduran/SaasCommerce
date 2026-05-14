namespace SaasCommerce.SharedKernel;

public abstract class Entity
{
  public Guid Id { get; protected init; }
}
