using Azure;
using Azure.Data.Tables;
using Azure.Storage;
using Azure.Storage.Files.DataLake;
using Finance.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Finance.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class QuotesController : ControllerBase
    {
        private string connectionString = "DefaultEndpointsProtocol=https;AccountName=financequotestorage;AccountKey=zF9IfHEw6C1FVGcoC07HngYDvvgsBuHc+Ww1U2fqTIdAIZUyqUIJs3ef4ZR8+Z5/kzHi95igdy1Pm+uuIOp12w==;EndpointSuffix=core.windows.net";
        private string tablename = "Companies";

        [HttpPost(Name = "StartImport")]
        public async Task StartImportAsync()
        {
            
            var companies = await  GetCompaniesAsync();
            foreach (var company in companies)
            {
                var quotes = await GetQuotesAsync(company.Ticker, DateTime.MinValue, DateTime.UtcNow);

                await SaveQuotes(quotes, company);
            }

            return;
        }

        private async Task SaveQuotes(string quotes, CompanyEntity companyEntity)
        {
            var client = GetDataLakeServiceClient("financequotestorage", "zF9IfHEw6C1FVGcoC07HngYDvvgsBuHc+Ww1U2fqTIdAIZUyqUIJs3ef4ZR8+Z5/kzHi95igdy1Pm+uuIOp12w==");

            var fsClient = client.GetFileSystemClient("raw");
            var dirclient = fsClient.GetDirectoryClient(".");
            DataLakeFileClient fileClient = await dirclient.CreateFileAsync($"{companyEntity.RowKey}.csv");

            var fileStream = GenerateStreamFromString(quotes);
            await fileClient.AppendAsync(fileStream, 0);
            await fileClient.FlushAsync(fileStream.Length);
        }

        public static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static DataLakeServiceClient GetDataLakeServiceClient(string accountName, string accountKey)
        {
            StorageSharedKeyCredential sharedKeyCredential = new StorageSharedKeyCredential(accountName, accountKey);

            string dfsUri = "https://" + accountName + ".dfs.core.windows.net";

            var dataLakeServiceClient = new DataLakeServiceClient(new Uri(dfsUri), sharedKeyCredential);
            return dataLakeServiceClient;
        }

        private async Task<IEnumerable<CompanyEntity>> GetCompaniesAsync()
        {
            var tableClient = new TableClient(connectionString, tablename);
            await tableClient.CreateIfNotExistsAsync();
            Pageable<CompanyEntity> result = tableClient.Query<CompanyEntity>();

            return result;
        }

        private async Task<string> GetQuotesAsync(string ticker, DateTime from, DateTime to)
        {
            var fromDate = from.ToUnixTime();
            var toDate = to.ToUnixTime();

            var result = "";
            using (var client = new HttpClient())
            {
                var url = $"https://query1.finance.yahoo.com/v7/finance/download/{ticker}?period1={fromDate}&period2={toDate}&interval=1d&events=history&includeAdjustedClose=true";
                result = await client.GetStringAsync(url).ConfigureAwait(false);
            }

            return result;
        }
    }

    public static class TimeExtentions
    {
        public static DateTime FromUnixTime(this long unixTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(unixTime);
        }

        public static long ToUnixTime(this DateTime date)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64((date - epoch).TotalSeconds);
        }
    }
}