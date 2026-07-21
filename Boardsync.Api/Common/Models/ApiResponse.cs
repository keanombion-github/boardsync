namespace Boardsync.Api.Common.Models;

/// <summary>
/// Standard API response wrapper.
/// Every endpoint returns this shape so the frontend always knows:
/// - success: did the request work?
/// - data: the actual payload (null on error)
/// - error: error details (null on success)
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public ApiError? Error { get; init; }

    public static ApiResponse<T> Ok(T data) => new()
    {
        Success = true,
        Data = data,
        Error = null
    };

    public static ApiResponse<T> Fail(string code, string message) => new()
    {
        Success = false,
        Data = default,
        Error = new ApiError { Code = code, Message = message }
    };
}

public class ApiError
{
    public required string Code { get; init; }
    public required string Message { get; init; }
    public List<string> Details { get; init; } = [];
}
