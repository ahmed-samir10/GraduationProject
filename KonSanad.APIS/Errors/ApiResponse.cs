
namespace KonSanad.Core.Dtos
{
    public class ApiResponse
    {
        public int StatusCode { get; }
        public string Message { get; }

        public ApiResponse(int statusCode, string? message = null)
        {
            StatusCode = statusCode;
            Message = message ?? GetDefaultMessage(statusCode);
        }

        private static string GetDefaultMessage(int statusCode) =>
            statusCode switch
            {
                400 => "A bad request, you have made",
                401 => "You are not authorized",
                404 => "Resource was not found",
                500 => "Server error occurred",
                _ => null ?? string.Empty
            };
    }
}
