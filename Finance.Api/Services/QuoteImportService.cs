using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Finance.Api.Controllers;
using Finance.Api.Domain;
using Finance.Api.Domain.ValueObjects;
using Finance.Api.Extentions;
using Finance.Api.Models;
using System.Runtime.Intrinsics.X86;

namespace Finance.Api.Services
{
    public class QuoteImportService : IQuoteImportService
    {
        public async Task<string> GetQuotesAsync(CompanyEntity company, DateTime from, DateTime to)
        {
            var ticker = company.Ticker;
            var fromDate = from.ToUnixTime();
            var toDate = to.ToUnixTime();

            using var client = new HttpClient();
            var url = $"https://query1.finance.yahoo.com/v7/finance/download/{ticker}?period1={fromDate}&period2={toDate}&interval=1d&events=history&includeAdjustedClose=true";
            var result = await client.GetStringAsync(url).ConfigureAwait(false);

            return result;
        }

        public async Task<DownloadData[]> GetRawQuotesData()
        {
            var connectionString = "DefaultEndpointsProtocol=https;AccountName=financequotestorage;AccountKey=zF9IfHEw6C1FVGcoC07HngYDvvgsBuHc+Ww1U2fqTIdAIZUyqUIJs3ef4ZR8+Z5/kzHi95igdy1Pm+uuIOp12w==;EndpointSuffix=core.windows.net";
            BlobServiceClient client = new BlobServiceClient(connectionString: connectionString);
            BlobContainerClient blobContainerClientSource = client.GetBlobContainerClient("raw");
            var downloadDatas = blobContainerClientSource.GetBlobs()
                .Where(b => b.Name.EndsWith("eod.json"))
                .Select(async item =>
                {
                    var downloadResult = await blobContainerClientSource.GetBlobClient(item.Name).DownloadContentAsync();
                    return new DownloadData(item.Name, downloadResult);
                });

            var r = await Task.WhenAll(downloadDatas);

            return r;
        }
    }

    public readonly record struct DownloadData(string Name, Response<BlobDownloadResult> Content);
    public readonly record struct CompanyContents<T>(CompanyEntity Company, T Content);
    public readonly record struct Ema(int Window, DatedSeries<double> Series);

    public readonly record struct EmaFan(CompanyEntity Company, IEnumerable<Ema> Fan);

    public class EmaFanEntity
    {
        public EmaFanEntity(Guid id, Guid companyId, ICollection<Ema> fan)
        {
            Id = id;
            CompanyId = companyId;
            Fan = fan;
        }

        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public ICollection<Ema> Fan { get; set; }
    }
}
