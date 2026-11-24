namespace FinTrack.Api.Helpers;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }

    public static ApiResponse<T> Ok(T data, string? message = null)
        => new() { Success = true, Data = data, Message = message };
}

public class ApiError
{
    public bool Success => false;
    public string Message { get; set; }
    public object? Errors { get; set; }

    public ApiError(string message, object? errors = null)
    {
        Message = message;
        Errors = errors;
    }
}
