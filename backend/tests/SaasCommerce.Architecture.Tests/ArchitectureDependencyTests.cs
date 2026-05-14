using System.Reflection;
using SaasCommerce.Application;
using SaasCommerce.Contracts;
using SaasCommerce.Domain;
using SaasCommerce.Infrastructure;
using FluentAssertions;
using NetArchTest.Rules;

namespace SaasCommerce.Architecture.Tests;

public sealed class ArchitectureDependencyTests
{
  private static readonly Assembly DomainAssembly = DomainAssemblyReference.Assembly;
  private static readonly Assembly ApplicationAssembly = ApplicationAssemblyReference.Assembly;
  private static readonly Assembly ContractsAssembly = ContractsAssemblyReference.Assembly;
  private static readonly Assembly InfrastructureAssembly = InfrastructureAssemblyReference.Assembly;

  [Theory]
  [InlineData("SaasCommerce.Infrastructure")]
  [InlineData("SaasCommerce.Application")]
  [InlineData("MassTransit")]
  [InlineData("Microsoft.EntityFrameworkCore")]
  public void DomainShouldNotDependOnForbiddenNamespaces(string dependency)
  {
    AssertNoDependency(DomainAssembly, dependency);
  }

  [Theory]
  [InlineData("SaasCommerce.Infrastructure")]
  [InlineData("Microsoft.AspNetCore")]
  [InlineData("Microsoft.AspNetCore.SignalR")]
  public void ApplicationShouldNotDependOnForbiddenNamespaces(string dependency)
  {
    AssertNoDependency(ApplicationAssembly, dependency);
  }

  [Fact]
  public void ContractsShouldNotDependOnInfrastructure()
  {
    AssertNoDependency(ContractsAssembly, "SaasCommerce.Infrastructure");
  }

  [Theory]
  [InlineData("SaasCommerce.Api")]
  [InlineData("SaasCommerce.Worker")]
  public void CoreLayersShouldNotDependOnEntrypoints(string dependency)
  {
    AssertNoDependency(DomainAssembly, dependency);
    AssertNoDependency(ApplicationAssembly, dependency);
    AssertNoDependency(InfrastructureAssembly, dependency);
  }

  private static void AssertNoDependency(Assembly assembly, string dependency)
  {
    var result = Types
      .InAssembly(assembly)
      .Should()
      .NotHaveDependencyOn(dependency)
      .GetResult();

    result.IsSuccessful.Should().BeTrue(
      $"{assembly.GetName().Name} should not depend on {dependency}");
  }
}
