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
        public async Task SaveQuotes(string quotes, CompanyEntity companyEntity)
        {
            var client = GetDataLakeServiceClient("financequotestorage", "zF9IfHEw6C1FVGcoC07HngYDvvgsBuHc+Ww1U2fqTIdAIZUyqUIJs3ef4ZR8+Z5/kzHi95igdy1Pm+uuIOp12w==");

            var fsClient = client.GetFileSystemClient("raw");
            var dirclient = fsClient.GetDirectoryClient("eod");
            await dirclient.CreateIfNotExistsAsync();
            
            DataLakeFileClient fileClient = await dirclient.CreateFileAsync($"{companyEntity.RowKey}.eod.csv");
            var fileStream = GenerateStreamFromString(quotes);
            await fileClient.AppendAsync(fileStream, 0);
            await fileClient.FlushAsync(fileStream.Length);
        }

        public async Task<ICollection<string>> GetEodFilesAsync()
        {
            var client = GetDataLakeServiceClient("financequotestorage", "zF9IfHEw6C1FVGcoC07HngYDvvgsBuHc+Ww1U2fqTIdAIZUyqUIJs3ef4ZR8+Z5/kzHi95igdy1Pm+uuIOp12w==");
            var fsClient = client.GetFileSystemClient("raw");
            var dirClient = fsClient.GetDirectoryClient(".");

            var t = fsClient.GetPathsAsync("eod");
            List<string> result = new List<string>();
            await foreach (var secretProperties in t)
            {
                var r = await DownloadFile(dirClient, secretProperties.Name);
                result.Add(r);
            }

            return result;
        }

        private async Task<string> DownloadFile(DataLakeDirectoryClient dirClient, string fileName)
        {
            DataLakeFileClient fileClient =
                dirClient.GetFileClient(fileName);

            Response<FileDownloadInfo> downloadResponse = await fileClient.ReadAsync();

            StreamReader reader = new StreamReader(downloadResponse.Value.Content);



            return "";// GenerateStringFromByteArray(buffer);
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

        private static string GenerateStringFromByteArray(byte[] bytes)
        {
            return Encoding.Default.GetString(bytes);
        }
    }
}