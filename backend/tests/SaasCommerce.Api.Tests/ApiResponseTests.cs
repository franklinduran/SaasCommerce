using SaasCommerce.BuildingBlocks.Contracts.Common;
using FluentAssertions;

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
}
