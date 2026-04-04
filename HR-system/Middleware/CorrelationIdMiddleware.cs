namespace HR_system.Middleware
{
    public class CorrelationIdMiddleware
    {
        public const string HeaderName = "X-Correlation-ID";
        public const string ItemKey = "CorrelationId";

        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = context.Request.Headers.TryGetValue(HeaderName, out var incoming) &&
                                !string.IsNullOrWhiteSpace(incoming)
                ? incoming.ToString()
                : context.TraceIdentifier;

            context.Items[ItemKey] = correlationId;
            context.Response.Headers[HeaderName] = correlationId;

            await _next(context);
        }
    }
}