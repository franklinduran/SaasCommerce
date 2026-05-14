namespace SaasCommerce.BuildingBlocks.Contracts.Common;

public sealed record ValidationError(string Field, string Message);
