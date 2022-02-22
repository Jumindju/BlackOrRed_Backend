using System.Diagnostics;
using Microsoft.AspNetCore.Http.Extensions;
using WebAPI.Model;
using WebAPI.Model.Exceptions;
using WebAPI.Model.Helper;
using WebAPI.Model.Lobby;

namespace WebAPI.Helper.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
    {
        _next = next;
        _logger = loggerFactory.CreateLogger<RequestLoggingMiddleware>();
    }

    public async Task Invoke(HttpContext context)
    {
        var timer = new Stopwatch();
        timer.Start();

        try
        {
            context.Items.Add(Constants.RequestGuidItemKey, Guid.NewGuid());
            await _next(context);
        }
        catch (StatusCodeException ex)
        {
            context.Response.StatusCode = (int)ex.StatusCode;

            try
            {
                await context.Response.WriteAsJsonAsync(new StatusCodeExceptionResponse(ex.Message, ex.InnerException),
                    Serializer.IgnoreNullSerializer);
            }
            catch (Exception writeEx)
            {
                _logger.LogError(writeEx, "Could not write status code response");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occured");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        }
        finally
        {
            timer.Stop();

            Guid? playerUid = null;
            if (context.Items[Constants.PlayerItemKey] is LobbyPlayer lb)
                playerUid = lb.PlayerUId;

            _logger.LogInformation(
                "Request {Method} {Path}: {StatusCode} in {ElapsedTime}ms; IpAdr: {IpAdr} PlayerUId: {PlayerUId} UserAgent: {UserAgent} fullUrl: {FullUrl} Req_UId: {ReqGuid}",
                context.Request.Method,
                context.Request.Path.Value,
                context.Response.StatusCode,
                timer.ElapsedMilliseconds,
                context.Connection.RemoteIpAddress?.ToString(),
                playerUid,
                context.Request.Headers.UserAgent,
                context.Request.GetDisplayUrl(),
                context.Items[Constants.RequestGuidItemKey]
            );
        }
    }
}