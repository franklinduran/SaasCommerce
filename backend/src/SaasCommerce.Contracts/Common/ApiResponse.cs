namespace SaasCommerce.Contracts.Common;

public sealed record ApiResponse<T>(
  bool Succeeded,
  T? Data,
  IReadOnlyCollection<ApiError> Errors,
  string? CorrelationId);

public static class ApiResponse
{
  public static ApiResponse<T> Success<T>(T data, string? correlationId = null)
    => new(true, data, Array.Empty<ApiError>(), correlationId);

  public static ApiResponse<T> Failure<T>(ApiError error, string? correlationId = null)
    => new(false, default, new[] { error }, correlationId);

  public static ApiResponse<T> Failure<T>(
    IReadOnlyCollection<ApiError> errors,
    string? correlationId = null)
    => new(false, default, errors, correlationId);
}
