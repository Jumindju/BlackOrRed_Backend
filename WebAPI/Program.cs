using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Handler;
using WebAPI.Helper.Extensions;
using WebAPI.Helper.Middleware;
using WebAPI.Interfaces.Handler;
using WebAPI.Interfaces.Repositories;
using WebAPI.Model.Lobby;
using WebAPI.Repositories;

var builder = WebApplication.CreateBuilder(args);

// configure logging/monitoring
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddApplicationInsightsTelemetry();
}

// configure services
builder.Services.AddSingleton<ILobbyHandler, LobbyHandler>();
builder.Services.AddSingleton<ILobbyRepository, LobbyRepository>();

var app = builder
    .Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseRequestLoggingMiddleware();
app.UseUserProvider();

// map endpoints
app.MapPost("/lobby",
    async ([FromServices] ILobbyHandler lobbyHandler, HttpContext context, [FromBody] LobbySettings? lobbySettings) =>
    {
        if (lobbySettings is null)
            return Results.BadRequest();

        var curPlayer = context.GetPlayer();
        if (curPlayer is null)
            return Results.Unauthorized();

        var createdLobby = await lobbyHandler.CreateLobby(curPlayer.PlayerUId, lobbySettings);

        return Results.Created($"lobby/{createdLobby.PublicId}", createdLobby);
    });

var logger = app.Services.GetRequiredService<ILogger<Program>>();
try
{
    logger.LogInformation("Web API started");
    app.Run();
}
finally
{
    logger.LogInformation("WebAPI finished");
}