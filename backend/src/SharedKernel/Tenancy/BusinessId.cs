namespace SaasCommerce.SharedKernel.Tenancy;

public readonly record struct BusinessId(Guid Value)
{
  public static BusinessId Empty => new(Guid.Empty);

  public static BusinessId New() => new(Guid.NewGuid());

  public override string ToString() => Value.ToString("D");
}
