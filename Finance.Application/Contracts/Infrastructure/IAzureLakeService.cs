using Finance.Application.Models;

namespace Finance.Application.Contracts.Infrastructure
{
    public interface IAzureLakeService<T> where T : class
    {
        //Task SaveQuotes(string quotes, CompanyEntity companyEntity);
        Task SaveQuotes(string quotes, T companyEntity);

        Task SaveJson(string jsonString, T companyEntity);

        Task SaveJsonEma(string jsonString, T companyEntity);

        Task<ICollection<string>> GetEodFilesAsync();

        Task<ICollection<CompanyContents<string>>> GetEmaFanFilesAsync(IEnumerable<T> companies);
    }
}
