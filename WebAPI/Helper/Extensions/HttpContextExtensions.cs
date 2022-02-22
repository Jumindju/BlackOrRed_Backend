using WebAPI.Model.Lobby;
using Constants = WebAPI.Model.Constants;

namespace WebAPI.Helper.Extensions;

public static class HttpContextExtensions
{
    public static LobbyPlayer? GetPlayer(this HttpContext context) 
        => (LobbyPlayer?)context.Items[Constants.PlayerItemKey];
}