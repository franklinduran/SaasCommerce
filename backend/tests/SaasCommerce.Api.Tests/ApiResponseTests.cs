using SaasCommerce.Contracts.Common;
using FluentAssertions;

namespace SaasCommerce.Api.Tests;

public sealed class ApiResponseTests
{
  [Fact]
  public void SuccessShouldCreateStandardResponse()
  {
    var response = ApiResponse.Success("ok", "correlation-id");

    response.Succeeded.Should().BeTrue();
    response.Data.Should().Be("ok");
    response.CorrelationId.Should().Be("correlation-id");
    response.Errors.Should().BeEmpty();
  }
}
