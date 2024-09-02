using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Linq;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Log request details before the next middleware is executed
        await LogRequestAsync(context);

        // Call the next middleware in the pipeline
        await _next(context);

        // Optionally log response details after the next middleware has been executed
        await LogResponseAsync(context);
    }

    private async Task LogRequestAsync(HttpContext context)
    {
        // Extract user details
        var user = context.User.Identity.IsAuthenticated ? context.User.Identity.Name : "Anonymous";

        // Extract request details
        var request = context.Request;
        var method = request.Method;
        var path = request.Path;
        var queryString = request.QueryString.HasValue ? request.QueryString.Value : string.Empty;
        var ip = context.Connection.RemoteIpAddress?.ToString();

        // Log details into the database
        using (var scope = context.RequestServices.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<YourDbContext>();

            var log = new HttpRequestLog
            {
                User = user,
                Method = method,
                Path = path,
                QueryString = queryString,
                IpAddress = ip,
                Timestamp = DateTime.UtcNow
            };

            dbContext.HttpRequestLogs.Add(log);
            await dbContext.SaveChangesAsync();
        }

        _logger.LogInformation($"Logged HTTP request: {method} {path}{queryString}");
    }

    private async Task LogResponseAsync(HttpContext context)
    {
        var statusCode = context.Response.StatusCode;

        // Log the response status code if needed
        _logger.LogInformation($"Response status code: {statusCode}");

        await Task.CompletedTask;
    }
}
