using FluentAssertions;
using SaasCommerce.SharedKernel;

namespace SaasCommerce.SharedKernel.Tests;

public sealed class ResultTests
{
  [Fact]
  public void SuccessShouldNotContainError()
  {
    var result = Result.Success();

    result.IsSuccess.Should().BeTrue();
    result.Error.Should().Be(DomainError.None);
  }

  [Fact]
  public void FailureShouldContainError()
  {
    var error = new DomainError("test_error", "Something failed.");

    var result = Result.Failure(error);

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(error);
  }
}
