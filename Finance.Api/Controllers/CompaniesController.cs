using Azure;
using Azure.Data.Tables;
using Azure.Data.Tables.Models;
using Finance.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Finance.Api.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class CompaniesController : ControllerBase
    {
        private string CompanyPartitionKey = "d308f72a-cc29-47d0-812e-178351c30bc4";
        private string connectionString = "DefaultEndpointsProtocol=https;AccountName=financequotestorage;AccountKey=zF9IfHEw6C1FVGcoC07HngYDvvgsBuHc+Ww1U2fqTIdAIZUyqUIJs3ef4ZR8+Z5/kzHi95igdy1Pm+uuIOp12w==;EndpointSuffix=core.windows.net";
        private string tablename = "Companies";

        public CompaniesController()
        {
        }

        [HttpGet(Name = "GetCompanies")]
        public async Task<IEnumerable<CompanyEntity>> GetCompaniesAsync()
        {
            //var tableServiceClient = new TableServiceClient(connectionString);
            var tableClient = new TableClient(connectionString, tablename);
            await tableClient.CreateIfNotExistsAsync();
            Pageable<CompanyEntity> result = tableClient.Query<CompanyEntity>();

            return result;
        }

        [HttpPost(Name = "AddCompany")]
        public async Task AddCompany(string name, string ticker, string url)
        {
            var companyEntity = new CompanyEntity
            {
                Name = name,
                Ticker = ticker,
                Url = url,
                PartitionKey = CompanyPartitionKey,
                RowKey = Guid.NewGuid().ToString()
            };

            var client = new TableClient(connectionString, tablename);
            await client.AddEntityAsync<CompanyEntity>(companyEntity);
        }
    }
}