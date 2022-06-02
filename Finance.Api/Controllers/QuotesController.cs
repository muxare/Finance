using Csv;
using Finance.Api.Domain;
using Finance.Api.Models;
using Finance.Api.Services;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace Finance.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class QuotesController : ControllerBase
    {
        private IAzureTableService TableService { get; }
        private IQuoteImportService ImportService { get; }
        private IAzureLakeService LakeService { get; }

        public QuotesController(IAzureTableService tableService, IQuoteImportService importService, IAzureLakeService lakeService)
        {
            TableService=tableService ?? throw new ArgumentNullException(nameof(tableService));
            ImportService=importService ?? throw new ArgumentNullException(nameof(importService));
            LakeService=lakeService ?? throw new ArgumentNullException(nameof(lakeService));
        }

        [HttpPost(Name = "StartImport")]
        public async Task StartImportAsync()
        {
            var companies = await TableService.GetCompaniesAsync();
            foreach (var company in companies)
            {
                var quotes = await ImportService.GetQuotesAsync(company.Ticker, DateTime.MinValue, DateTime.UtcNow);

                var res = quotes.Split('\n').Skip(1).Select(line =>
                {
                    var l = line.Split(',');
                    try
                    {
                        return new QuoteDtoYahoo(DateTime.Parse(l[0], CultureInfo.InvariantCulture), l[1] == "null" ? null : double.Parse(l[1]), l[2] == "null" ? null : double.Parse(l[2]), l[3] == "null" ? null : double.Parse(l[3]), l[4] == "null" ? null : double.Parse(l[4]), l[5] == "null" ? null : double.Parse(l[5]), l[6] == "null" ? null : long.Parse(l[6]));
                    }
                    catch (Exception ex)
                    {

                    }
                    return new QuoteDtoYahoo();

                }).ToDictionary(o => o.Date, o => o);

                var closeSeries = new DatedSeries<double>(res.Where(o => o.Value.Close != null).Select(o => new { key = o.Key, value = o.Value.Close }).ToDictionary(o => o.key, o => o.value.Value));
                var ema18 = EmaInvestiopedia(closeSeries, 18);
                var ema50 = EmaInvestiopedia(closeSeries, 50);
                var ema100 = EmaInvestiopedia(closeSeries, 100);
                var ema200 = EmaInvestiopedia(closeSeries, 200);

                var emaFan = CalculateEmaFan(quotes);

                await LakeService.SaveQuotes(quotes, company);
            }

            return;
        }

        DatedSeries<double> EmaInvestiopedia(DatedSeries<double> list, int days)
        {
            var smoothing = 2.0;
            var previous = 0.0;
            var beta = smoothing / (1 + days);
            var series = list.Select((d, i) =>
            {
                var current = d.Value * beta + previous * (1.0 - beta);
                previous = current;
                return new { DateTime = d.Key, Value = current };
            }).ToDictionary(o => o.DateTime, o => o.Value);

            var ema = new DatedSeries<double>(series);

            return ema;
        }

        private (DoubleSeries ema18, DoubleSeries ema50, DoubleSeries ema100, DoubleSeries ema200) CalculateEmaFan(string quotes)
        {
            var result = (new DoubleSeries(), new DoubleSeries(), new DoubleSeries(), new DoubleSeries());

            foreach (var row in CsvReader.ReadFromText(quotes))
            {

            }

            return result;
        }
    }

    public interface ITimed
    {
        public DateTime Date { get; set; }
    }

    public class Series : ITimed
    {
        public DateTime Date { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }

    public class DatedEntry
    {
        public DateTime Date { get; set; }
        public double Value { get; set; }
    }
    
    public class DoubleSeries
    {
        public ICollection<DatedEntry> Series { get; set; }
    }
}