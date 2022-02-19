using GameService;
using Serilog;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services => { services.AddHostedService<Worker>(); })
    .UseSerilog((context, serilogConfig) =>
    {
        serilogConfig.ReadFrom.Configuration(context.Configuration);
    })
    .Build();

await host.RunAsync();