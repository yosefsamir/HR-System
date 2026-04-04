using System.Net;

namespace HR_system.Services
{
    public class WhatsAppOperationResult
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public HttpStatusCode? StatusCode { get; set; }

        public static WhatsAppOperationResult Ok() => new() { Success = true };

        public static WhatsAppOperationResult Fail(string error, HttpStatusCode? statusCode = null) => new()
        {
            Success = false,
            Error = error,
            StatusCode = statusCode
        };
    }
}
