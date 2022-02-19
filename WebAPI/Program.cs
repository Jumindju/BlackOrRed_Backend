using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((_, serilogConfig) =>
{
    serilogConfig.ReadFrom.Configuration(builder.Configuration);
});

var app = builder
    .Build();

app.UseSerilogRequestLogging();

app.MapGet("/test", () => "Hello World!");

try
{
    app.Run();
    app.Logger.LogInformation("WebAPI started");
}
finally
{
    app.Logger.LogInformation("WebAPI finished");
}
