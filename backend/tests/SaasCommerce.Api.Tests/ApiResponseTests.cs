using System.Text.Json;
using FluentAssertions;
using SaasCommerce.BuildingBlocks.Contracts.Common;

namespace SaasCommerce.Api.Tests;

public sealed class ApiResponseTests
{
  [Fact]
  public void SuccessShouldCreateStandardResponse()
  {
    var response = ApiResponse.Success("ok", "correlation-id");

    response.IsSuccess.Should().BeTrue();
    response.Data.Should().Be("ok");
    response.Error.Should().BeNull();
    response.CorrelationId.Should().Be("correlation-id");
  }

  [Fact]
  public void FailureShouldCreateStandardResponse()
  {
    var error = new ApiError("validation_error", "Invalid request.");

    var response = ApiResponse.Failure<object>(error, "correlation-id");

    response.IsSuccess.Should().BeFalse();
    response.Data.Should().BeNull();
    response.Error.Should().Be(error);
    response.CorrelationId.Should().Be("correlation-id");
  }

  [Fact]
  public void JsonShouldUseStandardResultContract()
  {
    var response = ApiResponse.Success("ok", "correlation-id");

    var json = JsonSerializer.Serialize(response, JsonSerializerOptions.Web);

    json.Should().Contain("\"isSuccess\":true");
    json.Should().Contain("\"data\":\"ok\"");
    json.Should().Contain("\"error\":null");
    json.Should().Contain("\"correlationId\":\"correlation-id\"");
    json.Should().NotContain("succeeded");
    json.Should().NotContain("\"errors\"");
  }

  [Fact]
  public void FrontendShouldUseStandardResultContract()
  {
    var repositoryRoot = FindRepositoryRoot();
    var apiTypes = File.ReadAllText(
      Path.Combine(repositoryRoot, "frontend", "src", "shared", "types", "api.ts"));
    var httpClient = File.ReadAllText(
      Path.Combine(repositoryRoot, "frontend", "src", "shared", "services", "httpClient.ts"));

    apiTypes.Should().Contain("isSuccess: boolean");
    apiTypes.Should().Contain("data: T | null");
    apiTypes.Should().Contain("error: ApiError | null");
    apiTypes.Should().NotContain("succeeded: boolean");
    apiTypes.Should().NotContain("errors: ApiError[]");
    httpClient.Should().Contain("payload.isSuccess");
    httpClient.Should().Contain("payload.error");
    httpClient.Should().NotContain("payload.succeeded");
    httpClient.Should().NotContain("payload.errors");
  }

  private static string FindRepositoryRoot()
  {
    var directory = new DirectoryInfo(AppContext.BaseDirectory);

    while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "SaasCommerce.slnx")))
    {
      directory = directory.Parent;
    }

    directory.Should().NotBeNull();

    return directory!.FullName;
  }
}
