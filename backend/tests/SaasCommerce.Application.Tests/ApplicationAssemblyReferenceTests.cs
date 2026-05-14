using SaasCommerce.Application;
using FluentAssertions;

namespace SaasCommerce.Application.Tests;

public sealed class ApplicationAssemblyReferenceTests
{
  [Fact]
  public void AssemblyShouldBeApplicationAssembly()
  {
    ApplicationAssemblyReference.Assembly.GetName().Name.Should().Be("SaasCommerce.Application");
  }
}
