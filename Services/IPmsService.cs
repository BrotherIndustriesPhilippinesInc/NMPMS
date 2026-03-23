using Microsoft.AspNetCore.DataProtection.Repositories;
using NMPMS.Models;
using NMPMS.ViewModels;

namespace NMPMS.Services
{
    public interface IPmsService
    {
        Task CreateIssueAsync(CreateIssueViewModel model);
        Task<List<PmsRecord>> FetchAsync(string stage, string model);
    }
}
