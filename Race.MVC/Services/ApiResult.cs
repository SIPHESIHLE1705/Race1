namespace Race.MVC.Services
{
    // What every call to the API returns: success flag, HTTP status and a
    // user-friendly message (never a raw exception or stack trace).
    public class ApiResult
    {
        public bool Success { get; protected init; }
        public int StatusCode { get; protected init; }
        public string Message { get; protected init; } = string.Empty;
        public bool IsUnauthorized => StatusCode == 401;

        public static ApiResult Pass(int statusCode, string message = "")
            => new() { Success = true, StatusCode = statusCode, Message = message };

        public static ApiResult Fail(int statusCode, string message)
            => new() { Success = false, StatusCode = statusCode, Message = message };
    }

    public class ApiResult<T> : ApiResult
    {
        public T? Data { get; private init; }

        public static ApiResult<T> PassWith(T? data, int statusCode)
            => new() { Success = true, StatusCode = statusCode, Data = data };

        public static ApiResult<T> FailWith(int statusCode, string message)
            => new() { Success = false, StatusCode = statusCode, Message = message };
    }
}
