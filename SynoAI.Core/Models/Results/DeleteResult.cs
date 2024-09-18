namespace SynoAI.Core.Models.Results;

public class DeleteResult
{
    private DeleteResult(bool isSuccess, string? error)
    {
        if (!isSuccess && string.IsNullOrWhiteSpace(error))
            throw new ArgumentNullException(nameof(error), "Error cannot be null or empty for a failed result.");

        IsSuccess = isSuccess;
        Error = error;
    }


    public bool IsSuccess { get; }
    public string? Error { get; }

    public static DeleteResult Success() => new(true, null);
    public static DeleteResult Failure(string error) => new(false, error);
}
