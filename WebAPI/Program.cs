var builder = WebApplication.CreateBuilder(args);

// configure logging/monitoring
builder.Logging.ClearProviders();

// TODO disable application insights for dev
if (builder.Environment.IsDevelopment())
{
    builder.Logging.AddConsole();
}
else
{
    builder.Services.AddApplicationInsightsTelemetry();
}

var app = builder
    .Build();

var rnd = new Random();
app.MapGet("/test", (ILogger<Program> logger) =>
{
    if (rnd.Next(1, 4) > 2)
    {
        logger.LogInformation("Test log data");
        return Results.Ok("Hello World!");
    }
    
    logger.LogError(new Exception("Test exception"), "Error");
    return Results.StatusCode(500);
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