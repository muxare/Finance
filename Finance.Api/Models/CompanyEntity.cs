using Azure;
using Azure.Data.Tables;

namespace Finance.Api.Models
{
    public class CompanyEntity : ITableEntity
    {
        public string? Name { get; set; }
        public string? Ticker { get; set; }
        public string? Url { get; set; }

        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
