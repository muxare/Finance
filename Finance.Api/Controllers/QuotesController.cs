using Azure.Storage.Blobs;
using Csv;
using Finance.Api.Services;
using Finance.Application.Contracts.Infrastructure;
using Finance.Application.Models;
using Finance.Domain.Entities;
using Finance.Domain.Entities.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text;
using System.Text.Json;
using BlobServiceClient = Azure.Storage.Blobs.BlobServiceClient;

namespace Finance.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class QuotesController : ControllerBase
    {
        private IAzureTableService TableService { get; }
        private IQuoteImportService ImportService { get; }
        private IAzureLakeService<CompanyEntity> LakeService { get; }

        public QuotesController(IAzureTableService tableService, IQuoteImportService importService, IAzureLakeService<CompanyEntity> lakeService)
        {
            TableService = tableService ?? throw new ArgumentNullException(nameof(tableService));
            ImportService = importService ?? throw new ArgumentNullException(nameof(importService));
            LakeService = lakeService ?? throw new ArgumentNullException(nameof(lakeService));
        }

        /// <summary>
        /// External source => csv => Series<EndOfDay> => JSON => Azure Storage
        /// </summary>
        /// <returns></returns>
        [HttpPost(template: "import", Name = "StartImport")]
        public async Task StartImportAsync()
        {
            var companies = await TableService.GetCompaniesAsync();
            foreach (var company in companies.Where(c => c.Ticker != null))
            {
                var quotes = await ImportService.GetQuotesAsync(company, DateTime.MinValue, DateTime.UtcNow);

                Series<EndOfDay> series =
                    new Series<EndOfDay>(
                        quotes.Split('\n').Skip(1).Select(o => (EndOfDay)o)
                        );

                await LakeService.SaveQuotes(quotes, company);
            }
        }

        /// <summary>
        /// Azure Storage => JSON => Series<EndOfDay> => Ema Fan => JSON => Azure Storage
        /// </summary>
        /// <returns></returns>
        [HttpPost(template: "ema", Name = "EmaFanCalculation")]
        public async Task EmaFanCalculation()
        {
            // Read up on azure storage here https://learn.microsoft.com/en-us/azure/storage/blobs/storage-blob-dotnet-get-started
            // Setup and authenticate
            // Create a BlobServiceClient that will authenticate through connection string
            var connectionString = "DefaultEndpointsProtocol=https;AccountName=financequotestorage;AccountKey=zF9IfHEw6C1FVGcoC07HngYDvvgsBuHc+Ww1U2fqTIdAIZUyqUIJs3ef4ZR8+Z5/kzHi95igdy1Pm+uuIOp12w==;EndpointSuffix=core.windows.net";
            BlobServiceClient client = new BlobServiceClient(connectionString: connectionString);

            // Fetch raw edo and setup ema destination
            BlobContainerClient blobContainerClientDestination = client.GetBlobContainerClient("ema");
            await blobContainerClientDestination.CreateIfNotExistsAsync();

            var cs = await TableService.GetCompaniesAsync();
            var quotes = await ImportService.GetRawQuotesData();
            var csContents = quotes.Select(qs =>
            {
                var c = cs.Where(c => c.StorageFileName() == qs.Name).SingleOrDefault();
                if (c != null)
                {
                    var contentString = Encoding.Default.GetString(qs.Content.Value.Content);
                    return new CompanyContents<Series<EndOfDay>>(c, JsonSerializer.Deserialize<Series<EndOfDay>>(contentString));
                }
                return new CompanyContents<Series<EndOfDay>>();
            });
        }

        private Dictionary<DateTime, QuoteDtoYahoo> ParseQuoteString(string quotes)
        {
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

            return res;
        }

        private DatedSeries<double> EmaInvestiopedia(DatedSeries<double> list, int days)
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
