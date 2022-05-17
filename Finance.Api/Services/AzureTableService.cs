using Azure;
using Azure.Data.Tables;
using Finance.Api.Models;

namespace Finance.Api.Services
{
    public class AzureTableService : IAzureTableService
    {
        private string connectionString = "DefaultEndpointsProtocol=https;AccountName=financequotestorage;AccountKey=zF9IfHEw6C1FVGcoC07HngYDvvgsBuHc+Ww1U2fqTIdAIZUyqUIJs3ef4ZR8+Z5/kzHi95igdy1Pm+uuIOp12w==;EndpointSuffix=core.windows.net";
        private string tablename = "Companies";

        public async Task<IEnumerable<CompanyEntity>> GetCompaniesAsync()
        {
            var tableClient = new TableClient(connectionString, tablename);
            await tableClient.CreateIfNotExistsAsync();
            Pageable<CompanyEntity> result = tableClient.Query<CompanyEntity>();

            return result;
        }
    }
}