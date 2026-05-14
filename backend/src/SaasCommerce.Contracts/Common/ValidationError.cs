namespace SaasCommerce.Contracts.Common;

public sealed record ValidationError(string Field, string Message);
