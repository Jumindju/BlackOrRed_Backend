using WebAPI.Model;
using WebAPI.Model.Lobby;

namespace WebAPI.Helper.Middleware;

public class UserProviderMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;

    public UserProviderMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
    {
        _next = next;
        _logger = loggerFactory.CreateLogger<RequestLoggingMiddleware>();
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            var userName = context.Request.Headers[Constants.UserNameHeader];
            var rawGuid = context.Request.Headers[Constants.UserUIdHeader];
            
            if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(rawGuid) && Guid.TryParse(rawGuid, out var userGuid))
            {
                var curPlayer = new LobbyPlayer(userGuid, userName);
                context.Items.Add(Constants.UserItemKey, curPlayer);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not set user in header");
        }
        
        await _next(context);
    }
}