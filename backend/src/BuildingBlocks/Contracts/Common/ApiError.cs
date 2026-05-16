namespace SaasCommerce.BuildingBlocks.Contracts.Common;

public sealed record ApiError(
  string Code,
  string Message,
  string? Target = null,
  IReadOnlyCollection<ValidationError>? ValidationErrors = null)
{
  public static ApiError Validation(IReadOnlyCollection<ApiError> errors)
  {
    ArgumentNullException.ThrowIfNull(errors);

    return new ApiError(
      "VALIDATION_ERROR",
      "One or more validation errors occurred.",
      ValidationErrors: errors
        .Select(error => new ValidationError(error.Target ?? error.Code, error.Message))
        .ToArray());
  }
}
