using WebAPI.Model;
using WebAPI.Model.Lobby;

namespace WebAPI.Helper.Middleware;

public class PlayerProviderMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;

    public PlayerProviderMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
    {
        _next = next;
        _logger = loggerFactory.CreateLogger<RequestLoggingMiddleware>();
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            var playerName = context.Request.Headers[Constants.PlayerNameHeader];
            var rawGuid = context.Request.Headers[Constants.PlayerUIdHeader];
            
            if (!string.IsNullOrEmpty(playerName) && !string.IsNullOrEmpty(rawGuid) && Guid.TryParse(rawGuid, out var playerUid))
            {
                var curPlayer = new LobbyPlayer(playerUid, playerName);
                context.Items.Add(Constants.PlayerItemKey, curPlayer);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not set player from header");
        }
        
        await _next(context);
    }
}