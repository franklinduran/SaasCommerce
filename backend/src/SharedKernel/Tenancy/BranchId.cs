namespace SaasCommerce.SharedKernel.Tenancy;

public readonly record struct BranchId(Guid Value)
{
  public static BranchId Empty => new(Guid.Empty);

  public static BranchId New() => new(Guid.NewGuid());

  public override string ToString() => Value.ToString("D");
}
