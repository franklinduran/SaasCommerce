using System.Reflection;

namespace SaasCommerce.BuildingBlocks;

public static class BuildingBlocksAssemblyReference
{
  public static Assembly Assembly => typeof(BuildingBlocksAssemblyReference).Assembly;
}
