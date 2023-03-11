using Finance.Application.Models;

namespace Finance.Api.Services
{
    public interface IAzureTableService
    {
        Task<IEnumerable<CompanyEntity>> GetCompaniesAsync();

        Task AddCompanyAsync(string name, string ticker, string url);
    }
}
