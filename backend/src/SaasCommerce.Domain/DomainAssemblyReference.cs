using System.Reflection;

namespace SaasCommerce.Domain;

public static class DomainAssemblyReference
{
  public static Assembly Assembly => typeof(DomainAssemblyReference).Assembly;
}
