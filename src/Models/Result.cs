namespace AIWA.API.Models;

public record Result<T>(bool IsSuccess, T? Value, string? Error = null)
{
    public static Result<T> Success(T value) => new(true, value, null);

    public static Result<T> Failure(string error) => new(false, default, error);

    public void Match(Action<T> successAction, Action<string> failureAction)
    {
        if (IsSuccess)
        {
            successAction(Value!);
        }
        else
        {
            failureAction(Error!);
        }
    }
}
