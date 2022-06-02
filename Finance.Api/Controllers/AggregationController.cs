using Finance.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Finance.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AggregationController : ControllerBase
    {
        private IAzureLakeService LakeService { get; }
        
        public AggregationController(IAzureLakeService lakeService)
        {
            LakeService = lakeService;
        }
        
        [HttpPost("StartEmaCalculation")]
        public async Task StartEmaCalculation()
        {
            // Get all eod quote data files
            var res = LakeService.GetEodFilesAsync();

            // For each file
            // calculate ema 18, 50, 100, 200 and
            // store in separate file with original name and post _ema18, _ema50, _ema100 and _ema200

            return;
        }


    }
}