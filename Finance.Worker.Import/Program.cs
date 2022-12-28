using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services => { services.AddTransient<MyService>(); })
    .Build();

var my = host.Services.GetRequiredService<MyService>();
await my.ExecuteAsync();

internal class MyService
{
    private readonly ILogger<MyService> _logger;

    public MyService(ILogger<MyService> logger)
    {
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken stoppingToken = default)
    {
        _logger.LogInformation("Doing something");
    }
}

public class FetchYahooFinanceDataService
{
    private readonly ILogger<FetchYahooFinanceDataService> _logger;
    //private readonly IServiceProvider _serviceProvider;
}
