using Azure;
using Azure.Data.Tables;
using Finance.Application.Models;

namespace Finance.Api.Services
{
    public class AzureTableService : IAzureTableService
    {
        private const string ConnectionString = "DefaultEndpointsProtocol=https;AccountName=financequotestorage;AccountKey=zF9IfHEw6C1FVGcoC07HngYDvvgsBuHc+Ww1U2fqTIdAIZUyqUIJs3ef4ZR8+Z5/kzHi95igdy1Pm+uuIOp12w==;EndpointSuffix=core.windows.net";
        private const string TableName = "Companies";

        public async Task AddCompanyAsync(string name, string ticker, string url)
        {
            var companyEntity = new CompanyEntity(name, ticker, url);

            var client = new TableClient(ConnectionString, TableName);
            await client.AddEntityAsync(companyEntity);
        }

        public async Task<IEnumerable<CompanyEntity>> GetCompaniesAsync()
        {
            var tableClient = new TableClient(ConnectionString, TableName);
            await tableClient.CreateIfNotExistsAsync();
            Pageable<CompanyEntity> result = tableClient.Query<CompanyEntity>();

            return result;
        }
    }
}
