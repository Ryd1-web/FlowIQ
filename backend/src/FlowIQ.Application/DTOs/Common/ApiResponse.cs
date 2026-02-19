namespace FlowIQ.Application.DTOs.Common;

public record ApiResponse<T>(bool Success, string Message, T? Data)
{
    public static ApiResponse<T> Ok(T data, string message = "Success")
        => new(true, message, data);

    public static ApiResponse<T> Fail(string message)
        => new(false, message, default);
}

public record ErrorResponse(string Message, IDictionary<string, string[]>? Errors = null);
