using System.Reflection;

namespace SaasCommerce.Api;

public static class ApiAssemblyReference
{
  public static Assembly Assembly => typeof(ApiAssemblyReference).Assembly;
}
