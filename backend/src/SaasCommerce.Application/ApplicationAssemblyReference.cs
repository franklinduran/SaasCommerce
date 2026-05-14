using System.Reflection;

namespace SaasCommerce.Application;

public static class ApplicationAssemblyReference
{
  public static Assembly Assembly => typeof(ApplicationAssemblyReference).Assembly;
}
