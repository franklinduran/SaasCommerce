using Minio;
using Minio.DataModel.Args;
using SaasCommerce.BuildingBlocks.Application.Abstractions.Storage;

namespace SaasCommerce.Api.Infrastructure.Storage;

public sealed class MinioStorageService(IMinioClient minio, MinioOptions options) : IStorageService
{
  public async Task<string> UploadAsync(
    string bucketName,
    string objectName,
    Stream data,
    string contentType,
    long size,
    CancellationToken cancellationToken = default)
  {
    await EnsureBucketExistsAsync(bucketName, cancellationToken);

    var putArgs = new PutObjectArgs()
      .WithBucket(bucketName)
      .WithObject(objectName)
      .WithStreamData(data)
      .WithObjectSize(size)
      .WithContentType(contentType);

    await minio.PutObjectAsync(putArgs, cancellationToken);

    return $"{options.PublicUrl.TrimEnd('/')}/{bucketName}/{objectName}";
  }

  public async Task DeleteAsync(
    string bucketName,
    string objectName,
    CancellationToken cancellationToken = default)
  {
    var removeArgs = new RemoveObjectArgs()
      .WithBucket(bucketName)
      .WithObject(objectName);

    await minio.RemoveObjectAsync(removeArgs, cancellationToken);
  }

  private async Task EnsureBucketExistsAsync(string bucketName, CancellationToken cancellationToken)
  {
    var existsArgs = new BucketExistsArgs().WithBucket(bucketName);
    var exists = await minio.BucketExistsAsync(existsArgs, cancellationToken);

    if (!exists)
    {
      var makeArgs = new MakeBucketArgs().WithBucket(bucketName);
      await minio.MakeBucketAsync(makeArgs, cancellationToken);

      var policyJson = $$"""
        {
          "Version": "2012-10-17",
          "Statement": [
            {
              "Effect": "Allow",
              "Principal": {"AWS": ["*"]},
              "Action": ["s3:GetObject"],
              "Resource": ["arn:aws:s3:::{{bucketName}}/*"]
            }
          ]
        }
        """;

      var policyArgs = new SetPolicyArgs()
        .WithBucket(bucketName)
        .WithPolicy(policyJson);

      await minio.SetPolicyAsync(policyArgs, cancellationToken);
    }
  }
}
