using SaasCommerce.Domain;
using FluentAssertions;

namespace SaasCommerce.Domain.Tests;

public sealed class DomainAssemblyReferenceTests
{
  [Fact]
  public void AssemblyShouldBeDomainAssembly()
  {
    DomainAssemblyReference.Assembly.GetName().Name.Should().Be("SaasCommerce.Domain");
  }
}
