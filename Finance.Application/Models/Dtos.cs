using Azure.Storage.Blobs.Models;
using Azure;

namespace Finance.Application.Models
{
    public readonly record struct DatedValue(DateTime Date, double Value);
    public readonly record struct QuoteDto(DateTime Date, double Open, double High, double Low, double Close, int Volume, int OpenInt);
    public readonly record struct QuoteDtoYahoo(DateTime Date, double? Open, double? High, double? Low, double? Close, double? AdjClose, long? Volume);
    public readonly record struct DownloadData(string Name, Response<BlobDownloadResult> Content);
    public readonly record struct CompanyContents<T>(CompanyEntity Company, T Content);
}
