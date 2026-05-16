namespace SaasCommerce.SharedKernel;

public sealed record DomainError(
  string Code,
  string Message,
  IReadOnlyCollection<DomainError>? Details = null)
{
  public static readonly DomainError None = new(string.Empty, string.Empty);
}
