using Finance.Api.Models;

namespace Finance.Api.Services
{
    public interface IAzureTableService
    {
        Task<IEnumerable<CompanyEntity>> GetCompaniesAsync();
    }
}