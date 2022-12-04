using Finance.Api.Models;

namespace Finance.Api.Services
{
    public interface IQuoteImportService
    {
        Task<string> GetQuotesAsync(CompanyEntity company, DateTime from, DateTime to);

        Task<DownloadData[]> GetRawQuotesData();
    }
}
