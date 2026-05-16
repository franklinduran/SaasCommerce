using FluentAssertions;
using SaasCommerce.BuildingBlocks;

namespace SaasCommerce.BuildingBlocks.Tests;

public sealed class BuildingBlocksAssemblyReferenceTests
{
  [Fact]
  public void AssemblyShouldBeBuildingBlocksAssembly()
  {
    BuildingBlocksAssemblyReference.Assembly.GetName().Name.Should().Be("SaasCommerce.BuildingBlocks");
  }
}
