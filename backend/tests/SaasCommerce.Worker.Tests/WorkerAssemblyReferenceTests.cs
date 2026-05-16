using FluentAssertions;
using SaasCommerce.Worker;

namespace SaasCommerce.Worker.Tests;

public sealed class WorkerAssemblyReferenceTests
{
  [Fact]
  public void AssemblyShouldBeWorkerAssembly()
  {
    WorkerAssemblyReference.Assembly.GetName().Name.Should().Be("SaasCommerce.Worker");
  }
}
