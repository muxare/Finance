using Azure;
using Azure.Data.Tables;

namespace Finance.Application.Models
{
    public class CompanyEntity : ITableEntity
    {
        public CompanyEntity()
        {
            PartitionKey = CompanyPartitionKey;
            RowKey = Guid.NewGuid().ToString();
        }

        public CompanyEntity(string name, string ticker, string url)
        {
            Name = name;
            Ticker = ticker;
            Url = url;
            PartitionKey = CompanyPartitionKey;
            RowKey = Guid.NewGuid().ToString();
        }

        public string? Name { get; set; }
        public string? Ticker { get; set; }
        public string? Url { get; set; }

        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        private const string CompanyPartitionKey = "d308f72a-cc29-47d0-812e-178351c30bc4";
    }

    public static class CompanyEntityExtentions
    {
        public static string StorageFileName(this CompanyEntity company)
        {
            return $"eod/{company.RowKey}.eod.json";
        }
    }
}
