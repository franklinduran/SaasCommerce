namespace SaasCommerce.SharedKernel;

public class Result
{
  protected Result(bool isSuccess, DomainError error)
  {
    if (isSuccess && error != DomainError.None)
    {
      throw new ArgumentException("A successful result cannot contain an error.", nameof(error));
    }

    if (!isSuccess && error == DomainError.None)
    {
      throw new ArgumentException("A failed result must contain an error.", nameof(error));
    }

    IsSuccess = isSuccess;
    Error = error;
  }

  public bool IsSuccess { get; }

  public bool IsFailure => !IsSuccess;

  public DomainError Error { get; }

  public static Result Success() => new(true, DomainError.None);

  public static Result<T> Success<T>(T value) => new(value);

  public static Result Failure(DomainError error) => new(false, error);

  public static Result<T> Failure<T>(DomainError error) => new(error);
}

public sealed class Result<T> : Result
{
  private readonly T? value;

  internal Result(T value)
    : base(true, DomainError.None)
    => this.value = value;

  internal Result(DomainError error)
    : base(false, error)
  {
  }

  public T Value => IsSuccess
    ? value!
    : throw new InvalidOperationException("The value of a failed result cannot be accessed.");
}
