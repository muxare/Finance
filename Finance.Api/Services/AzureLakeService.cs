using Azure.Storage;
using Azure.Storage.Files.DataLake;
using Finance.Api.Models;

namespace Finance.Api.Services
{
    public class AzureLakeService : IAzureLakeService
    {
        public async Task SaveQuotes(string quotes, CompanyEntity companyEntity)
        {
            var client = GetDataLakeServiceClient("financequotestorage", "zF9IfHEw6C1FVGcoC07HngYDvvgsBuHc+Ww1U2fqTIdAIZUyqUIJs3ef4ZR8+Z5/kzHi95igdy1Pm+uuIOp12w==");

            var fsClient = client.GetFileSystemClient("raw");
            var dirclient = fsClient.GetDirectoryClient(".");
            DataLakeFileClient fileClient = await dirclient.CreateFileAsync($"{companyEntity.RowKey}.csv");

            var fileStream = GenerateStreamFromString(quotes);
            await fileClient.AppendAsync(fileStream, 0);
            await fileClient.FlushAsync(fileStream.Length);
        }

        public static DataLakeServiceClient GetDataLakeServiceClient(string accountName, string accountKey)
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