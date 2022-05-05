using Microsoft.AspNetCore.Mvc;

namespace Finance.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class QuotesController : ControllerBase
    {
        // Use client for data lake to store quote data

        [HttpPost(Name = "StartImport")]
        public Task GetCompaniesAsync()
        {
            return Task.CompletedTask;
        }
    }
}