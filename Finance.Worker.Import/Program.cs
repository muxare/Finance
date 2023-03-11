using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Finance.Api.Services;
using Finance.Api.Domain.ValueObjects;
using System.Text.Json;
using Finance.Infrastructure.AzureLake;
using Finance.Application.Contracts.Infrastructure;
using Finance.Application.Models;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services => services.AddTransient<FetchYahooFinanceDataService>())
    .Build();

var my = host.Services.GetRequiredService<FetchYahooFinanceDataService>();
await my.ExecuteAsync();

// This should run a while, 1h, after the appropriate market has closed to get the end of day data
public class FetchYahooFinanceDataService
{
    public readonly ILogger<FetchYahooFinanceDataService> _logger;

    public FetchYahooFinanceDataService(ILogger<FetchYahooFinanceDataService> logger)
    {
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        IAzureTableService tableService = new AzureTableService();
        IAzureLakeService<CompanyEntity> lakeService = new AzureLakeService();
        var cs = (await tableService.GetCompaniesAsync()).ToList();

        IQuoteImportService service = new QuoteImportService();

        foreach (var companyEntity in cs)
        {
            var quotes = await service.GetQuotesAsync(
                companyEntity, DateTime.MinValue, DateTime.UtcNow);

            var enumerable = quotes.Split('\n').Skip(1).Select(o => (EndOfDay)o);
            var series = new Series<EndOfDay>(enumerable);

            var jsonString = JsonSerializer.Serialize(series);

            await lakeService.SaveJson(jsonString, companyEntity);
        }
    }
}
