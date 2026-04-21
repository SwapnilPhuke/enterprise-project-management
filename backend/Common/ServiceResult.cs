namespace ProjectManagement.Common;

/// <summary>
/// Typed result returned by service methods.
/// Controllers map this to an HTTP response — no try/catch needed in controllers.
/// </summary>
public class ServiceResult
{
    public bool    Success      { get; protected init; }
    public string? ErrorMessage { get; protected init; }
    public int     StatusCode   { get; protected init; }

    public static ServiceResult Ok() =>
        new() { Success = true, StatusCode = 200 };

    public static ServiceResult Fail(string message, int statusCode = 400) =>
        new() { Success = false, ErrorMessage = message, StatusCode = statusCode };

    public static ServiceResult NotFound(string message) => Fail(message, 404);
}

public sealed class ServiceResult<T> : ServiceResult
{
    public T? Data { get; private init; }

    public static ServiceResult<T> Ok(T data) =>
        new() { Success = true, StatusCode = 200, Data = data };

    public new static ServiceResult<T> Fail(string message, int statusCode = 400) =>
        new() { Success = false, ErrorMessage = message, StatusCode = statusCode };

    public new static ServiceResult<T> NotFound(string message) => Fail(message, 404);
}
