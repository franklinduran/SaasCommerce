using SaasCommerce.BuildingBlocks.Application.Abstractions.Storage;

namespace SaasCommerce.Api.Infrastructure.Storage;

public sealed class NullStorageService : IStorageService
{
  public Task<string> UploadAsync(
    string bucketName,
    string objectName,
    Stream data,
    string contentType,
    long size,
    CancellationToken cancellationToken = default)
    => throw new InvalidOperationException(
      "Storage service is not configured. Set Minio:AccessKey and Minio:SecretKey in configuration.");

  public Task DeleteAsync(
    string bucketName,
    string objectName,
    CancellationToken cancellationToken = default)
    => Task.CompletedTask;
}
