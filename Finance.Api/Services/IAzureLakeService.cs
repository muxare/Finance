using Finance.Api.Models;

namespace Finance.Api.Services
{
    public interface IAzureLakeService
    {
        Task SaveQuotes(string quotes, CompanyEntity companyEntity);

        Task SaveJson(string jsonString, CompanyEntity companyEntity);

        Task SaveJsonEma(string jsonString, CompanyEntity companyEntity);

        Task<ICollection<string>> GetEodFilesAsync();

        Task<ICollection<string>> GetEmaFanFilesAsync();
    }
}
