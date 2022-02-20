using GameService;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services => { services.AddHostedService<Worker>(); })
    .ConfigureLogging((context, logConfig) =>
    {
        logConfig.ClearProviders();
        
        // TODO Check logging for azure functions
        if (context.HostingEnvironment.IsDevelopment())
        {
            logConfig.AddConsole();
        }
    })
    .Build();

await host.RunAsync();