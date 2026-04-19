namespace Application.Common;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public int StatusCode { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();

    public static ApiResponse<T> Ok(T data, string? message = null) => new()
    {
        Success = true,
        StatusCode = 200,
        Data = data,
        Message = message
    };

    public static ApiResponse<T> Created(T data, string? message = null) => new()
    {
        Success = true,
        StatusCode = 201,
        Data = data,
        Message = message
    };

    public static ApiResponse<T> NoContent(string message) => new()
    {
        Success = true,
        StatusCode = 204,
        Message = message
    };

    public static ApiResponse<T> Fail(int statusCode, string message, List<string>? errors = null) => new()
    {
        Success = false,
        StatusCode = statusCode,
        Message = message,
        Errors = errors ?? new()
    };
}

// Non-generic for no-data responses
public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse Ok(string message) => new()
    {
        Success = true,
        StatusCode = 200,
        Message = message
    };

    public static new ApiResponse NoContent(string message) => new()
    {
        Success = true,
        StatusCode = 204,
        Message = message
    };
}
