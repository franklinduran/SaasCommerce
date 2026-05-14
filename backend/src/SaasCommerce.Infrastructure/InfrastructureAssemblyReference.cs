using System.Reflection;

namespace SaasCommerce.Infrastructure;

public static class InfrastructureAssemblyReference
{
  public static Assembly Assembly => typeof(InfrastructureAssemblyReference).Assembly;
}
