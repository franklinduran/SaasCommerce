namespace SaasCommerce.SharedKernel;

public sealed record DomainError(string Code, string Message)
{
  public static readonly DomainError None = new(string.Empty, string.Empty);
}
