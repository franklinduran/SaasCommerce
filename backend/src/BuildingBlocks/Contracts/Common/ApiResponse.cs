namespace SaasCommerce.BuildingBlocks.Contracts.Common;

public sealed record ApiResponse<T>(
  bool IsSuccess,
  T? Data,
  ApiError? Error,
  string? CorrelationId);

public static class ApiResponse
{
  public static ApiResponse<T> Success<T>(T data, string? correlationId = null)
    => new(true, data, null, correlationId);

  public static ApiResponse<T> Failure<T>(ApiError error, string? correlationId = null)
    => new(false, default, error, correlationId);

  public static ApiResponse<T> Failure<T>(
    IReadOnlyCollection<ApiError> errors,
    string? correlationId = null)
    => new(false, default, ApiError.Validation(errors), correlationId);
}
