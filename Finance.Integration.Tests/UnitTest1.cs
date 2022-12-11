using Finance.Api.Domain;
using Finance.Api.Domain.ValueObjects;
using Finance.Api.Models;
using Finance.Api.Services;
using System.Text;
using System.Text.Json;

namespace Finance.Integration.Tests
{
    public class EndToEndTests
    {
        [Fact]
        public async Task Test__ImportFromExternalSourceAsync()
        {
            IAzureTableService tableService = new AzureTableService();
            IAzureLakeService lakeService = new AzureLakeService();
            var cs = (await tableService.GetCompaniesAsync()).ToList();

            IQuoteImportService service = new QuoteImportService();

            foreach (var companyEntity in cs)
            {
                //IQuoteImportService service = new QuoteImportService();
                var quotes = await service.GetQuotesAsync(
                    companyEntity, DateTime.MinValue, DateTime.UtcNow);

                var enumerable = quotes.Split('\n').Skip(1).Select(o => (EndOfDay)o);
                var series = new Series<EndOfDay>(enumerable);

                var jsonString = JsonSerializer.Serialize(series);

                //var company = (await tableService.GetCompaniesAsync()).ToList().Where(c => c.Ticker == "TSLA").SingleOrDefault();

                await lakeService.SaveJson(jsonString, companyEntity);
            }
        }

        [Fact]
        public async Task Test__AzureStorage_Json_To_SeriesEndOfDay_To_EmaFan_To_Json_To_AzureStorage_With_TeslaJsonData()
        {
            // Azure Storage => JSON => Series<EndOfDay> => Ema Fan => JSON => Azure Storage
            IAzureTableService tableService = new AzureTableService();
            IQuoteImportService service = new QuoteImportService();
            var companies = (await tableService.GetCompaniesAsync()).ToList();
            var quotes = await service.GetRawQuotesData();

            var csContents = quotes.Select(qs =>
            {
                var company = companies.Where(c => c.StorageFileName() == qs.Name).SingleOrDefault();
                if (company != null)
                {
                    var contentString = Encoding.Default.GetString(qs.Content.Value.Content);
                    return new CompanyContents<Series<EndOfDay>>(company, JsonSerializer.Deserialize<Series<EndOfDay>>(contentString), null);
                }
                return new CompanyContents<Series<EndOfDay>>();
            }).ToList();

            // Series<DateTime, Close> Handle nulls
            var t = csContents.Select(c => new CompanyContents<Series<DateTime, Close>>(
                c.Company,
                new Series<DateTime, Close>(c.Content.SeriesCore.ToDictionary(sc => sc.DateTime, sc => sc.Close)),
                new Series<DateTime, Close>(c.Content.SeriesCore.Where(o => o.Close.CloseCore != null).ToDictionary(sc => sc.DateTime, sc => sc.Close))));

            foreach (var companyContent in t)
            {
                // Ema Fan
                Series<DateTime, double> closeSeries = new Series<DateTime, double>(companyContent.AdjustedContent.ToDictionary(q => q.Key, q => q.Value.CloseCore.Value));
                var ema18Processor = new Ema(18);
                var ema50Processor = new Ema(50);
                var ema100Processor = new Ema(100);
                var ema200Processor = new Ema(200);

                var ema18 = ema18Processor.Calculate(closeSeries);
                var ema50 = ema50Processor.Calculate(closeSeries);
                var ema100 = ema100Processor.Calculate(closeSeries);
                var ema200 = ema200Processor.Calculate(closeSeries);

                //ema18.Zip(ema50).Zip(ema100).Zip(ema200);
                var emaFanDictionary = ema18.Zip(ema50, (first, second) => (date: first.Key, first.Key.Date == second.Key.Date, ema18: first.Value, ema50: second.Value))
                    .Zip(ema100, (first, second) => (date: first.date, first.date.Date == second.Key.Date, ema18: first.Item3, ema50: first.Item4, ema100: second.Value))
                    .Zip(ema200, (first, second) => (date: first.date, first.date.Date == second.Key.Date, ema18: first.Item3, ema50: first.Item4, ema100: first.Item5, ema200: second.Value))
                    .ToDictionary(o => o.date, o => new EmaFanEntry { DateTime = o.date, Value18 = o.ema18, Value50 = o.ema50, Value100 = o.ema100, Value200 = o.ema200 });
                Series<DateTime, EmaFanEntry> emaFan = new Series<DateTime, EmaFanEntry>(emaFanDictionary);

                // JSON
                string emaFanJsonString = JsonSerializer.Serialize(emaFan.SeriesCore);

                // Azure Storage
                IAzureLakeService lakeService = new AzureLakeService();
                //var company = (await tableService.GetCompaniesAsync()).ToList().Where(c => c.Ticker == "TSLA").SingleOrDefault();

                await lakeService.SaveJsonEma(emaFanJsonString, companyContent.Company);
                Dictionary<DateTime, EmaFanEntry>? d = JsonSerializer.Deserialize<Dictionary<DateTime, EmaFanEntry>>(emaFanJsonString);
                Series<DateTime, EmaFanEntry> ind2 = new Series<DateTime, EmaFanEntry>(d);
            }
        }

        [Fact]
        public async Task Test__Load_Eod_and_EmaFan__Calculate_trends()
        {
            // Fetch companies from Azure Table
            IAzureTableService tableService = new AzureTableService();
            IQuoteImportService service = new QuoteImportService();
            var companies = (await tableService.GetCompaniesAsync()).ToList();

            // Fetsh emafan from Azure blob
            IAzureLakeService lakeService = new AzureLakeService();
            var emaFanJsonStrings = await lakeService.GetEmaFanFilesAsync();
            var emaFans = emaFanJsonStrings.Select(emaFanJsonString =>
            {
                Dictionary<DateTime, EmaFanEntry>? d = JsonSerializer.Deserialize<Dictionary<DateTime, EmaFanEntry>>(emaFanJsonString);
                Series<DateTime, EmaFanEntry> ind2 = new Series<DateTime, EmaFanEntry>(d);
                return ind2;
            }).ToList();

            // Calculate Series<DateTime, TrendEntry>
            var trends = emaFans.Select(fan =>
            {
                return new Series<DateTime, TrendEntry>(fan.ToDictionary(o => o.Key, o =>
                {
                    var type = TrendType.Between;
                    if (o.Value.Value18 > o.Value.Value50 && o.Value.Value50 > o.Value.Value100 && o.Value.Value100 > o.Value.Value200)
                        type = TrendType.Upp;
                    if (o.Value.Value18 < o.Value.Value50 && o.Value.Value50 < o.Value.Value100 && o.Value.Value100 < o.Value.Value200)
                        type = TrendType.Down;
                    return new TrendEntry
                    {
                        DateTime = o.Key,
                        TrendType = type
                    };
                }));
            });

            var t = 0;
        }

        [Fact]
        public async Task Test__Load_Eod__Calculate_PriceActions()
        {
            // Fetch companies from Azure Table
            IAzureTableService tableService = new AzureTableService();
            IQuoteImportService service = new QuoteImportService();
            var companies = (await tableService.GetCompaniesAsync()).ToList();

            // Fetsh eod from Azure blob
            IAzureLakeService lakeService = new AzureLakeService();

            // Calculate a series identifying price actions
        }
    }
}
