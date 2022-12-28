using Azure;
using Azure.Storage;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Finance.Api.Models;
using System.Text;

namespace Finance.Api.Services
{
    public class AzureLakeService : IAzureLakeService
    {
        private const string AccountName = "financequotestorage";
        private const string AccountKey = "zF9IfHEw6C1FVGcoC07HngYDvvgsBuHc+Ww1U2fqTIdAIZUyqUIJs3ef4ZR8+Z5/kzHi95igdy1Pm+uuIOp12w==";

        public async Task SaveQuotes(string quotes, CompanyEntity companyEntity)
        {
            var client = GetDataLakeServiceClient(AccountName, AccountKey);

            var fsClient = client.GetFileSystemClient("raw");
            var dirClient = fsClient.GetDirectoryClient("eod");
            await dirClient.CreateIfNotExistsAsync();

            DataLakeFileClient fileClient = await dirClient.CreateFileAsync($"{companyEntity.RowKey}.eod.csv");
            var fileStream = GenerateStreamFromString(quotes);
            await fileClient.AppendAsync(fileStream, 0);
            await fileClient.FlushAsync(fileStream.Length);
        }

        public async Task SaveJson(string jsonString, CompanyEntity companyEntity)
        {
            var client = GetDataLakeServiceClient(AccountName, AccountKey);

            var fsClient = client.GetFileSystemClient("raw");
            var dirClient = fsClient.GetDirectoryClient("eod");
            await dirClient.CreateIfNotExistsAsync();

            DataLakeFileClient fileClient = await dirClient.CreateFileAsync($"{companyEntity.RowKey}.eod.json");
            var fileStream = GenerateStreamFromString(jsonString);
            await fileClient.AppendAsync(fileStream, 0);
            await fileClient.FlushAsync(fileStream.Length);
        }

        public async Task SaveJsonEma(string jsonString, CompanyEntity companyEntity)
        {
            var client = GetDataLakeServiceClient(AccountName, AccountKey);

            var fsClient = client.GetFileSystemClient("raw");
            var dirClient = fsClient.GetDirectoryClient("ema");
            await dirClient.CreateIfNotExistsAsync();

            DataLakeFileClient fileClient = await dirClient.CreateFileAsync($"{companyEntity.RowKey}.ema.json");
            var fileStream = GenerateStreamFromString(jsonString);
            await fileClient.AppendAsync(fileStream, 0);
            await fileClient.FlushAsync(fileStream.Length);
        }

        public async Task<ICollection<string>> GetEodFilesAsync()
        {
            var client = GetDataLakeServiceClient(AccountName, AccountKey);
            var fsClient = client.GetFileSystemClient("raw");
            var dirClient = fsClient.GetDirectoryClient(".");

            var t = fsClient.GetPathsAsync("eod");
            var result = new List<string>();
            await foreach (var secretProperties in t)
            {
                var r = await DownloadFile(dirClient, secretProperties.Name);
                result.Add(r);
            }

            return result;
        }

        public async Task<ICollection<CompanyContents<string>>> GetEmaFanFilesAsync(IEnumerable<CompanyEntity> companies)
        {
            var client = GetDataLakeServiceClient(AccountName, AccountKey);
            var fsClient = client.GetFileSystemClient("raw");
            var dirClient = fsClient.GetDirectoryClient(".");

            var t = fsClient.GetPathsAsync("ema");
            var result = new List<CompanyContents<string>>();
            await foreach (var secretProperties in t)
            {
                string companyGuid = secretProperties.Name.Split('/')[1].Split('.')[0];
                var r = await DownloadFile(dirClient, secretProperties.Name);
                result.Add(new CompanyContents<string>(companies.Where(c => c.RowKey == companyGuid).SingleOrDefault(), r));
            }

            return result;
        }

        private async Task<string> DownloadFile(DataLakeDirectoryClient dirClient, string fileName)
        {
            var fileClient = dirClient.GetFileClient(fileName);

            var downloadResponse = await fileClient.ReadAsync();
            var reader = new StreamReader(downloadResponse.Value.Content);

            return await reader.ReadToEndAsync();
        }

        private static DataLakeServiceClient GetDataLakeServiceClient(string accountName, string accountKey)
        {
            StorageSharedKeyCredential sharedKeyCredential = new StorageSharedKeyCredential(accountName, accountKey);

            string dfsUri = "https://" + accountName + ".dfs.core.windows.net";

            var dataLakeServiceClient = new DataLakeServiceClient(new Uri(dfsUri), sharedKeyCredential);
            return dataLakeServiceClient;
        }

        private static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
