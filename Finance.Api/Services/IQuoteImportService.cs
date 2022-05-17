namespace Finance.Api.Services
{
    public interface IQuoteImportService
    {
        Task<string> GetQuotesAsync(string ticker, DateTime from, DateTime to);
    }
}