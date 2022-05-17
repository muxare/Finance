using Finance.Api.Models;
using Finance.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Finance.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CompaniesController : ControllerBase
    {
        private IAzureTableService TableService { get; }

        public CompaniesController(IAzureTableService tableService)
        {
            TableService=tableService;
        }

        [HttpGet(Name = "GetCompanies")]
        public async Task<IEnumerable<CompanyEntity>> GetCompaniesAsync()
        {
            return await TableService.GetCompaniesAsync();
        }

        [HttpPost(Name = "AddCompany")]
        public async Task AddCompany(string name, string ticker, string url)
        {
            await TableService.AddCompanyAsync(name, ticker, url);
        }
    }
}