using Finance.Api.Domain.ValueObjects;
using Finance.Api.Services;
using Finance.Application.Contracts.Infrastructure;
using Finance.Application.Models;
using Finance.Domain.Entities;
using Finance.Domain.Entities.ValueObjects;
using Finance.Infrastructure.AzureLake;
using System.Text;
using System.Text.Json;

namespace Finance.Integration.Tests
{
    public class EndToEndTests
    {
        /// <summary>
        /// Infrastructure tests
        /// Importing and storing all data on companies in storage
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Test__ImportFromExternalSourceAsync()
        {
            IAzureTableService tableService = new AzureTableService();
            IAzureLakeService<CompanyEntity> lakeService = new AzureLakeService();
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
                var company = companies.SingleOrDefault(c => c.StorageFileName() == qs.Name);
                if (company != null)
                {
                    var contentString = Encoding.Default.GetString(qs.Content.Value.Content);
                    return new CompanyContents<Series<EndOfDay>>(company, JsonSerializer.Deserialize<Series<EndOfDay>>(contentString));
                }
                return new CompanyContents<Series<EndOfDay>>();
            }).ToList();

            // Series<DateTime, Close> Handle nulls
            var t = csContents.Select(c => new CompanyContents<Series<DateTime, Close>>(
                c.Company,
                new Series<DateTime, Close>(c.Content.SeriesCore.Where(o => o.Close.CloseCore != null).ToDictionary(sc => sc.DateTime, sc => sc.Close))));

            foreach (var companyContent in t)
            {
                // Ema Fan
                var closeSeries = new Series<DateTime, double>(companyContent.Content.ToDictionary(q => q.Key, q => q.Value.CloseCore.Value));
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
                    .Zip(ema100, (first, second) => (first.date, first.date.Date == second.Key.Date, first.ema18, first.ema50, ema100: second.Value))
                    .Zip(ema200, (first, second) => (first.date, first.date.Date == second.Key.Date, first.ema18, first.ema50, first.ema100, ema200: second.Value))
                    .ToDictionary(o => o.date, o => new EmaFanEntry { DateTime = o.date, Value18 = o.ema18, Value50 = o.ema50, Value100 = o.ema100, Value200 = o.ema200 });
                var emaFan = new Series<DateTime, EmaFanEntry>(emaFanDictionary);

                // JSON
                string emaFanJsonString = JsonSerializer.Serialize(emaFan.SeriesCore);

                // Azure Storage
                IAzureLakeService<CompanyEntity> lakeService = new AzureLakeService();
                //var company = (await tableService.GetCompaniesAsync()).ToList().Where(c => c.Ticker == "TSLA").SingleOrDefault();

                await lakeService.SaveJsonEma(emaFanJsonString, companyContent.Company);
                Dictionary<DateTime, EmaFanEntry>? d = JsonSerializer.Deserialize<Dictionary<DateTime, EmaFanEntry>>(emaFanJsonString);
                var ind2 = new Series<DateTime, EmaFanEntry>(d);
            }
        }

        [Fact]
        public async Task Test__Load_Eod_and_EmaFan__Calculate_trends_by_date()
        {
            // Fetch companies from Azure Table
            IAzureTableService tableService = new AzureTableService();
            IQuoteImportService service = new QuoteImportService();
            var companies = (await tableService.GetCompaniesAsync()).ToList();

            // Fetsh emafan from Azure blob
            IAzureLakeService<CompanyEntity> lakeService = new AzureLakeService();
            var emaFanJsonStrings = await lakeService.GetEmaFanFilesAsync(companies);
            var emaFans = emaFanJsonStrings.Select(companyContent =>
            {
                //var c = new CompanyContents<Series<DateTime, EmaFanEntry>>();
                var companyId = companyContent.Company.RowKey;
                var emaFanJsonString = companyContent.Content;
                Dictionary<DateTime, EmaFanEntry>? d = JsonSerializer.Deserialize<Dictionary<DateTime, EmaFanEntry>>(emaFanJsonString);
                var ind2 = new Series<DateTime, EmaFanEntry>(d);
                return new CompanyContents<Series<DateTime, EmaFanEntry>>(companyContent.Company, ind2);
                //return ind2;
            }).ToList();

            // Calculate Series<DateTime, TrendEntry>
            IEnumerable<CompanyContents<Series<DateTime, TrendEntry>>> trends = emaFans.Select(fan =>
            {
                return new CompanyContents<Series<DateTime, TrendEntry>>(fan.Company, new Series<DateTime, TrendEntry>(fan.Content.ToDictionary(o => o.Key, o =>
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
                })));
            });
            var cs = trends.Where(t => t.Content.Last().Value.TrendType == TrendType.Upp).Select(o => o);
            var lastTrend = trends.Select(content => content.Content.Last());
        }
    }
}
