namespace SaasCommerce.BuildingBlocks.Application.Abstractions.Storage;

public interface IStorageService
{
  Task<string> UploadAsync(
    string bucketName,
    string objectName,
    Stream data,
    string contentType,
    long size,
    CancellationToken cancellationToken = default);

  Task DeleteAsync(
    string bucketName,
    string objectName,
    CancellationToken cancellationToken = default);
}
