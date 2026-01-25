// Common/ServiceResult.cs
namespace DayLog.Common
{
    public class ServiceResult<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? ErrorMessage { get; set; }
        public object? ErrorData { get; set; }

        public static ServiceResult<T> Ok(T data) => new() { Success = true, Data = data };
        public static ServiceResult<T> Fail(string error) => new() { Success = false, ErrorMessage = error };
        public static ServiceResult<T> Fail(string error, object? errorData) => new() { Success = false, ErrorMessage = error, ErrorData = errorData };
    }
}
