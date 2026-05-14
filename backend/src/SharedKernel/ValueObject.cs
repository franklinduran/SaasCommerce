namespace SaasCommerce.SharedKernel;

public abstract class ValueObject
{
  protected abstract IEnumerable<object?> GetEqualityComponents();

  public override bool Equals(object? obj)
    => obj is ValueObject other &&
       GetType() == other.GetType() &&
       GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());

  public override int GetHashCode()
    => GetEqualityComponents()
      .Aggregate(1, (current, component) =>
        HashCode.Combine(current, component));
}
