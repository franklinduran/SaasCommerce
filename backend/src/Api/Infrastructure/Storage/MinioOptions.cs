namespace SaasCommerce.Api.Infrastructure.Storage;

public sealed class MinioOptions
{
  public string Endpoint { get; init; } = "localhost:9000";
  public string AccessKey { get; init; } = string.Empty;
  public string SecretKey { get; init; } = string.Empty;
  public string BucketName { get; init; } = "products";
  public string PublicUrl { get; init; } = "http://localhost:9000";
  public bool UseSsl { get; init; }
}
