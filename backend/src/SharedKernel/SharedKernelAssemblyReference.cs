using System.Reflection;

namespace SaasCommerce.SharedKernel;

public static class SharedKernelAssemblyReference
{
  public static Assembly Assembly => typeof(SharedKernelAssemblyReference).Assembly;
}
