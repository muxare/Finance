using Finance.Application.Models;

namespace Finance.Application.Contracts.Infrastructure
{
    public interface IQuoteImportService
    {
        Task<string> GetQuotesAsync(CompanyEntity company, DateTime from, DateTime to);

        Task<DownloadData[]> GetRawQuotesData();
    }
}
