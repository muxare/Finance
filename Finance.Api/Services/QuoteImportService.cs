using Finance.Api.Extentions;

namespace Finance.Api.Services
{
    public class QuoteImportService : IQuoteImportService
    {
        public async Task<string> GetQuotesAsync(string ticker, DateTime from, DateTime to)
        {
            var fromDate = from.ToUnixTime();
            var toDate = to.ToUnixTime();

            var result = "";
            using (var client = new HttpClient())
            {
                var url = $"https://query1.finance.yahoo.com/v7/finance/download/{ticker}?period1={fromDate}&period2={toDate}&interval=1d&events=history&includeAdjustedClose=true";
                result = await client.GetStringAsync(url).ConfigureAwait(false);
            }

            return result;
        }
    }
}