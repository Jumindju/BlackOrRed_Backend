using Microsoft.AspNetCore.Mvc;
using WebAPI.Handler;
using WebAPI.Helper.Extensions;
using WebAPI.Interfaces.Handler;
using WebAPI.Interfaces.Repositories;
using WebAPI.Model.Lobby;
using WebAPI.Repositories;

namespace WebAPI.Endpoints;

public static class LobbyEndpoints
{
    public static void MapLobbyEndpoints(this WebApplication app)
    {
        app.MapPost("/lobby", CreateLobby);
    }


    public static void AddLobbyServices(this IServiceCollection services)
    {
        services.AddSingleton<ILobbyHandler, LobbyHandler>();
        services.AddSingleton<ILobbyRepository, LobbyRepository>();
    }

    private static async Task<IResult> CreateLobby([FromServices] ILobbyHandler lobbyHandler, HttpContext context,
        [FromBody] LobbySettings? lobbySettings)
    {
        if (lobbySettings is null)
            return Results.BadRequest();

        var curPlayer = context.GetPlayer();
        if (curPlayer is null)
            return Results.Unauthorized();

        var createdLobby = await lobbyHandler.CreateLobby(curPlayer.PlayerUId, lobbySettings);

        return Results.Created($"lobby/{createdLobby.PublicId}", createdLobby);
    }
}