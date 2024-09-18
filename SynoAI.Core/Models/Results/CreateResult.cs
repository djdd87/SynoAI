namespace SynoAI.Core.Models.Results;

public class CreateResult<T>
{
    private CreateResult(bool isSuccess, T? result, string? error)
    {
        if (isSuccess && result == null)
            throw new ArgumentNullException(nameof(result), "Cannot be null for a successful result.");
        if (!isSuccess && string.IsNullOrWhiteSpace(error))
            throw new ArgumentNullException(nameof(error), "Error cannot be null or empty for a failed result.");

        IsSuccess = isSuccess;
        Result = result;
        Error = error;
    }


    public bool IsSuccess { get; }
    public T? Result { get; }
    public string? Error { get; }

    public static CreateResult<T> Success(T result) => new(true, result, null);
    public static CreateResult<T> Failure(string error) => new(false, default(T?), error);
}