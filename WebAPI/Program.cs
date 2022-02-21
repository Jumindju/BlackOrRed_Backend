using Microsoft.AspNetCore.HttpOverrides;
using WebAPI.Helper.Middleware;

var builder = WebApplication.CreateBuilder(args);

// configure logging/monitoring
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddApplicationInsightsTelemetry();
}

var app = builder
    .Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseRequestLoggingMiddleware();
app.UseUserProvider();

var rnd = new Random();
app.MapGet("/test", (ILogger<Program> logger) =>
{
    logger.LogInformation("Test log data");
    return Results.Ok("Hello Wasabi!");
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