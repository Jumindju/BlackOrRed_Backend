using Cosmonaut.Extensions.Microsoft.DependencyInjection;
using Microsoft.AspNetCore.HttpOverrides;
using WebAPI.Endpoints;
using WebAPI.Helper.Cosmos;
using WebAPI.Helper.Middleware;
using WebAPI.Model.Lobby;

var builder = WebApplication.CreateBuilder(args);

// configure logging/monitoring
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddApplicationInsightsTelemetry();
}

// cosmos services
builder.Services.AddCosmosStore<LobbyDb>(
    CosmosHelper.GetCosmosStoreSettings(builder.Configuration)
);

// configure services
builder.Services.AddLobbyServices();

var app = builder
    .Build();

// add middlewares
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});
app.UseRequestLoggingMiddleware();
app.UseUserProvider();

// add endpoints
app.MapLobbyEndpoints();

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