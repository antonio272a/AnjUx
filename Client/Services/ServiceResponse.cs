namespace AnjUx.Client.Services
{
    public class ServiceResponse<T>
    {
        public T? Data { get; set; }
        public bool Success { get; set; } = true;
        public string Message { get; set; } = string.Empty;
    }

    public static class ServiceResponseExtensions
    {
        public static void Fail<T>(this ServiceResponse<T> response, Exception ex)
        {
            response.Success = false;
            response.Message = ex.Message;
            response.Data = default;
        }
    }
}