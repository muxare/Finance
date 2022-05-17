using Finance.Api.Models;

namespace Finance.Api.Services
{
    public interface IAzureLakeService
    {
        Task SaveQuotes(string quotes, CompanyEntity companyEntity);
    }
}