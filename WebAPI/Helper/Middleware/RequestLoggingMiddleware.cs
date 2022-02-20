using Microsoft.AspNetCore.Http.Extensions;

namespace WebAPI.Helper.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;

    private const string RequestGuidItemKey = "req_guid";

    public RequestLoggingMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
    {
        _next = next;
        _logger = loggerFactory.CreateLogger<RequestLoggingMiddleware>();
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            context.Items.Add(RequestGuidItemKey, Guid.NewGuid());
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occured");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        }
        finally
        {
            _logger.LogInformation(
                "Request {method} {path}: {statusCode}; IpAdr: {ipAdr} UserAgent: {userAgent} fullUrl: {fullUrl} Req_UId: {reqGuid}",
                context.Request.Method,
                context.Request.Path.Value,
                context.Response.StatusCode,
                context.Connection.RemoteIpAddress.ToString(),
                context.Request.Headers.UserAgent,
                context.Request.GetDisplayUrl(),
                context.Items[RequestGuidItemKey]
            );
        }
    }
}