using System.Reflection;

namespace SaasCommerce.Worker;

public static class WorkerAssemblyReference
{
  public static Assembly Assembly => typeof(WorkerAssemblyReference).Assembly;
}
