using Finance.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Finance.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class QuotesController : ControllerBase
    {
        private IAzureTableService TableService { get; }
        private IQuoteImportService ImportService { get; }
        public IAzureLakeService LakeService { get; }

        public QuotesController(IAzureTableService tableService, IQuoteImportService importService, IAzureLakeService lakeService)
        {
            TableService=tableService;
            ImportService=importService;
            LakeService=lakeService;
        }

        [HttpPost(Name = "StartImport")]
        public async Task StartImportAsync()
        {
            var companies = await TableService.GetCompaniesAsync();
            foreach (var company in companies)
            {
                var quotes = await ImportService.GetQuotesAsync(company.Ticker, DateTime.MinValue, DateTime.UtcNow);

                await LakeService.SaveQuotes(quotes, company);
            }

            return;
        }
    }
}